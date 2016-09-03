using SimpleQA.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleQA.Models
{
    public interface IMessaging : IDisposable
    {
        Task<PushMessage> ReceiveMessage(CancellationToken cancel);
        Task SendMessageAsync(PushSubscriptionRequest request, CancellationToken cancel);
    }
}
