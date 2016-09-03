using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    internal abstract class ConnectionBase : IConnection
    {
        readonly static TimeSpan _zeroDelay = TimeSpan.FromSeconds(0);

        readonly ConcurrentQueue<ExecutionToken> _pending;
        readonly RedisClientOptions _options;
        readonly IPEndPoint[] _endpoints;
        readonly TaskCompletionSource<Object> _connected;
        readonly IRedisClientLog _logger;

        readonly String _code;
        readonly Timer _pingTimer;
        readonly TimeSpan _interval;
        readonly Boolean _skipTimerReset;

        protected List<IConnectionInitializer> Initializers { get; private set; }
        protected Int32 PendingCount { get { return _pending.Count; } }

        Int32 _connectingFlag = 0;
        DateTime _lastActivity;

        TcpClient _client;
        SocketWriter _writer;
        SocketReader _reader;

        CancellationTokenSource _cancel;

        internal ConnectionBase(IPEndPoint[] endpoints, RedisClientOptions options)
        {
            Contract.Assert(endpoints.Any(), "Creating connection with empty list of endpoints.");
            Contract.Assert(options != null, "RedisClientOptions is null.");

            _code = this.GetHashCode().ToString();
            _endpoints = endpoints;
            _options = options;
            _pending = new ConcurrentQueue<ExecutionToken>();
            Initializers = new List<IConnectionInitializer>();
            Initializers.Add(new ConnectionInitializer(_options));
            _connected = new TaskCompletionSource<Object>();
            _logger = options.Logger;

            _interval = options.PingTimeout == Timeout.InfiniteTimeSpan 
                            ? Timeout.InfiniteTimeSpan 
                            : TimeSpan.FromMilliseconds(options.PingTimeout.TotalMilliseconds / 2);

            _pingTimer = new Timer(o => { try { Execute(GeneratePingToken(), CancellationToken.None); } catch (Exception) { } });

            _skipTimerReset = !options.PreventPingIfActive || options.PingTimeout == Timeout.InfiniteTimeSpan;

            _cancel = new CancellationTokenSource();
        }

        public Task ConnectAsync(CancellationToken cancel)
        {
            _cancel.Cancel();
            _cancel.Dispose();
            _cancel = new CancellationTokenSource();

            // Once "Connect" has been called, it auto reconnects each time 
            // it looses connection.
            if (Interlocked.CompareExchange(ref _connectingFlag, 1, 0) == 0)
            {
                using(cancel.Register(_cancel.Cancel))
                    Task.Run(() => RunInSafeLoop(_cancel.Token));
            }

            return _connected.Task;
        }

        private async Task RunInSafeLoop(CancellationToken cancel)
        {
            var currentEndpoint = 0;
            while (!cancel.IsCancellationRequested)
            {
                try
                {
                    _logger.Info("Running {1} connection {0}...", _code, this.GetType().Name);
                    await RunConnection(_endpoints[currentEndpoint], cancel).ConfigureAwait(false);
                    _logger.Info("Connection {0} ended gracefully.", _code);
                }
                catch (SocketException soex)
                {
                    _logger.Error(soex, "Connection {0} ended in SocketException: {1}", _code, soex.Message);
                    currentEndpoint = (currentEndpoint + 1) % _endpoints.Length;
    
                    Task.Run(() => _connected.TrySetException(soex));
                }
                catch(OperationCanceledException)
                {
                    Task.Run(() => _connected.TrySetCanceled());
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Connection {0} ended in error : {1}", _code, ex.Message);
                    Task.Run(() => _connected.TrySetException(ex));
                }
                finally
                {
                    OnDisconnection();
                }
            }
        }

        private async Task RunConnection(IPEndPoint endpoint, CancellationToken cancel)
        {
            using (_client = new TcpClient())
            {
                if (_options.UseNagleAlgorithm.HasValue)
                    _client.NoDelay = !_options.UseNagleAlgorithm.Value;

                var timeout = Task.Delay(_options.ConnectionTimeout);
                var connecting = _client.ConnectAsync(endpoint.Address, endpoint.Port);

                await Task.WhenAny(timeout, connecting).ConfigureAwait(false);

                if (timeout.IsCompleted)
                    throw new SocketException(10053); // https://msdn.microsoft.com/en-us/library/windows/desktop/ms740668(v=vs.85).aspx

                _logger.Info("Connection {0} established socket to {1}.", _code, endpoint);

                using ( var stream = _client.GetStream())
                using ( _writer = new SocketWriter(stream, _options.WriteBufferSize))
                using ( _reader = new SocketReader(stream, _options.ReadBufferSize))
                {
                    _logger.Info("Connection {0} executing initializers...", _code);

                    foreach (var initializer in Initializers)
                        initializer.Initialize(_reader, _writer);

                    _pingTimer.Change(_interval, _interval); // start timer

                    var tasks = RunConnectionTasks(cancel);

                    _logger.Info("Connection {0} running.", _code);

                    Task.Run(() => _connected.TrySetResult(null));

                    OnConnection();

                    await Task.WhenAny(tasks).ConfigureAwait(false);

                    _logger.Info("Connection {0} tasks ended: {1}", _code, String.Join(", ", tasks.Select(t => GetStatus(t))));

                    ThrowSocketExceptionIfExists(tasks);
                }
            }
        }

        public void Execute(ExecutionToken token, CancellationToken cancel)
        {
            TokenHandling.ProcessToken(token, ExecuteToken, cancel);
        }

        protected virtual void ExecuteToken(ExecutionToken token, CancellationToken cancel)
        {
            if (token.IsCancelled)
                return;

            var hascommands = false;

            foreach (var command in ExecuteOperation(token, cancel))
            {
                if (!hascommands)
                {
                    // If it is going to write in the socket, enqueue the token first
                    // to make sure that by the time the read operation gets the RESPObject
                    // there is an actual token in the queue.
                    _pending.Enqueue(token);
                    hascommands = true;
                }
                command.WriteTo(_writer);
            }

            if (hascommands)
                _writer.Flush();
        }

        protected virtual void OnConnection() { }

        protected virtual void OnDisconnection() 
        {
            _pending.CancelTokens();
        }

        protected abstract IEnumerable<RESPCommand> ExecuteOperation(ExecutionToken token, CancellationToken cancel);

        protected abstract void ProcessResponse(RESPObject obj, ConcurrentQueue<ExecutionToken> pending, CancellationToken cancel);

        protected internal abstract ExecutionToken GeneratePingToken();

        protected virtual Task[] RunConnectionTasks(CancellationToken cancel)
        {
            var tasks = new Task[2];
            // StartNew does not give 1 fuck about your method returning a Task
            tasks[0] = Task.Factory.StartNew(() => ProcessResponses(cancel), TaskCreationOptions.LongRunning);
            tasks[1] = Task.Run(() => TimeoutWatchdogAsync(cancel));
            return tasks;
        }

        private void ProcessResponses(CancellationToken cancel)
        {
            try
            {
                while (!cancel.IsCancellationRequested && _client.Connected)
                {
                    //var resp = await RESPObject.ReadAsync(_reader, cancel).ConfigureAwait(false);
                    var resp = RESPObject.Read(_reader);
                    RegisterActivity();
                    ProcessResponse(resp, _pending, cancel);
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (IOException) { }
            catch (NullReferenceException) { }
        }

        static String GetStatus(Task task)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                    var error = task.Exception.GetBaseException();
                    Contract.Assert(error is ThreadAbortException, "Connection task ended by unexpected exception.");
                    return "Faulted: (" + error.GetType().Name + ") " + error.Message;
                case TaskStatus.RanToCompletion:
                    return "Success";
                default:
                    return task.Status.ToString();
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

        private async Task TimeoutWatchdogAsync(CancellationToken cancel)
        {
            _lastActivity = DateTime.Now;
            while (!cancel.IsCancellationRequested)
            {
                await Task.Delay(_options.PingTimeout, cancel).ConfigureAwait(false);

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

        protected virtual void Dispose(Boolean disposing)
        {
            _cancel.Cancel();
            _cancel.Dispose();
            _pending.CancelTokens();
            _pingTimer.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
