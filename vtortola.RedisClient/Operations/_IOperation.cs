using System;
using System.Collections.Generic;

namespace vtortola.Redis
{
    internal interface IOperation
    {
        Boolean IsCompleted { get; }

        IEnumerable<RESPCommand> Execute();

        void HandleResponse(RESPObject response);
    }

    internal interface ICommandOperation : IOperation
    {
    }

    internal interface ISubscriptionOperation : IOperation
    {
    }
}
