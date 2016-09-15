using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace vtortola.Redis
{
    internal abstract class RedisConnection : IConnection, ILoadMeasurable
    {
        readonly IPEndPoint[] _endpoints;
        readonly RedisClientOptions _options;
        readonly BlockingCollection<ExecutionToken> _requests;
        readonly Timer _pingTimer;
        readonly TimeSpan _interval;
        readonly Boolean _skipTimerReset;
        readonly IRedisClientLog _logger;
        readonly String _code;

        protected readonly CancellationTokenSource _connectionCancellation;
        protected readonly ConcurrentQueue<ExecutionToken> _pending;

        Int32 _currentEndpoint;
        Int32 _loadFactor;
        DateTime _lastActivity;
        
        protected List<IConnectionInitializer> Initializers { get; private set; }
        public Int32 CurrentLoad { get { return (1 + _pending.Count) * _loadFactor; } }

        internal RedisConnection(IPEndPoint[] endpoints, RedisClientOptions options)
        {
            _loadFactor = 100;
            _endpoints = endpoints;
            _options = options;
            _connectionCancellation = new CancellationTokenSource();
            _requests = options.QueuesBoundedCapacity.HasValue ? new BlockingCollection<ExecutionToken>(options.QueuesBoundedCapacity.Value) : new BlockingCollection<ExecutionToken>();
            _pending = new ConcurrentQueue<ExecutionToken>();
            _logger = _options.Logger;
            _code = this.GetHashCode().ToString();
            Initializers = new List<IConnectionInitializer>();
            Initializers.Add(new ConnectionInitializer(_options));
            
            if(options.PingTimeout != Timeout.InfiniteTimeSpan)
            {
                _interval = TimeSpan.FromMilliseconds(options.PingTimeout.TotalMilliseconds / 2);
                _pingTimer = new Timer(o => { try { Execute(GeneratePingToken(), CancellationToken.None); } catch (Exception) { } });
                _skipTimerReset = !options.PreventPingIfActive;
            }
            else
            {
                _skipTimerReset = true;
            }
            _logger.Info("Created connection {0} of type {1}.", _code, this.GetType().Name);
        }

        public async Task ConnectAsync(CancellationToken cancel)
        {
            using (cancel.Register(_connectionCancellation.Cancel))
            {
                var tcp = new TcpClient();
                await ConnectWithTimeOut(tcp, _endpoints[_currentEndpoint]).ConfigureAwait(false);
                Task.Run(() => ConnectionWatchDog(tcp));                
            }
        }

        private async Task ConnectWithTimeOut(TcpClient tcp, IPEndPoint endpoint)
        {
            _logger.Info("Connection {0} launching...", _code);

            if (_options.UseNagleAlgorithm.HasValue)
                tcp.NoDelay = !_options.UseNagleAlgorithm.Value;

            var timeout = Task.Delay(_options.ConnectionTimeout, _connectionCancellation.Token);
            var connecting = tcp.ConnectAsync(endpoint.Address, endpoint.Port);

            await Task.WhenAny(timeout, connecting).ConfigureAwait(false);

            if (timeout.IsCompleted)
                throw new SocketException(10053); // https://msdn.microsoft.com/en-us/library/windows/desktop/ms740668(v=vs.85).aspx

            if (timeout.IsCanceled)
                throw new TaskCanceledException();

            if (connecting.IsFaulted)
                throw connecting.Exception;

            _logger.Info("Connection {0} established.", _code);
        }

        private void RunInitializers(SocketReader reader, SocketWriter writer)
        {
            foreach (var initializer in Initializers)
                initializer.Initialize(reader, writer);
        }

        private async Task ConnectionWatchDog(TcpClient tcp)
        {
            while(!_connectionCancellation.IsCancellationRequested)
            {
                try
                {
                    using (tcp)
                    using (var nstream = tcp.GetStream())
                    using (var writer = new SocketWriter(nstream, _options.WriteBufferSize))
                    using (var reader = new SocketReader(nstream, _options.ReadBufferSize))
                    {
                        RunInitializers(reader, writer);

                        var tasks = new[]
                        {
                            Task.Run(()=> ReadAsync(reader)),
                            Task.Run(()=> WriteAsync(writer)),
                            _options.PingTimeout != Timeout.InfiniteTimeSpan ? Task.Run(()=> TimeoutWatchdogAsync()) : new Task(()=>{})
                        };

                        if(!_skipTimerReset)
                            _pingTimer.Change(_interval, _interval); // start timer

                        _loadFactor = 1;
                        OnConnection();

                        await Task.WhenAny(tasks).ConfigureAwait(false);
                        ThrowSocketExceptionIfExists(tasks);
                    }
                }
                catch(SocketException soex)
                {
                    // rotate endpoint
                    _currentEndpoint = (_currentEndpoint + 1) % _endpoints.Length;
                    _logger.Error(soex, "Connection {0} error. Switching endpoing.", _code);
                }
                catch(Exception ex)
                {
                    _logger.Error(ex, "Connection {0} error.", _code);
                }

                _logger.Info("Connection {0} disconnected.", _code);
                _loadFactor = 100;
                _pending.CancelTokens();
                OnDisconnection();

                if (_connectionCancellation.IsCancellationRequested)
                    continue;

                tcp = new TcpClient();
                await ConnectWithTimeOut(tcp,  _endpoints[_currentEndpoint]).ConfigureAwait(false);
            }
        }

        static void ThrowSocketExceptionIfExists(Task[] tasks)
        {
            Contract.Assert(tasks.Any(), "Checking SocketException in an empty task list.");

            Exception error = null;

            foreach (var task in tasks)
            {
                if (!task.IsFaulted)
                    continue;

                var terror = task.Exception.GetBaseException();

                if (terror is SocketException)
                    throw terror;
                else if (error == null && !(terror is OperationCanceledException))
                    error = terror;
            }

            if (error != null)
                throw error;
        }

        public void Execute(ExecutionToken token, CancellationToken cancel)
        {
            try
            {
                _requests.Add(token, cancel);
            }
            catch(OperationCanceledException)
            {
                token.SetCancelled();
            }
            catch(Exception ex)
            {
                token.SetFaulted(ex);
            }
        }

        private async Task ReadAsync(SocketReader reader)
        {
            try
            {
                while (!_connectionCancellation.IsCancellationRequested)
                {
                    _logger.Debug("{0} listening...", _code);
                    var resp = await RESPObject.ReadAsync(reader, _connectionCancellation.Token).ConfigureAwait(false);
                    _logger.Debug("{0} Received response of type {1}.", _code, resp.Header);
                    RegisterActivity();
                    ProcessResponse(resp);
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (IOException) { }
        }

        private async Task WriteAsync(SocketWriter writer)
        {
            try
            {
                while (!_connectionCancellation.IsCancellationRequested)
                {
                    var token = _requests.Take(_connectionCancellation.Token);

                    if (token == null || token.IsCancelled)
                        return;

                    WriteToken(writer, token);
                }
            }
            catch (OperationCanceledException) { }
            catch (InvalidOperationException) { }
            catch (ArgumentNullException) { }
        }

        private void WriteToken(SocketWriter writer, ExecutionToken token)
        {
            try
            {
                _logger.Debug("{0} Received token {1}.", _code, token);

                var hasCommands = false;

                foreach (var command in ExecuteOperation(token))
                {
                    // some subscription commands are aggregated
                    // and produce no commands.
                    if (!hasCommands)
                    {
                        hasCommands = true;
                        _pending.Enqueue(token);
                    }
                    command.WriteTo(writer);
                }

                if (hasCommands)
                {
                    _logger.Debug("{0} flushed buffer.", _code);
                }
            }
            catch(OperationCanceledException)
            {
                token.SetCancelled();
                throw;
            }
            catch (Exception ex)
            {
                token.SetFaulted(ex);
                throw;
            }
        }

        private async Task TimeoutWatchdogAsync()
        {
            _lastActivity = DateTime.Now;
            while (!_connectionCancellation.IsCancellationRequested)
            {
                await Task.Delay(_options.PingTimeout, _connectionCancellation.Token).ConfigureAwait(false);

                Contract.Assert(_options.PingTimeout != Timeout.InfiniteTimeSpan, "Ping timeout woke up from infinite.");

                var now = DateTime.Now;
                if (now - _lastActivity > _options.PingTimeout)
                {
                    _logger.Info("Connection {0} is being disconnected due ping timeout.", _code);
                    return;
                }
            }
        }

        private void RegisterActivity()
        {
            if (!_skipTimerReset)
                _pingTimer.Change(_interval, _interval); // restart ping timer

            _lastActivity = DateTime.Now;
        }

        protected abstract void ProcessResponse(RESPObject response);
        protected abstract IEnumerable<RESPCommand> ExecuteOperation(ExecutionToken token);
        protected internal abstract ExecutionToken GeneratePingToken();
        protected virtual void OnConnection(){}
        protected virtual void OnDisconnection() { }

        public void Dispose()
        {
            _connectionCancellation.Cancel();
            _connectionCancellation.Dispose();
            _pending.CancelTokens();
            _requests.CancelTokens();
            _requests.Dispose();
        }
    }
}
