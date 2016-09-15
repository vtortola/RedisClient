using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vtortola.Redis;

namespace RedisClientStressApplication
{
    public sealed class ConsoleLogger : IRedisClientLog
    {
        public void Info(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Error(string format, params object[] args)
        {
            Console.WriteLine("Exception: " + format, args);
        }

        public void Error(Exception error, string format, params object[] args)
        {
            Console.WriteLine("Exception: " + String.Format(format, args) + "\n" + error.ToString());
        }
        
        public void Debug(String format, params Object[] args)
        {

        }
    }
}
