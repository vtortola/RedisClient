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
        public void Info(String format, params Object[] args)
        {
            Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff ") + String.Format(format, args));
        }

        public void Error(String format, params Object[] args)
        {
            Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff ") + "Error: " + String.Format(format, args));
        }

        public void Error(Exception error, String format, params Object[] args)
        {
            Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff ") + "Error: " + String.Format(format, args) + "\r\n" + error.ToString());
        }
        
        public void Debug(String format, params Object[] args)
        {
            Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff ") + String.Format(format, args));
        }
    }
}
