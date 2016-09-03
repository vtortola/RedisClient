using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    internal abstract class ConcurrentConnection : ConnectionBase, ILoadMeasurable
    {
        readonly BlockingCollection<ExecutionToken> _requests;
        readonly RedisClientOptions _options;

        public abstract Int32 CurrentLoad { get; }
        protected Int32 LoadFactor { get; private set; }

        internal ConcurrentConnection(IPEndPoint[] endpoints, RedisClientOptions options)
            :base(endpoints, options)
        {
            _requests = TokenHandling.CreateQueue<ExecutionToken>(options.QueuesBoundedCapacity);
            _options = options;
            LoadFactor = 10;
        }

        protected sealed override void ExecuteToken(ExecutionToken token, CancellationToken cancel)
        {
            TokenHandling.ProcessToken(token, _requests.Add, cancel);
        }

        protected sealed override Task[] RunConnectionTasks(CancellationToken cancel)
        {
            var baseTasks = base.RunConnectionTasks(cancel);
            var tasks = new Task[baseTasks.Length + 1];
            Array.Copy(baseTasks, tasks, baseTasks.Length);
            tasks[baseTasks.Length] = Task.Factory.StartNew(() => TokenHandling.ProcessTokensLoop(_requests, base.ExecuteToken, cancel), TaskCreationOptions.LongRunning);
            return tasks;
        }   

        protected override void OnConnection()
        {
            base.OnConnection();
            // restore load factor to normal level
            LoadFactor = 1; 
        }

        protected override void OnDisconnection()
        {
            base.OnDisconnection();
            // make this instance less likely for be choosen
            LoadFactor = 10;
        }


        protected override void Dispose(Boolean disposing)
        {
            base.Dispose(disposing);
            _requests.CancelTokens();
            _requests.Dispose();
        }
    }
}
