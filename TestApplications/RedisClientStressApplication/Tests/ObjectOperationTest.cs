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
    public class ObjectOperationTest : OperationTestBase
    {
        static DummyClass[] _data = Enumerable.Range(0, 100).Select(i => DummyClass.Create(i)).ToArray();

        public ObjectOperationTest(IPEndPoint endpoint)
            :base(endpoint)
        {
        }

        protected override async Task RunClient(String userId, IRedisChannel channel, CancellationToken cancel)
        {
            try
            {
                var userKey = "user_" + userId;

                foreach (var dummy in _data)
                {
                    var result = await channel.ExecuteAsync("hmset @key @data", new { key = userId + "_" + dummy.Id, data = Parameter.SequenceProperties(dummy) }).ConfigureAwait(false);
                    result.ThrowErrorIfAny();
                }

                foreach (var dummy in _data)
                {
                    var result = await channel.ExecuteAsync(@"
                                                    hgetall @key
                                                    del @key",
                                                    new { key = userId + "_" + dummy.Id })
                                                    .ConfigureAwait(false);
                    result.ThrowErrorIfAny();
                    var readed = result[0].AsObjectCollation<DummyClass>();
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
