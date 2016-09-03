using System;
using System.Diagnostics.Contracts;
using System.Threading;

namespace vtortola.Redis
{
    internal abstract class ExecutionToken : IDisposable
    {
        internal Exception Error { get; private set; }
        internal Boolean IsCancelled { get; private set; }
        internal ICommandOperation CommandOperation { get; private set; }
        internal ISubscriptionOperation SubscriptionOperation { get; private set; }

        Int32 _signalingsNeeded;
        Int32 _finished;

        internal ExecutionToken(ICommandOperation commandOperation, ISubscriptionOperation subscriptionOperation)
        {
            Contract.Assert(commandOperation != null || subscriptionOperation != null, "In ExecutionToken both operations cannot be null.");

            CommandOperation = commandOperation;
            SubscriptionOperation = subscriptionOperation;

            if(commandOperation != null)      
                _signalingsNeeded++;

            if (subscriptionOperation != null)
                _signalingsNeeded++;
        }

        internal virtual void SetCompleted()
        {
            Contract.Assert(_signalingsNeeded > 0, "SetCompleted called more times than expected.");

            if (IsCancelled || Error != null)
                return;

            if(Interlocked.Decrement(ref _signalingsNeeded) == 0)
            {
                if(Interlocked.CompareExchange(ref _finished, 1,0) == 0)
                    SignalCompleted();
            }
        }
        
        internal void SetFaulted(Exception error)
        {
            Contract.Assert(error != null, "Trying to set a toke faulted with a null exception.");

            if (Interlocked.CompareExchange(ref _finished, 1, 0) == 0)
            {
                Error = error;
                SignalFaulted(error);
            }
        }

        internal void SetCancelled()
        {
            if (Interlocked.CompareExchange(ref _finished, 1, 0) == 0)
            {
                IsCancelled = true;
                SignalCancelled();
            }
        }

        protected virtual void SignalCompleted() { }
        protected virtual void SignalFaulted(Exception error) { }
        protected virtual void SignalCancelled() { }
        public virtual void Dispose() { }
    }
}
