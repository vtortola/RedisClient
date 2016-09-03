using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using Moq;
using System.Collections.Generic;

namespace UnitTest.RedisClient
{
    [TestClass]
    public class SubscriptionSplitterTests
    {
        SubscriptionSplitter _router;

        [TestInitialize]
        public void Init()
        {
            _router = new SubscriptionSplitter();
        }

        private IRedisChannel CreateChannel()
        {
            var channel = new Mock<IRedisChannel>();
            return channel.Object;
        }

        private void AssertArray(RESPCommand array, params String[] entries)
        {
            Assert.IsNotNull(array);
            Assert.AreEqual(entries.Length, array.Count);
            for (int i = 0; i < entries.Length; i++)
            {
                Assert.AreEqual(entries[i], ((RESPCommandLiteral)array[i]).Value);
            }
        }

        private RESPCommand BuildCommandArray(params String[] literals)
        {
            var cmd = new RESPCommand(new RESPCommandLiteral(literals[0]), true);
            for (int i = 1; i < literals.Length; i++)
                cmd.Add(new RESPCommandLiteral(literals[i]));
            return cmd;
        }

        private RESPArray BuildArray(params String[] literals)
        {
            return new RESPArray(literals);
        }

        private IEnumerable<RESPCommand> EnumerableRESPArray(RESPCommand array)
        {
            yield return array;
        }
        
        [TestMethod]
        public void CanDoSimpleSubscription()
        {
            var channelA = CreateChannel();

            var response = _router.Aggregate(channelA, EnumerableRESPArray(BuildCommandArray("subscribe", "message-channel-1")));
            AssertArray(response.GetCommands().First(), "SUBSCRIBE", "message-channel-1");
            Assert.AreEqual(channelA, _router.GetSubscribedTo(BuildArray("message", "message-channel-1")).Single());

            response = _router.Aggregate(channelA, EnumerableRESPArray(BuildCommandArray("psubscribe", "message-channel-1")));
            AssertArray(response.GetCommands().First(), "PSUBSCRIBE", "message-channel-1");
            Assert.AreEqual(channelA, _router.GetSubscribedTo(BuildArray("pmessage", "message-channel-1")).Single());
        }

        [TestMethod]
        public void CanDoSimpleUnsubscription()
        {
            var channelA = CreateChannel();

            _router.Aggregate(channelA, EnumerableRESPArray(BuildCommandArray("subscribe", "message-channel-1")));
            _router.Aggregate(channelA, EnumerableRESPArray(BuildCommandArray("psubscribe", "message-channel-1")));

            var response = _router.Aggregate(channelA, EnumerableRESPArray(BuildCommandArray("unsubscribe", "message-channel-1")));
            AssertArray(response.GetCommands().First(), "UNSUBSCRIBE", "message-channel-1");

            Assert.IsNull(_router.GetSubscribedTo(BuildArray("message", "message-channel-1")).SingleOrDefault());
            Assert.AreEqual(channelA, _router.GetSubscribedTo(BuildArray("pmessage", "message-channel-1")).Single());

            response = _router.Aggregate(channelA, EnumerableRESPArray(BuildCommandArray("punsubscribe", "message-channel-1")));
            AssertArray(response.GetCommands().First(), "PUNSUBSCRIBE", "message-channel-1");
            Assert.IsNull(_router.GetSubscribedTo(BuildArray("pmessage", "message-channel-1")).SingleOrDefault());
        }

        [TestMethod]
        public void CanEvictChannels()
        {
            var channelA = CreateChannel();
            var channelB = CreateChannel();

            _router.Aggregate(channelA, EnumerableRESPArray(BuildCommandArray("subscribe", "message-channel-1")));
            _router.Aggregate(channelA, EnumerableRESPArray(BuildCommandArray("psubscribe", "message-channel-1")));
                                                           
            _router.Aggregate(channelB, EnumerableRESPArray(BuildCommandArray("subscribe", "message-channel-1")));
            _router.Aggregate(channelB, EnumerableRESPArray(BuildCommandArray("psubscribe", "message-channel-1")));

            var channels = _router.GetChannels().ToList();
            Assert.AreEqual(2, channels.Count);
            Assert.AreEqual(channelA, channels[0]);
            Assert.AreEqual(channelB, channels[1]);

            channels = _router.GetChannels().ToList();
            Assert.AreEqual(2, channels.Count);
            Assert.AreEqual(channelB, channels[1]);

            var responses = _router.RemoveChannel(channelB);
            Assert.IsNotNull(responses);
            Assert.AreEqual(0, responses.GetCommands().Count());

            responses = _router.RemoveChannel(channelA);
            Assert.IsNotNull(responses);
            Assert.AreEqual(2, responses.GetCommands().Count());
            AssertArray(responses.GetCommands().ElementAt(0), "UNSUBSCRIBE", "message-channel-1");
            AssertArray(responses.GetCommands().ElementAt(1), "PUNSUBSCRIBE", "message-channel-1");
        }

        [TestMethod]
        public void CanGetSubscriptions()
        {
            var channelA = CreateChannel();
            var channelB = CreateChannel();
            
            _router.Aggregate(channelA, EnumerableRESPArray(BuildCommandArray("subscribe", "message-channel-1")));
            _router.Aggregate(channelA, EnumerableRESPArray(BuildCommandArray("psubscribe", "message-channel-1")));
                                                
            _router.Aggregate(channelB, EnumerableRESPArray(BuildCommandArray("subscribe", "message-channel-2")));
            _router.Aggregate(channelB, EnumerableRESPArray(BuildCommandArray("psubscribe", "message-channel-3")));

            var subscriptions = _router.GetAllSubscribeCommands().ToList();
            Assert.IsNotNull(subscriptions);
            Assert.AreEqual(2, subscriptions.Count);
            AssertArray(subscriptions[0], "SUBSCRIBE", "message-channel-1", "message-channel-2");
            AssertArray(subscriptions[1], "PSUBSCRIBE", "message-channel-1", "message-channel-3");

            _router.RemoveChannel(channelA);
            _router.RemoveChannel(channelB);

            subscriptions = _router.GetAllSubscribeCommands().ToList();
            Assert.IsNotNull(subscriptions);
        }
    }
}
