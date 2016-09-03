using System;
using System.Threading;

namespace vtortola.Redis
{
    internal sealed class SyncExecutionToken : ExecutionToken
    {
        readonly ManualResetEventSlim _reset;
        volatile Boolean _cancelled;

        internal SyncExecutionToken(ICommandOperation commandOperation, ISubscriptionOperation subscriptionOperation)
            : base(commandOperation, subscriptionOperation)
        {
            _reset = new ManualResetEventSlim(false);
        }

        internal void Wait(CancellationToken cancel)
        {
            _reset.Wait(cancel);

            if (Error != null)
                throw Error;
            else if (_cancelled)
                throw new OperationCanceledException(); 
        }

        protected override void SignalCompleted()
        {
            _reset.Set();
        }

        protected override void SignalFaulted(Exception error)
        {
            _reset.Set();
        }

        protected override void SignalCancelled()
        {
            _cancelled = true;
            _reset.Set();
        }

        public override void Dispose()
        {
            _reset.Dispose();
        }
    }
}
