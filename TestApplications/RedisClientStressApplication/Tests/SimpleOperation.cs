using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace RedisClientStressApplication
{
    public class SimpleOperationTest : OperationTestBase
    {
        public SimpleOperationTest(IPEndPoint endpoint)
            :base(endpoint)
        {
        }

        protected override Task RunClient(String userId, IRedisChannel channel, CancellationToken cancel)
        {
            try
            {
                var userKey = "user_" + userId;
                var result = channel.Execute("incr @key", new { key = userKey });
            }
            catch (ObjectDisposedException)
            {
                Console.Write("[D]");
            }
            catch (TaskCanceledException)
            {
                Console.Write("[C]");
            }
            catch (AggregateException aex)
            {
                foreach (var ex in aex.InnerExceptions)
                {
                    Console.Write("[EX: " + ex.Message + "]");
                }
            }
            catch (Exception ex)
            {
                Console.Write("[EX: " + ex.Message+"]");
            }
            return Task.FromResult<Object>(null);
        }
    }
}
