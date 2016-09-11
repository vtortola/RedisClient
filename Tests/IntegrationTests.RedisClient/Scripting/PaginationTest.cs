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
    public class PaginationTest : RedisMultiplexTestBase
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
            return new StreamReader("Pagination.lua");
        }

        [TestInitialize]
        public void InitializeTestData()
        {
            using (var channel = Client.CreateChannel())
            {
                for (int i = 0; i < 100; i++)
                {
                    var articleId = "article:" + i.ToString("000");
                    var date = DateTime.Now.AddDays(100 - i);

                    var article = new
                    {
                        Id = articleId,
                        Title = "Article " + i.ToString(),
                        Author = i.ToString("0000") + "@something.com"
                    };

                    channel.Execute(@"
                                hmset @id @data
                                zadd @articlesId @score @id", 
                                new 
                                { 
                                    articlesId = "articles", 
                                    id = articleId,
                                    score = date.Ticks,
                                    data = Parameter.SequenceProperties(article) 
                                });
                } 
            }
        }

        [TestMethod]
        [DeploymentItem("Scripting\\Scripts\\Pagination.lua")]
        public async Task CanPaginateDataUsingScript()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = await channel.ExecuteAsync("PaginationTest @articles @page @items", new { articles = "articles",  page=3, items=10 });

                Assert.IsNotNull(result);
                var subresults = result[0].AsResults();
                Assert.AreEqual(10, subresults.Count);
                for (int i = 0; i < 10; i++)
                {
                    var dictionary = subresults[i].AsDictionaryCollation<String, String>();
                    Assert.IsNotNull(dictionary["Id"]);
                    Assert.IsNotNull(dictionary["Title"]);
                    Assert.IsNotNull(dictionary["Author"]);

                    var ii = i + 30;
                    Assert.AreEqual("article:" + ii.ToString("000"), dictionary["Id"]);
                    Assert.AreEqual("Article " + ii.ToString(), dictionary["Title"]);
                    Assert.AreEqual(ii.ToString("0000") + "@something.com", dictionary["Author"]);
                }
            }
        }
    }
}
