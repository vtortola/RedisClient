using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    internal sealed class SubscriptionsInitialization : IConnectionInitializer
    {
        readonly SubscriptionSplitter _subscriptions;

        internal SubscriptionsInitialization(SubscriptionSplitter subscriptions)
        {
            _subscriptions = subscriptions;
        }

        public void Initialize(SocketReader reader, SocketWriter writer)
        {
            var currentSubscriptions = _subscriptions.GetAllSubscribeCommands();
            if (currentSubscriptions.Any())
            {
                foreach (var command in currentSubscriptions)
                {
                    command.WriteTo(writer);
                }

                writer.Flush();
            }
        }
    }
}
