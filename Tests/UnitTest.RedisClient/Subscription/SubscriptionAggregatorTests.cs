using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using Moq;

namespace UnitTest.RedisClient
{
    [TestClass]
    public class SubscriptionAggregatorTests
    {
        SubscriptionAggregator _subscriptions;

        [TestInitialize]
        public void Init()
        {
            _subscriptions = new SubscriptionAggregator();
        }

        private IRedisChannel CreateChannel()
        {
            return new Mock<IRedisChannel>().Object;
        }

        [TestMethod]
        public void CanDoSimpleSubscribe()
        {
            var channelA = CreateChannel();
            
            var result = _subscriptions.Subscribe(channelA, new []{"message-channel"});

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("message-channel", result[0]);
        }


        [TestMethod]
        public void DoNotResubscribe()
        {
            var channelA = CreateChannel();

            _subscriptions.Subscribe(channelA, new[] { "message-channel" });
            var result = _subscriptions.Subscribe(channelA, new[] { "message-channel" });

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }


        [TestMethod]
        public void DoNotResubscribeToAnAlreadySubscribedMessageChannel()
        {
            var channelA = CreateChannel();
            var channelB = CreateChannel();

            var result = _subscriptions.Subscribe(channelA, new[] { "message-channel" });
            result = _subscriptions.Subscribe(channelB, new[] { "message-channel" });

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void CanDoSimpleUnsubscribe()
        {
            var channelA = CreateChannel();

            _subscriptions.Subscribe(channelA, new[] { "message-channel" });
            var removed = _subscriptions.Unsubscribe(channelA, new[] { "message-channel" });

            Assert.IsNotNull(removed);
            Assert.AreEqual(1, removed.Count);
            Assert.AreEqual("message-channel", removed[0]);
        }


        [TestMethod]
        public void CanDoMultipleSubscribe()
        {
            var channelA = CreateChannel();

            var result = _subscriptions.Subscribe(channelA, new[] { "message-channel-1", "message-channel-2", "message-channel-3" });

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("message-channel-1", result[0]);
            Assert.AreEqual("message-channel-2", result[1]);
            Assert.AreEqual("message-channel-3", result[2]);
        }

        [TestMethod]
        public void CanDoMultipleUnsubscribe()
        {
            var channelA = CreateChannel();

            _subscriptions.Subscribe(channelA, new[] { "message-channel-1", "message-channel-2", "message-channel-3" });
            var result = _subscriptions.Unsubscribe(channelA, new[] { "message-channel-1", "message-channel-3" });

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("message-channel-1", result[0]);
            Assert.AreEqual("message-channel-3", result[1]);
        }

        [TestMethod]
        public void DoNotUnsubscribeFromMessageChannelsInUse()
        {
            var channelA = CreateChannel();
            var channelB = CreateChannel();

            _subscriptions.Subscribe(channelA, new[] { "message-channel-1", "message-channel-2", "message-channel-3" });
            _subscriptions.Subscribe(channelB, new[] { "message-channel-3", "message-channel-4", "message-channel-5" });

            var result = _subscriptions.Unsubscribe(channelA, new[] { "message-channel-1", "message-channel-2", "message-channel-3" });

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("message-channel-1", result[0]);
            Assert.AreEqual("message-channel-2", result[1]);

            result = _subscriptions.Unsubscribe(channelB, new[] { "message-channel-3", "message-channel-4" });

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("message-channel-3", result[0]);
            Assert.AreEqual("message-channel-4", result[1]);

            result = _subscriptions.Unsubscribe(channelA, new[] { "message-channel-5" });

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);

            result = _subscriptions.Unsubscribe(channelB, new[] { "message-channel-5" });

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("message-channel-5", result[0]);
        }

        [TestMethod]
        public void CanUnregisterChannels()
        {
            var channelA = CreateChannel();
            var channelB = CreateChannel();

            _subscriptions.Subscribe(channelA, new[] { "message-channel-1", "message-channel-2", "message-channel-3" });
            _subscriptions.Subscribe(channelB, new[] { "message-channel-1", "message-channel-2", "message-channel-3" });

            var result = _subscriptions.GetSubscriptions(channelA).ToArray();

            Assert.IsNotNull(result);

            result = _subscriptions.GetSubscriptions(channelB).ToArray();

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
            Assert.AreEqual("message-channel-1", result[0]);
            Assert.AreEqual("message-channel-2", result[1]);
            Assert.AreEqual("message-channel-3", result[2]);
        }

        [TestMethod]
        public void CanGetSubscriptions()
        {
            var channelA = CreateChannel();
            var channelB = CreateChannel();

            _subscriptions.Subscribe(channelA, new[] { "message-channel-1", "message-channel-2", "message-channel-3" });
            _subscriptions.Subscribe(channelB, new[] { "message-channel-3", "message-channel-4", "message-channel-5" });

            var result = _subscriptions.GetChannels("message-channel-3").ToList();
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(channelA, result[0]);
            Assert.AreEqual(channelB, result[1]);

            result = _subscriptions.GetChannels("message-channel-1").ToList(); 
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(channelA, result[0]);

            result = _subscriptions.GetChannels("message-channel-5").ToList(); 
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(channelB, result[0]);

            result = _subscriptions.GetChannels("message-channel-6").ToList(); 
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
    }
}
