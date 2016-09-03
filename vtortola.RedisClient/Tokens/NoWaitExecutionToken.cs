using System;

namespace vtortola.Redis
{
    internal sealed class NoWaitExecutionToken : ExecutionToken
    {
        internal NoWaitExecutionToken(ICommandOperation commandOperation, ISubscriptionOperation subscriptionOperation)
            : base(commandOperation, subscriptionOperation)
        {
        }
    }
}
