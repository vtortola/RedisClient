using System;
using System.Threading;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    internal interface IConnection : IDisposable
    {
        void Execute(ExecutionToken token, CancellationToken cancel);
        Task ConnectAsync(CancellationToken cancel);
    }

    internal interface IConnectionProvider<out TConnection>  : IDisposable
        where TConnection : IConnection
    {
        TConnection Provide();
        Task ConnectAsync(CancellationToken cancel);
    }

    internal interface ICommandConnection : IConnection
    {
    }

    internal interface ISubscriptionConnection : IConnection
    {
        SubscriptionSplitter Subscriptions { get; }
    }
}
