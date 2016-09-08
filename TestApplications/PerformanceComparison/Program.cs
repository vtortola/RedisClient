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

            var endpoint = new IPEndPoint(IPAddress.Parse("192.168.0.16")/*IPAddress.Loopback*/, 6379);

            // This test is meaningless because ServiceStack does not pipeline by default
            // and it does not support asynchronous operations.
            //CreateReport<RedisClientSimpleTest, ServiceStackSimpleTest, StackExchangeRedisSimpleTest>("simple", endpoint);

            Console.WriteLine("\n***************************************\n");

            CreateReport<RedisClientTransactionTest, ServiceStackTransactionTest, StackExchangeRedisTransactionTest>("transaction", endpoint);

            Console.ReadKey(true);
        }

        static TimeSpan GetMinimum(IEnumerable<TimeSpan> times)
        {
            return times
                    .Where(x => x != Timeout.InfiniteTimeSpan)
                    .Min();
        }

        static void CreateReport<TRedisClient, TServiceStack, TStackExchange>(String fileName, IPEndPoint endpoint)
            where TRedisClient : ITest, new()
            where TServiceStack : ITest, new()
            where TStackExchange : ITest, new()
        {
            var usersCounts = new[] { 50, 100, 200, 500, 1000 };
            var results = new List<TestData>(usersCounts.Length);

            foreach (var userCount in usersCounts)
            {
                var partial = new List<TestData>(10);
                for (int i = 0; i < 10; i++)
                {
                    partial.Add(PerformSingleTest<TRedisClient, TServiceStack, TStackExchange>(userCount, endpoint));
                }
                results.Add(new TestData()
                {
                    Users = userCount,
                    RedisClient = GetMinimum(partial.Select(x=>x.RedisClient)),
                    StackExchangeRedis = GetMinimum(partial.Select(x => x.StackExchangeRedis)),
                    ServiceStackRedis = GetMinimum(partial.Select(x => x.ServiceStackRedis)),
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

        static void Display(String test, Tuple<Int64, TimeSpan> result)
        {
            if(result != null)
                Console.WriteLine("{2}\t: Operations {0}, Time: {1}", result.Item1.ToString(), result.Item2.ToString(), test);
            else
                Console.WriteLine("{0}\t: FAILED", test);
        }

        static TimeSpan GetTime(Tuple<Int64, TimeSpan> result)
        {
            return result != null ? result.Item2 : Timeout.InfiniteTimeSpan;
        }

        static TestData PerformSingleTest<TRedisClient, TServiceStack, TStackExchange>(Int32 users, IPEndPoint endpoint)
            where TRedisClient : ITest, new()
            where TServiceStack : ITest, new()
            where TStackExchange : ITest, new()
        {
            Console.WriteLine("\nTesting 1000 iterations with {0} users.\n", users);

            GC.Collect();

            var serviceStack = Test(new TServiceStack(), users, endpoint);

            GC.Collect();

            var redisClient = Test(new TRedisClient(), users, endpoint);

            GC.Collect();

            var stackExchange = Test(new TStackExchange(), users, endpoint);

            Console.WriteLine();
            Display("RedisClient", redisClient);
            Display("ServiceStack", serviceStack);
            Display("StackExchange", stackExchange);

            return new TestData()
            {
                Users = users,
                RedisClient = GetTime(redisClient),
                StackExchangeRedis = GetTime(stackExchange),
                ServiceStackRedis = GetTime(serviceStack),
            };
        }

        static Tuple<Int64, TimeSpan> Test(ITest test, Int32 concurrentUsers, IPEndPoint endpoint)
        {
            Int64 counter = 0;
            test.Init(endpoint, CancellationToken.None).Wait();

            var cancel = new CancellationTokenSource();
            var threadLists = new List<Thread>();

            var bars = 0;
            var progress = 0;

            var sw = new Stopwatch();
            Console.WriteLine();
            sw.Start();

            Exception error = null;
            for (int i = 0; i < concurrentUsers; i++)
            {
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
                        Console.Write("[E." + test.GetType().Name + ", "+ex.GetType().Name+":"+ex.Message+"]");
                        Interlocked.CompareExchange(ref error, ex, null);
                    }
                    finally
                    {
                        var p = Interlocked.Increment(ref progress);
                        var percentage = (Int32)((p * 100D) / concurrentUsers);
                        while (bars < percentage)
                        {
                            Interlocked.Increment(ref bars);
                            Console.Write("|");
                        }
                    }
                });

                var t = new Thread(ts);
                t.Start();
                threadLists.Add(t);
            }

            foreach (var t in threadLists)
                t.Join();

            sw.Stop();
            cancel.Cancel();
            test.ClearData();

            if (error != null)
                return null;
            else
                return new Tuple<Int64, TimeSpan>(counter, sw.Elapsed);
        }
    }
}

