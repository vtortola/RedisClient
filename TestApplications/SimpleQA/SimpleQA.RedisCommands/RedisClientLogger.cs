using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public class RedisLogger : IRedisClientLog
    {
        readonly ILogger _log;
        public RedisLogger()
        {
            _log = LogManager.GetLogger("vtortola.RedisClient");
        }

        public void Info(string format, params object[] args)
        {
            _log.Info(format, args);
        }

        public void Error(string format, params object[] args)
        {
            _log.Error(format, args);
        }

        public void Error(System.Exception error, string format, params object[] args)
        {
            _log.Error(error, format, args);
        }

        public void Debug(string format, params object[] args)
        {

        }
    }
}
