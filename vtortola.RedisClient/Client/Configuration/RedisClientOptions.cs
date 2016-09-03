using System;
using System.Collections.Generic;
using System.Threading;

namespace vtortola.Redis
{
    /// <summary>
    /// Configuration options for <see cref="RedisClient"/>
    /// </summary>
    public sealed class RedisClientOptions
    {
        /// <summary>
        /// Multiplexed pool options.
        /// </summary>
        public MultiplexPoolOptions MultiplexPool { get; private set; }

        /// <summary>
        /// Exclusive pool options.
        /// </summary>
        public ExclusivePoolOptions ExclusivePool { get; private set; }

        /// <summary>
        /// Gets or sets the maximum waiting time for a PING response.
        /// Use <see cref="Timeout.InfiniteTimeSpan"/> to diable PING.
        /// Default <c>Timeout.InfiniteTimeSpan</c>.
        /// </summary>
        public TimeSpan PingTimeout { get;  set; }

        /// <summary>
        /// Gets or sets the maximum waiting time for the connection to complete.
        /// Default <c>5 seconds</c>.
        /// </summary>
        public TimeSpan ConnectionTimeout { get; set; }

        /// <summary>
        /// Size of the buffer used for socket reading.
        /// Default <c>8192</c>.
        /// </summary>
        public Int32 ReadBufferSize { get; set; }

        /// <summary>
        /// Size of the buffer used for socket writing.
        /// Default <c>8192</c>.
        /// </summary>
        public Int32 WriteBufferSize { get; set; }

        /// <summary>
        /// Capacity of the internal queues used for
        /// pipelining.
        /// By default is unset.
        /// </summary>
        public Int32? QueuesBoundedCapacity { get; set; }

        /// <summary>
        /// Enables or disables the execution plan caching.
        /// Disabling this functionality may reduce performance dramatically.
        /// Default <c>true</c>.
        /// </summary>
        public Boolean UseExecutionPlanCaching { get; set; }

        /// <summary>
        /// Enables or disables the use of the Nagle algorithm in sockets. By default is unset (system default).
        /// </summary>
        public Boolean? UseNagleAlgorithm { get; set; }

        /// <summary>
        /// Avoid sending PING if there is already activity in the socket. Default <c>true</c>.
        /// </summary>
        public Boolean PreventPingIfActive { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="IRedisClientLog"/> used for logging connection information.
        /// </summary>
        public IRedisClientLog Logger { get; set; }
        
        List<PreInitializationCommand> _initializationCmds;
        /// <summary>
        /// Sets a list of commands that will be executed right after connecting in each connection.
        /// </summary>
        public List<PreInitializationCommand> InitializationCommands 
        {
            get
            {
                CreateIfNotExists(ref _initializationCmds, () => new List<PreInitializationCommand>());
                return _initializationCmds;
            }
        }

        ProcedureLoader _procedureLoader;
        /// <summary>
        /// Loads RedisClient procedures.
        /// </summary>
        public ProcedureLoader Procedures
        {
            get
            {
                CreateIfNotExists(ref _procedureLoader, () => new ProcedureLoader());
                return _procedureLoader;
            }
        }

        static void CreateIfNotExists<T>(ref T variable, Func<T> factory)
            where T:class
        {
            if (variable == null)
            {
                var temp = factory();
                Interlocked.CompareExchange(ref variable, temp, null);
            }
        }

        internal ILoadBasedSelector LoadBasedSelector { get; set; }

        internal static readonly RedisClientOptions Default = new RedisClientOptions();
        
        /// <summary>
        /// Initializes an instance of <see cref="RedisClientOptions"/> with the default configuration.
        /// </summary>
        public RedisClientOptions()
        {
            PingTimeout = Timeout.InfiniteTimeSpan;
            ConnectionTimeout = TimeSpan.FromSeconds(5);
            ReadBufferSize = 8192;
            WriteBufferSize = 8192;
            UseExecutionPlanCaching = true;
            LoadBasedSelector = new BasicLoadBasedSelector();
            PreventPingIfActive = true;
            Logger = NoLogger.Instance;
            var multiplexPool = new MultiplexPoolOptions();
            multiplexPool.CommandConnection = 2;
            multiplexPool.SubscriptionOptions = 2;
            MultiplexPool = multiplexPool;
            var exclusivePool = new ExclusivePoolOptions();
            exclusivePool.Minimum = 10;
            exclusivePool.Maximum = 50;
            ExclusivePool = exclusivePool;
        }

        internal RedisClientOptions Clone()
        {
            try
            {
                ValidateMultiplex(this.MultiplexPool);
                ValidateExclusive(this.ExclusivePool);


                var clone = new RedisClientOptions()
                {
                    PingTimeout = this.PingTimeout,
                    ReadBufferSize = this.ReadBufferSize,
                    WriteBufferSize = this.WriteBufferSize,
                    QueuesBoundedCapacity = this.QueuesBoundedCapacity,
                    _procedureLoader = this.Procedures,
                    UseExecutionPlanCaching = this.UseExecutionPlanCaching,
                    LoadBasedSelector = this.LoadBasedSelector ?? new BasicLoadBasedSelector(),
                    UseNagleAlgorithm = this.UseNagleAlgorithm,
                    PreventPingIfActive = this.PreventPingIfActive,
                    Logger = this.Logger ?? NoLogger.Instance,
                    MultiplexPool = this.MultiplexPool,
                    ExclusivePool = this.ExclusivePool,
                    ConnectionTimeout = this.ConnectionTimeout
                };
                if (_initializationCmds != null)
                    clone._initializationCmds = new List<PreInitializationCommand>(_initializationCmds);
                return clone;
            }
            catch (Exception ex)
            {
                throw new RedisClientConfigurationException(ex.Message, ex);
            }
        }

        private void ValidateExclusive(ExclusivePoolOptions config)
        {
            ParameterGuard.CannotBeZeroOrNegative(config.Minimum, "ExclusivePoolOptions.Minimum");
            ParameterGuard.MustBeBiggerOrEqualThan(config.Maximum, "ExclusivePoolOptions.Maximum", config.Minimum);
        }

        private void ValidateMultiplex(MultiplexPoolOptions config)
        {
            ParameterGuard.CannotBeZeroOrNegative(config.CommandConnection, "MultiplexPoolOptions.CommandConnection");
            ParameterGuard.CannotBeZeroOrNegative(config.SubscriptionOptions, "MultiplexPoolOptions.SubscriptionOptions");
        }
    }
}
