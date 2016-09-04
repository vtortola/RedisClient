using PerformanceComparison.Tests;
using PerformanceComparison.Tests.SimpleTests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace PerformanceComparison
{
    class Program
    {
        class TestData
        {
            public Int32 Users { get; set; }
            public TimeSpan RedisClient { get; set; }
            public TimeSpan StackExchangeRedis { get; set; }
            public TimeSpan ServiceStackRedis { get; set; }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to start...");
            Console.ReadKey(true);

            var endpoint = new IPEndPoint(IPAddress.Loopback, 6379);

            // This test is meaningless because ServiceStack does not pipeline by default
            // and it does not support asynchronous operations.
            //CreateReport<RedisClientSimpleTest, ServiceStackSimpleTest, StackExchangeRedisSimpleTest>("simple", endpoint);

            Console.WriteLine("\n***************************************\n");

            CreateReport<RedisClientTransactionTest, ServiceStackTransactionTest, StackExchangeRedisTransactionTest>("transaction", endpoint);

            Console.ReadKey(true);
        }

        static void CreateReport<TRedisClient, TServiceStack, TStackExchange>(String fileName, IPEndPoint endpoint)
            where TRedisClient : ITest, new()
            where TServiceStack : ITest, new()
            where TStackExchange : ITest, new()
        {
            const Int32 iterations = 1000;
            var usersCounts = new[] {  10, 25, 50, 100, 200, 500 };
            var results = new List<TestData>(usersCounts.Length);

            foreach (var userCount in usersCounts)
            {
                var partial = new List<TestData>(10);
                for (int i = 0; i < 10; i++)
                {
                    partial.Add(PerformSingleTest<TRedisClient, TServiceStack, TStackExchange>(iterations, userCount, endpoint));
                }
                results.Add(new TestData()
                {
                    Users = userCount,
                    RedisClient = TimeSpan.FromTicks((Int64)partial.Min(x=>x.RedisClient.Ticks)),
                    StackExchangeRedis = TimeSpan.FromTicks((Int64)partial.Min(x => x.StackExchangeRedis.Ticks)),
                    ServiceStackRedis = TimeSpan.FromTicks((Int64)partial.Min(x => x.ServiceStackRedis.Ticks)),
                });
            }

            using (var writer = new StreamWriter(fileName+".csv"))
            {
                foreach (var userCount in usersCounts)
                {
                    writer.Write(",");
                    writer.Write(userCount);
                }

                writer.WriteLine();
                writer.Write("RedisClient");
                foreach (var result in results)
                {
                    writer.Write(",");
                    writer.Write(result.RedisClient.TotalMilliseconds);
                }

                writer.WriteLine();
                writer.Write("ServiceStackRedis");
                foreach (var result in results)
                {
                    writer.Write(",");
                    writer.Write(result.ServiceStackRedis.TotalMilliseconds);
                }

                writer.WriteLine();
                writer.Write("StackExchangeRedis");
                foreach (var result in results)
                {
                    writer.Write(",");
                    writer.Write(result.StackExchangeRedis.TotalMilliseconds);
                }
            }
            Process.Start(fileName + ".csv");
        }

        static TestData PerformSingleTest<TRedisClient, TServiceStack, TStackExchange>(Int32 iterations, Int32 users, IPEndPoint endpoint)
            where TRedisClient : ITest, new()
            where TServiceStack : ITest, new()
            where TStackExchange : ITest, new()
        {
            Console.WriteLine("\nTesting {0} with {1} users.\n", iterations, users);

            GC.Collect();

            var serviceStack = Test(new TServiceStack(), iterations, users, endpoint);

            GC.Collect();

            var redisClient = Test(new TRedisClient(), iterations, users, endpoint);

            GC.Collect();

            var stackExchange = Test(new TStackExchange(), iterations, users, endpoint);

            Console.WriteLine();
            Console.WriteLine("RedisClient  : Operations {0}, Time: {1}", redisClient.Item1.ToString(), redisClient.Item2.ToString());
            Console.WriteLine("ServiceStack : Operations {0}, Time: {1}", serviceStack.Item1.ToString(), serviceStack.Item2.ToString());
            Console.WriteLine("StackExchange: Operations {0}, Time: {1}", stackExchange.Item1.ToString(), stackExchange.Item2.ToString());

            return new TestData()
            {
                Users = users,
                RedisClient = redisClient.Item2,
                StackExchangeRedis = stackExchange.Item2,
                ServiceStackRedis = serviceStack.Item2
            };
        }


        static Tuple<Int64, TimeSpan> Test(ITest test, Int32 loops, Int32 concurrentUsers, IPEndPoint endpoint)
        {
            Int64 counter = 0;
            test.Init(endpoint, CancellationToken.None).Wait();

            var cancel = new CancellationTokenSource();
            var taskList = new List<Thread>();
            var semaphore = new SemaphoreSlim(concurrentUsers);
            var step = loops / 100;
            step = step == 0 ? 1 : step;
            var sw = new Stopwatch();
            Console.WriteLine();
            sw.Start();
            for (int i = 0; i < loops; i++)
            {
                semaphore.Wait();
                int ii = i;
                var ts = new ThreadStart(() =>
                {
                    try
                    {
                        var r = test.RunClient(ii, cancel.Token).Result;
                        Interlocked.Add(ref counter, r);
                    }
                    catch (Exception ex)
                    {
                        Console.Write("[E." + test.GetType().Name + "]");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
                var t = new Thread(ts);
                t.Start();
                taskList.Add(t);

                if (i % step == 0)
                    Console.Write("|");
            }

            foreach (var t in taskList)
            {
                t.Join();
            }

            sw.Stop();
            cancel.Cancel();
            test.ClearData();

            return new Tuple<Int64, TimeSpan>(counter, sw.Elapsed);
        }
    }
}

