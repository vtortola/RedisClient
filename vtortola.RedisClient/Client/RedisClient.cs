using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    /// <summary>
    /// RedisClient provides dual channels (<see cref="IRedisChannel"/>) to access Redis. 
    /// </summary>
    public sealed class RedisClient : IDisposable
    {
        readonly IPEndPoint[] _endpoints;
        readonly RedisClientOptions _options;
        readonly ProcedureCollection _procedures;
        readonly ICommandConnection _multiplexedCommander;
        readonly IConnectionProvider<ISubscriptionConnection> _subscriptorsPool;
        readonly IConnectionProvider<ICommandConnection> _exclusivePool;
        readonly IExecutionPlanner _planner;
        readonly IRedisClientLog _logger;

        CancellationTokenSource _cancel;

        private RedisClient(RedisClientOptions options)
        {
            _cancel = new CancellationTokenSource();
            _options = options != null? options.Clone() : RedisClientOptions.Default;
            _logger = _options.Logger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisClient"/> class.
        /// </summary>
        /// <param name="endpoints">The Redis endpoints. The selected endpoint is selected in a rota basis.</param>
        /// <param name="options"><see cref="RedisClientOptions"/></param>
        public RedisClient(IPEndPoint[] endpoints, RedisClientOptions options = null)
            : this(options)
        {
            ParameterGuard.CannotBeNullOrEmpty(endpoints, "endpoints");

            _endpoints = endpoints.ToArray();

            _procedures = _options.Procedures != null ? _options.Procedures.ToCollection() : ProcedureCollection.Empty;

            _multiplexedCommander = new AggregatedCommandConnection<RedisCommanderConnection>(_options.MultiplexPool.CommandConnections, CommanderFactory, _options, _procedures);
            _subscriptorsPool = new ConnectionSelector<RedisSubscriberConnection>(_options.MultiplexPool.SubscriptionOptions, SubscriberFactory, _options);

            if (_options.ExclusivePool.Maximum > 0)
                _exclusivePool = new ConnectionPool(_options.ExclusivePool.Minimum, _options.ExclusivePool.Maximum, CommanderFactory, _options.Logger);
            else
                _exclusivePool = DisabledConnectionPool.Instance;

            IExecutionPlanner planner = new ExecutionPlanner(_procedures);
            _planner = _options.UseExecutionPlanCaching ? new CachingExecutionPlanner(planner) : planner;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisClient"/> class.
        /// </summary>
        /// <param name="endpoint">The Redis endpoint.</param>
        /// <param name="options"><see cref="RedisClientOptions"/></param>
        public RedisClient(IPEndPoint endpoint, RedisClientOptions options = null)
            : this(new[] { endpoint }, options)
        {
            ParameterGuard.CannotBeNull(endpoint, "endpoint");

            _endpoints = new[] { endpoint };
        }

        private RedisCommanderConnection CommanderFactory()
        {
            return new RedisCommanderConnection(_endpoints, _options, _procedures);
        }

        private RedisSubscriberConnection SubscriberFactory()
        {
            return new RedisSubscriberConnection(_endpoints, _options);
        }

        /// <summary>
        /// Connects to Redis.
        /// </summary>
        /// <param name="cancel">A token to cancel the operation.</param>
        public async Task ConnectAsync(CancellationToken cancel)
        {
            _cancel.Cancel();
            _cancel.Dispose();
            _cancel = new CancellationTokenSource();
            _logger.Info("Initiating connections to Redis...");
            using (cancel.Register(_cancel.Cancel))
            {
                await Task.WhenAll(_multiplexedCommander.ConnectAsync(_cancel.Token),
                    _subscriptorsPool.ConnectAsync(_cancel.Token),
                    _exclusivePool.ConnectAsync(_cancel.Token)
                    ).ConfigureAwait(false);
            }
            _logger.Info("Connections to Redis initiated.");
        }

        /// <summary>
        /// Creates a <see cref="IRedisChannel"/> that provides seamless access to both data and subscription connections.
        /// </summary>
        /// <returns><see cref="IRedisChannel"/></returns>
        public IRedisChannel CreateChannel()
        {
            return new RedisChannel(_planner, _procedures, _multiplexedCommander, _subscriptorsPool, _exclusivePool);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _cancel.Cancel();
            DisposeHelper.SafeDispose(_cancel);
            DisposeHelper.SafeDispose(_multiplexedCommander);
            DisposeHelper.SafeDispose(_subscriptorsPool);
        }
    }
}
