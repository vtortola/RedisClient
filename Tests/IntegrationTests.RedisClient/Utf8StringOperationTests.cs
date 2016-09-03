using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using vtortola.Redis;

namespace IntegrationTests.RedisClientTests
{
    [TestClass]
    public class Utf8StringOperationTests : RedisMultiplexTestBase
    {
        [TestMethod]
        public async Task CanExecuteSimpleUtf8Command()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = await channel.ExecuteAsync(@"set examplekey Düsseldorf");

                Assert.IsNotNull(result);
                Assert.IsNull(result[0].GetException());
                Assert.AreEqual("OK", result[0].GetString());

                result = await channel.ExecuteAsync(@"get examplekey");
                Assert.IsNotNull(result);
                Assert.IsNull(result[0].GetException());
                Assert.AreEqual("Düsseldorf", result[0].GetString());
            }
        }

        [TestMethod]
        public async Task CanExecuteComplexUtf8Command()
        {
            using (var channel = Client.CreateChannel())
            {
                var parameter = new
                {
                    value = "In the German alphabet, the (traditionally lowercase-only) letter ß, called \"Eszett\" (IPA: [ɛsˈtsɛt]) or \"scharfes S\" (IPA: [ˈʃaɐ̯.fəs ˈʔɛs],[ˈʃaː.fəs ˈʔɛs])"
                };

                var result = await channel.ExecuteAsync(@"set examplekey @value", parameter);

                Assert.IsNotNull(result);
                Assert.IsNull(result[0].GetException());
                Assert.AreEqual("OK", result[0].GetString());

                result = await channel.ExecuteAsync(@"get examplekey");
                Assert.IsNotNull(result);
                Assert.IsNull(result[0].GetException());
                Assert.AreEqual(parameter.value, result[0].GetString());
            }
        }



        [TestMethod]
        public async Task CanExecuteComplexHighSymbolCommand()
        {
            using (var channel = Client.CreateChannel())
            {
                var parameter = new
                {
                    value = "《"
                };

                var result = await channel.ExecuteAsync(@"set examplekey @value", parameter);

                Assert.IsNotNull(result);
                Assert.IsNull(result[0].GetException());
                Assert.AreEqual("OK", result[0].GetString());

                result = await channel.ExecuteAsync(@"get examplekey");
                Assert.IsNotNull(result);
                Assert.IsNull(result[0].GetException());
                Assert.AreEqual(parameter.value, result[0].GetString());
            }
        }


        [TestMethod]
        public async Task CanExecuteComplexUtf8CyrilicHashSetCommand()
        {
            using (var channel = Client.CreateChannel())
            {
                var parameter = new
                {
                    value = "вельмі шырокае паняцьце, якое азначае форму пісьма, абапертую на лацінскі альфабэт, якая на сёньняшні дзень"
                };

                var result = await channel.ExecuteAsync(@"hset examplekey field @value", parameter);

                Assert.IsNotNull(result);
                Assert.IsNull(result[0].GetException());
                Assert.AreEqual(1, result[0].GetInteger());

                result = await channel.ExecuteAsync(@"hget examplekey field");
                Assert.IsNotNull(result);
                Assert.IsNull(result[0].GetException());
                Assert.AreEqual(parameter.value, result[0].GetString());
            }
        }
    }
}
