using System;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    internal sealed class AsyncExecutionToken : ExecutionToken
    {
        readonly TaskCompletionSource<Object> _completion;
        internal Task<Object> Completion { get { return _completion.Task; } }

        internal AsyncExecutionToken(ICommandOperation commandOperation, ISubscriptionOperation subscriptionOperation)
            : base(commandOperation, subscriptionOperation)
        {
            _completion = new TaskCompletionSource<Object>();
        }

        protected override void SignalCompleted()
        {
            _completion.TrySetResult(null);
        }

        protected override void SignalFaulted(Exception error)
        {
            _completion.TrySetException(Error);
        }

        protected override void SignalCancelled()
        {
            _completion.TrySetCanceled();
        }
    }
}
