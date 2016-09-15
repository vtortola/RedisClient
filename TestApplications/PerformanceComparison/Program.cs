using PerformanceComparison.Tests;
using PerformanceComparison.Tests.GetData;
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
            public Double RedisClient { get; set; }
            public Double StackExchangeRedis { get; set; }
            public Double ServiceStackRedis { get; set; }
        }

        static void Main(string[] args)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("192.168.0.16"), 6379);

            Console.WriteLine("PERFORMANCE COMPARISON");
            Console.WriteLine("Choose scneartio:");
            Console.WriteLine("1) Simple INCR operation.");
            Console.WriteLine("2) Multiple operation transaction.");
            Console.WriteLine("3) Multiple read operations pipelined.");
            var option = Console.ReadKey();
            switch (option.KeyChar)
            {
                case '1':
                    CreateReport<RedisClientSimpleTest, ServiceStackSimpleTest, StackExchangeRedisSimpleTest>("simple", endpoint);
                    break;
                case '2':
                    CreateReport<RedisClientTransactionTest, ServiceStackTransactionTest, StackExchangeRedisTransactionTest>("transaction", endpoint);
                    break;
                case '3':
                    CreateReport<RedisClientGetDataTest, ServiceStackGetDataTest, StackExchangeGetDataTest>("getdata", endpoint);
                    break;
                default:
                    throw new InvalidOperationException("No test defined with option " + option.KeyChar);
            }

            Console.ReadKey(true);
        }

        static Double GetMaximum(IEnumerable<Double> opsPerSecond)
        {
            return opsPerSecond
                    .Where(x => !Double.IsPositiveInfinity(x))
                    .Max();
        }

        static void CreateReport<TRedisClient, TServiceStack, TStackExchange>(String fileName, IPEndPoint endpoint)
            where TRedisClient : ITest, new()
            where TServiceStack : ITest, new()
            where TStackExchange : ITest, new()
        {
            var threadCounts = new[] { 10, 25, 50, 75, 100 };
            var results = new List<TestData>(threadCounts.Length);

            foreach (var threadCount in threadCounts)
            {
                var partial = new List<TestData>(5);
                for (int i = 0; i < 3; i++)
                {
                    partial.Add(PerformSingleTest<TRedisClient, TServiceStack, TStackExchange>(threadCount, 10000, endpoint));
                }
                results.Add(new TestData()
                {
                    Users = threadCount,
                    RedisClient = GetMaximum(partial.Select(x=>x.RedisClient)),
                    StackExchangeRedis = GetMaximum(partial.Select(x => x.StackExchangeRedis)),
                    ServiceStackRedis = GetMaximum(partial.Select(x => x.ServiceStackRedis)),
                });
            }
            fileName = fileName + "_" + Guid.NewGuid().ToString() + ".csv";
            using (var writer = new StreamWriter(fileName))
            {
                foreach (var userCount in threadCounts)
                {
                    writer.Write(",");
                    writer.Write(userCount);
                }

                writer.WriteLine();
                writer.Write("RedisClient");
                foreach (var result in results)
                {
                    writer.Write(",");
                    writer.Write(result.RedisClient);
                }

                writer.WriteLine();
                writer.Write("ServiceStackRedis");
                foreach (var result in results)
                {
                    writer.Write(",");
                    writer.Write(result.ServiceStackRedis);
                }

                writer.WriteLine();
                writer.Write("StackExchangeRedis");
                foreach (var result in results)
                {
                    writer.Write(",");
                    writer.Write(result.StackExchangeRedis);
                }
            }
            Process.Start(fileName);
        }

        static void Display(String test, Tuple<Int64, TimeSpan> result)
        {
            if(result != null)
                Console.WriteLine("{2}\t::: \tops/s: {0} \tTime: {1}", (result.Item1/result.Item2.TotalSeconds).ToString(".00").PadLeft(9, ' '), result.Item2.ToString(), test);
            else
                Console.WriteLine("{0}\t::: FAILED", test);
        }

        static Double GetOpsPerSecond(Tuple<Int64, TimeSpan> result)
        {
            return result != null ? (result.Item1 / result.Item2.TotalSeconds) : Double.PositiveInfinity;
        }

        static TestData PerformSingleTest<TRedisClient, TServiceStack, TStackExchange>(Int32 threads, Int32 runsPerThread, IPEndPoint endpoint)
            where TRedisClient : ITest, new()
            where TServiceStack : ITest, new()
            where TStackExchange : ITest, new()
        {
            Console.WriteLine("\nTesting with {0} threads, with {1} tries per thread.\n", threads, runsPerThread);

            GC.Collect();

            var serviceStack = Test(new TServiceStack(), threads, runsPerThread, endpoint);

            GC.Collect();

            var redisClient = Test(new TRedisClient(), threads, runsPerThread, endpoint);

            GC.Collect();

            var stackExchange = Test(new TStackExchange(), threads, runsPerThread, endpoint);

            Console.WriteLine();
            Display("ServiceStack", serviceStack);
            Display("RedisClient", redisClient);
            Display("StackExchange", stackExchange);

            return new TestData()
            {
                Users = threads,
                RedisClient = GetOpsPerSecond(redisClient),
                StackExchangeRedis = GetOpsPerSecond(stackExchange),
                ServiceStackRedis = GetOpsPerSecond(serviceStack),
            };
        }

        static Tuple<Int64, TimeSpan> Test(ITest test, Int32 threads, Int32 runsPerThread, IPEndPoint endpoint)
        {
            Int64 counter = 0;
            test.Init(endpoint, CancellationToken.None).Wait();

            var cancel = new CancellationTokenSource();
            var threadLists = new List<Thread>();

            var total = threads * runsPerThread;
            var progress = 0;
            var bars = 0;

            var sw = new Stopwatch();
            Console.WriteLine();
            sw.Start();

            Exception error = null;
            for (int i = 0; i < threads; i++)
            {
                int ii = i;
                var ts = new ThreadStart(() =>
                {
                    try
                    {
                        for (int r = 0; r < runsPerThread; r++)
                        {
                            test.RunClient(ii, cancel.Token).Wait();
                            Interlocked.Increment(ref counter);

                            var p = Interlocked.Increment(ref progress);
                            var percentage = (Int32)((p * 100D) / total);
                            while (bars < percentage)
                            {
                                Interlocked.Increment(ref bars);
                                Console.Write("|");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Write("[E." + test.GetType().Name + ", "+ex.GetType().Name+":"+ex.Message+"]");
                        Interlocked.CompareExchange(ref error, ex, null);
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

