using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace RedisClientStressApplication
{
    public class ObjectOperationTest : ITestScenario
    {
        static DummyClass[] _data = Enumerable.Range(0, 100).Select(i => DummyClass.Create(i)).ToArray();

        Int64 counter = 0;
        Random ran = new Random();

        public Tuple<long, TimeSpan> Test(Int32 userCount, Int32 loops)
        {
            using (var client = new RedisClient(new IPEndPoint(IPAddress.Loopback, 6379), new RedisClientOptions() /*{ Logger = new ConsoleLogger() }*/))
            {
                client.ConnectAsync(CancellationToken.None).Wait();

                using (var channel = client.CreateChannel())
                    channel.Execute("flushall", (String)null);

                var cancel = new CancellationTokenSource();
                var taskList = new List<Thread>();
                var semaphore = new SemaphoreSlim(userCount);
                var iterations = loops * userCount;
                var step = iterations / 100;
                var sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < iterations; i++)
                {
                    semaphore.Wait();
                    int ii = i;
                    var ts = new ThreadStart(() =>
                    {
                        try
                        {
                            RunClient(ii, client.CreateChannel(), loops, userCount, cancel.Token).Wait();
                        }
                        catch (Exception ex)
                        {
                            Console.Write("[{0}]", ex.GetType());
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

                return new Tuple<long, TimeSpan>(counter, sw.Elapsed);
            }
        }

        private async Task RunClient(Int32 userId, IRedisChannel channel, int loops, int userCount, CancellationToken cancel)
        {
            try
            {
                var userKey = "user_" + userId;
                using (channel)
                {
                    foreach (var dummy in _data)
                    {
                        var result = await channel.ExecuteAsync("hmset @key @data", new { key = userId + "_" + dummy.Id, data = Parameter.SequenceProperties(dummy) }).ConfigureAwait(false);
                        result.ThrowErrorIfAny();
                        Interlocked.Increment(ref counter);
                    }
                    foreach (var dummy in _data)
                    {
                        var result = await channel.ExecuteAsync(@"
                                                        hgetall @key
                                                        del @key", 
                                                        new { key = userId + "_" + dummy.Id})
                                                        .ConfigureAwait(false);
                        result.ThrowErrorIfAny();
                        var readed = result[0].AsObjectCollation<DummyClass>();
                        Interlocked.Increment(ref counter);
                    }
                }
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
        }
    }

    public class DummyClass
    {
        public String Id { get; set; }

        public String SProperty1 { get; set; }
        public Int32 IProperty1 { get; set; }
        public Int64 LProperty1 { get; set; }
        public String SSProperty1 { get; set; }

        public String SProperty2 { get; set; }
        public Int32 IProperty2 { get; set; }
        public Int64 LProperty2 { get; set; }
        public String SSProperty2 { get; set; }

        public String SProperty3 { get; set; }
        public Int32 IProperty3 { get; set; }
        public Int64 LProperty3 { get; set; }
        public String SSProperty3 { get; set; }

        public String SProperty4 { get; set; }
        public Int32 IProperty4 { get; set; }
        public Int64 LProperty4 { get; set; }
        public String SSProperty4 { get; set; }

        public static DummyClass Create(Int32 i)
        {
            return new DummyClass()
            {
                Id = "entity:" +i,

                SProperty1 = i.ToString(),
                IProperty1 = i,
                LProperty1 = i,
                SSProperty1 = i.ToString(),


                SProperty2 = String.Empty.PadLeft(i, 'x'),
                IProperty2 = Int32.MaxValue -i,
                LProperty2 = Int64.MaxValue - i,
                SSProperty2 = String.Empty.PadLeft(1024- i, 'x'),


                SProperty3 = i.ToString(),
                IProperty3 = i,
                LProperty3 = i,
                SSProperty3 = i.ToString(),


                SProperty4 = String.Empty.PadLeft(i, 'x'),
                IProperty4 = Int32.MaxValue -i,
                LProperty4 = Int64.MaxValue - i,
                SSProperty4 = String.Empty.PadLeft(1024- i, 'x')

            };
        }

    }
}
