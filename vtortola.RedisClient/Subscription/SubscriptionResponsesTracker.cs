using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    internal sealed class SubscriptionResponsesTracker
    {
        readonly HashSet<String> _awaitedSubscriptions;
        readonly HashSet<String> _awaitedUnsubscriptions;
        readonly List<RESPCommand> _commands;

        internal Boolean IsCompleted { get { return _awaitedSubscriptions.Count == 0 && _awaitedUnsubscriptions.Count == 0; } }

        internal SubscriptionResponsesTracker()
        {
            _awaitedSubscriptions = new HashSet<String>();
            _awaitedUnsubscriptions = new HashSet<String>();
            _commands = new List<RESPCommand>();
        }

        internal void AddSubscription(String name)
        {
            _awaitedSubscriptions.Add(name);
        }

        internal void AddUnsubscription(String name)
        {
            _awaitedUnsubscriptions.Add(name);
        }

        internal void AddCommand(RESPCommand command)
        {
            _commands.Add(command);
        }

        private void AcknowledgeSubscription(String name)
        {
            _awaitedSubscriptions.Remove(name);
        }

        private void AcknowledgeUnsubscription(String name)
        {
            _awaitedUnsubscriptions.Remove(name);
        }

        internal IEnumerable<RESPCommand> GetCommands()
        {
            return _commands.AsEnumerable();
        }

        internal void Acknowledge(RESPArray response)
        {
            Contract.Assert(response.Any(), "Empty RESPArray");

            var operation = response.ElementAt<RESPBulkString>(0).Value.ToUpperInvariant();
            var key = response.ElementAt<RESPBulkString>(1).Value;

            switch (operation)
            {
                case "SUBSCRIBE":
                case "PSUBSCRIBE":
                    AcknowledgeSubscription(key);
                    break;

                case "UNSUBSCRIBE":
                case "PUNSUBSCRIBE":
                    AcknowledgeUnsubscription(key);
                    break;
            }
        }
    }
}
