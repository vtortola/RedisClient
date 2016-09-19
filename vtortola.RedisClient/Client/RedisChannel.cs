using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    [DebuggerDisplay("Channel: {GetHashCode()}")]
    internal sealed class RedisChannel : IRedisChannel
    {
        readonly ICommandConnection _multiplexedCommander;
        readonly IConnectionProvider<ICommandConnection> _exclusivePool;
        readonly IConnectionProvider<ISubscriptionConnection> _subscribers;
        readonly IExecutionPlanner _planner;
        readonly ProcedureCollection _procedures;
        readonly ExecutionContext _context;

        ICommandConnection _heldConnection;
        ISubscriptionConnection _subscriber;
        Boolean _disposed;

        public RedisMessageHandler NotificationHandler { get; set; }

        internal RedisChannel(IExecutionPlanner planner, 
                              ProcedureCollection procedures, 
                              ICommandConnection multiplexedCommander,
                              IConnectionProvider<ISubscriptionConnection> subscribers, 
                              IConnectionProvider<ICommandConnection> exclusivePool)
        {
            _multiplexedCommander = multiplexedCommander;
            _exclusivePool = exclusivePool;
            _subscribers = subscribers;
            _planner = planner;
            _procedures = procedures;
            _context = new ExecutionContext();
        }

        private Tuple<ICommandOperation, ISubscriptionOperation, RESPObject[]> Pack<T>(ExecutionPlan plan, T parameters)
            where T:class
        {
            var respCommand = plan.Bind(parameters);
            var responsesPlaceholder = new RESPObject[respCommand.Length];

            ICommandOperation commandOperation = null;
            ISubscriptionOperation subOp = null;

            var hasSubscriptions = false;
            var hasCommands = false;
            IdentifyOperationMainRoute(respCommand, ref hasCommands, ref hasSubscriptions);

            if (!hasCommands && !hasSubscriptions)
                throw new RedisClientParsingException("The given RESP commands do not contain Redis commands.");

            if (hasCommands)
                commandOperation = new CommandOperation(respCommand, responsesPlaceholder, _procedures);

            if (hasSubscriptions)
            {
                if (_subscriber == null)
                    _subscriber = _subscribers.Provide();
                subOp = new SubscriptionOperation(this, respCommand, responsesPlaceholder, _subscriber.Subscriptions);
            }

            return new Tuple<ICommandOperation, ISubscriptionOperation, RESPObject[]>(commandOperation, subOp, responsesPlaceholder);
        }

        static void IdentifyOperationMainRoute(RESPCommand[] commands, ref Boolean hasCommands, ref Boolean hasSubscriptions)
        {
            hasSubscriptions = false;
            hasCommands = false;
            for (var i = 0; i < commands.Length; i++)
            {
                var command = commands[i];

                hasSubscriptions |= command.IsSubscription;
                hasCommands |= !command.IsSubscription;

                if (hasSubscriptions && hasCommands)
                    break;
            }
        }

        private void Route(ExecutionPlan plan, ExecutionToken token, CancellationToken cancel)
        {
            if (token.IsCancelled)
                return;

            if (token.SubscriptionOperation != null)
                _subscriber.Execute(token, cancel);

            if (token.CommandOperation != null)
            {
                if (plan.ContextOperation.RequiresStandaloneConnection && _heldConnection == null)
                    _heldConnection = _exclusivePool.Provide();

                if (_heldConnection != null)
                    _heldConnection.Execute(token, cancel);
                else
                    _multiplexedCommander.Execute(token, cancel);
            }

            _context.Apply(plan.ContextOperation);
        }

        private void Clean()
        {
            if (_heldConnection != null && !_context.MustKeepConnection)
            {
                _heldConnection.Dispose();
                _heldConnection = null;
            }
        }

        public void PushMessage(RedisNotification message)
        {
            Contract.Assert(message != null, "Trying to notify a null message.");
            CheckDispose();

            var handler = NotificationHandler;
            if (handler != null)
            {
                try
                {
                    handler(message);
                }
                catch(Exception)
                {}
            }
        }

        private void AmendResults(ExecutionPlan plan, RESPObject[] responses)
        {
            if(plan.IsTransaction)
                Transaction.Consolidate(responses, plan.Headers);
            
            if(plan.HasScripts)
                Procedure.AmmendScriptErrors(responses, plan.Headers, _procedures);
        }

        public async Task<IRedisResults> ExecuteAsync<T>(String command, T parameters, CancellationToken cancel) 
            where T:class
        {
            ParameterGuard.CannotBeNullOrEmpty(command, "command");
            CheckDispose();

            var plan = _planner.Build(command);
            var setup = Pack(plan, parameters);

            using (var token = new AsyncExecutionToken(setup.Item1, setup.Item2))
            using (cancel.Register(token.SetCancelled))
            {
                try
                {
                    Route(plan, token, cancel);
                    await token.Completion.ConfigureAwait(false);
                }
                finally
                {
                    Clean();
                }

                AmendResults(plan, setup.Item3);
                return new RedisResults(setup.Item3, plan.Headers);
            }
        }

        public IRedisResults Execute<T>(String command, T parameters, CancellationToken cancel) 
            where T:class
        {
            ParameterGuard.CannotBeNullOrEmpty(command, "command");
            CheckDispose();

            var plan = _planner.Build(command);
            var setup = Pack(plan, parameters);

            using (var token = new SyncExecutionToken(setup.Item1, setup.Item2))
            using (cancel.Register(token.SetCancelled))
            {
                try
                {
                    Route(plan, token, cancel);
                    token.Wait(cancel);
                }
                finally
                {
                    Clean();
                }

                AmendResults(plan, setup.Item3);
                return new RedisResults(setup.Item3, plan.Headers);
            }  
        }

        public void Dispatch<T>(String command, T parameters)
            where T : class
        {
            ParameterGuard.CannotBeNullOrEmpty(command, "command");
            CheckDispose();

            if (_heldConnection == null)
            {
                var plan = _planner.Build(command);
                var setup = Pack(plan, parameters);

                using (var token = new NoWaitExecutionToken(setup.Item1, setup.Item2))
                    Route(plan, token, CancellationToken.None);
            }
            else
            {
                // if in the middle of transaction, it needs to wait
                // to check if the connection can be cleaned afterwards.
                Execute<T>(command, parameters, CancellationToken.None);
            }
        }

        private void CheckDispose()
        {
            if (_disposed)
                throw new ObjectDisposedException("RedisChannel has been already disposed.");
        }

        public void Dispose()
        {
            _disposed = true;
            try
            {
                this.NotificationHandler = null;

                if(_heldConnection != null)
                {
                    if (_context.IsTransactionOngoing)
                        _heldConnection.Execute(new NoWaitExecutionToken(new DiscardTransactionOperation(), null), CancellationToken.None);
                    else if(_context.IsWatchOnGoing)
                        _heldConnection.Execute(new NoWaitExecutionToken(new UnwatchOperation(), null), CancellationToken.None);
                }

                DisposeHelper.SafeDispose(_heldConnection);

                if (_subscriber != null)
                    _subscriber.Execute(new NoWaitExecutionToken(null, new RemoveChannelOperation(this, _subscriber.Subscriptions)), CancellationToken.None);
            }
            catch (ObjectDisposedException) { }
        }
    }
}
