using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vtortola.Redis;

namespace IntegrationTests.RedisClientTests
{
    public class TraceRedisClientLogger : IRedisClientLog
    {
        public void Info(string format, params object[] args)
        {
            Trace.WriteLine(String.Format(format, args));
        }

        public void Error(string format, params object[] args)
        {
            Trace.WriteLine("Error: " + String.Format(format, args));
        }

        public void Error(Exception error, string format, params object[] args)
        {
            Trace.WriteLine("Error: " + String.Format(format, args) + "\r\n" + error.ToString());
        }
    }
}
