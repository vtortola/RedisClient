﻿using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PerformanceComparison.Tests.SimpleTests
{
    public class ServiceStackTransactionTest : ITest
    {
        IRedisClientsManager _pool;
        public Task Init(IPEndPoint endpoint, System.Threading.CancellationToken cancel)
        {
            _pool = new PooledRedisClientManager(100, 60, endpoint.ToString());
            using (var c = _pool.GetClient())
            {
                c.FlushAll();
                c.Increment("whatever", 1);
            }
            return Task.FromResult((Object)null);
        }

        public Task RunClient(Int32 id, CancellationToken cancel)
        {
            var key = id.ToString();
            using (var client = _pool.GetClient())
            {
                var values = new[] { new KeyValuePair<String, String>("Member1", "Value1"), new KeyValuePair<String, String>("Member2", "Value2"), 
                                     new KeyValuePair<String, String>("Member3", "Value3"), new KeyValuePair<String, String>("Member4", "Value4")};
                
                using (var trans = client.CreateTransaction())
                {
                    trans.QueueCommand(r => r.IncrementValue(key + "1"));
                    trans.QueueCommand(r => r.Set(key + "2", key));
                    trans.QueueCommand(r => r.SetRangeInHash(key + "3", values));
                    trans.Commit();
                }

                return Task.FromResult<Object>(null);
            }
        }

        public void ClearData()
        {
            using(var client = _pool.GetClient())
                client.FlushAll();
        }
    }
}
