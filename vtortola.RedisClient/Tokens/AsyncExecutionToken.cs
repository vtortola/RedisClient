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
            Task.Run(()=>_completion.SetResult(null));
        }

        protected override void SignalFaulted(Exception error)
        {
            Task.Run(()=>_completion.TrySetException(Error));
        }

        protected override void SignalCancelled()
        {
            Task.Run(()=>_completion.TrySetCanceled());
        }
    }
}
