using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using vtortola.Redis;
using System.Threading.Tasks;

namespace IntegrationTests.RedisClientTests.Scripting
{
    [TestClass]
    public class MULTIGETTests : RedisMultiplexTestBase
    {
        protected override RedisClientOptions GetOptions()
        {
            var options = new RedisClientOptions();
            var scripts = options.Procedures;
            scripts.Load(GetScript());
            return options;
        }

        TextReader GetScript()
        {
            return new StreamReader("MULTIGET.lua");
        }

        List<String> UserIds;

        [TestInitialize]
        public void InitializeTestData()
        {
            UserIds = new List<String>();
            using (var channel = Client.CreateChannel())
            {
                for (int i = 0; i < 10; i++)
                {
                    var userId = "user:" + i.ToString("000");
                    UserIds.Add(userId);

                    var userData = new
                    {
                        Id = userId,
                        Name = "User " + i.ToString(),
                        Email = i.ToString("0000") + "@something.com"
                    };

                    channel.Execute("hmset @userId @data", new { userId = userId, data = Parameter.SequenceProperties(userData) });
                }
            }
        }

        [TestMethod]
        [DeploymentItem("Scripting\\Scripts\\MULTIGET.lua")]
        public async Task CanPassDataToScript()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = await channel.ExecuteAsync("multiget @mykeys", new { mykeys = UserIds.AsEnumerable() });

                Assert.IsNotNull(result);
                var subresults = result[0].AsResults();
                Assert.AreEqual(10, subresults.Count);
                foreach (var subresult in subresults)
                {
                    var dictionary = subresult.AsDictionaryCollation<String,String>();
                    Assert.IsNotNull(dictionary["Id"]);
                    Assert.IsNotNull(dictionary["Name"]);
                    Assert.IsNotNull(dictionary["Email"]);
                }
            }
        }
    }
}
