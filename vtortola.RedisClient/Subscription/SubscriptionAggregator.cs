using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    internal class AddedKeys : List<String> { }
    internal class RemovedKeys : List<String> { }

    [DebuggerDisplay("Subscribed: {_subscribed.Count}, Subscriptions: {_subscriptions.Count}")]
    internal sealed class SubscriptionAggregator
    {
        readonly Dictionary<String, HashSet<IRedisChannel>> _subscribed;
        readonly Dictionary<IRedisChannel, HashSet<String>> _subscriptions;

        internal Int32 KeyCount { get { return _subscribed.Count; } }
        internal Int32 ChannelCount { get { return _subscribed.Count; } }

        internal SubscriptionAggregator()
        {
            _subscribed = new Dictionary<String, HashSet<IRedisChannel>>();
            _subscriptions = new Dictionary<IRedisChannel, HashSet<String>>();
        }

        internal AddedKeys Subscribe(IRedisChannel channel, IEnumerable<String> keys)
        {
            Contract.Assert(keys.Any(), "Subscribing redis channel to empty list of keys.");
                       
            var added = new AddedKeys();

            HashSet<IRedisChannel> subscribed;
            foreach (var key in keys)
            {
                if (_subscribed.TryGetValue(key, out subscribed))
                {
                    subscribed.Add(channel);
                }
                else
                {
                    subscribed = new HashSet<IRedisChannel>();
                    subscribed.Add(channel);
                    _subscribed.Add(key, subscribed);
                    added.Add(key);
                }
            }

            HashSet<String> channelSubscriptions;
            if (!_subscriptions.TryGetValue(channel, out channelSubscriptions))
            {
                channelSubscriptions = new HashSet<String>(keys);
                _subscriptions.Add(channel, channelSubscriptions);
            }
            else
            {
                foreach (var key in keys)
                    channelSubscriptions.Add(key);
            }


            return added;
        }

        internal RemovedKeys Unsubscribe(IRedisChannel channel, IEnumerable<String> keys)
        {
            Contract.Assert(keys.Any(), "Unsubcribing channel from an empty list of keys.");

            return UnsubscribeInternal(channel, keys);
        }

        private RemovedKeys UnsubscribeInternal(IRedisChannel channel, IEnumerable<String> keys)
        {
            var removed = new RemovedKeys();
            HashSet<IRedisChannel> subscribed;

            foreach (var key in keys)
            {
                if (_subscribed.TryGetValue(key, out subscribed))
                {
                    subscribed.Remove(channel);
                    if (subscribed.Count == 0)
                    {
                        removed.Add(key);
                        _subscribed.Remove(key);
                    }
                }
            }

            HashSet<String> channelSubscriptions;
            if (_subscriptions.TryGetValue(channel, out channelSubscriptions))
            {
                channelSubscriptions.RemoveWhere(s => keys.Contains(s));
                if (channelSubscriptions.Count == 0)
                    _subscriptions.Remove(channel);
            }

            return removed;
        }

        internal IEnumerable<IRedisChannel> GetChannels(String key)
        {
            Contract.Assert(!String.IsNullOrWhiteSpace(key), "Using a null key to locate a channel.");

            var result = new List<IRedisChannel>();
            HashSet<IRedisChannel> channels;
            if (_subscribed.TryGetValue(key, out channels))
                result.AddRange(channels);
            return result;
        }

        internal IEnumerable<IRedisChannel> GetChannels()
        {
            var result = new List<IRedisChannel>();
            result.AddRange(_subscriptions.Keys);                
            return result;
        }

        internal IEnumerable<String> GetSubscriptions()
        {
            var result = new List<String>();
            result.AddRange(_subscribed.Keys);                
            return result;
        }

        internal IEnumerable<String> GetSubscriptions(IRedisChannel channel)
        {
            var subscribedTo = new RemovedKeys();
            HashSet<String> subscriptions;
            if (_subscriptions.TryGetValue(channel, out subscriptions))
                subscribedTo.AddRange(subscriptions);
            return subscribedTo;
        }
    }
}
