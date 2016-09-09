using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace RedisClientStressApplication
{
    public abstract class OperationTestBase : ITestScenario
    {
        readonly IPEndPoint _endpoint;

        public OperationTestBase(IPEndPoint endpoint)
        {
            _endpoint = endpoint;
        }

        public Tuple<Int64, TimeSpan> Test(Int32 threads, Int32 loops)
        {
            using (var client = new RedisClient(_endpoint, new RedisClientOptions() /*{ Logger = new ConsoleLogger() }*/))
            {
                client.ConnectAsync(CancellationToken.None).Wait();

                using (var channel = client.CreateChannel())
                    channel.Execute("flushall");

                var cancel = new CancellationTokenSource();
                var taskList = new List<Thread>();

                var total = threads * loops;
                var progress = 0;
                var bars = 0;

                var sw = new Stopwatch();
                sw.Start();
                for (int threadId = 0; threadId < threads; threadId++)
                {
                    var ts = new ThreadStart(() =>
                    {
                        try
                        {
                            for (int loop = 0; loop < loops; loop++)
                            {
                                using (var channel = client.CreateChannel())
                                    RunClient(threadId.ToString() + "_" + loop.ToString(), channel, cancel.Token).Wait();

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
                            Console.Write("[{0}]", ex.GetType());
                        }
                    });

                    var thread = new Thread(ts);
                    thread.Start();
                    taskList.Add(thread);
                }

                foreach (var t in taskList)
                    t.Join();
                sw.Stop();

                cancel.Cancel();

                return new Tuple<Int64, TimeSpan>(progress, sw.Elapsed);
            }
        }

        protected abstract Task RunClient(String userId, IRedisChannel channel, CancellationToken cancel);
    }
}
