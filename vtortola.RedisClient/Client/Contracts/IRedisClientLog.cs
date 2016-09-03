using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    /// <summary>
    /// Provides means to log internal activity in RedisClient
    /// </summary>
    public interface IRedisClientLog
    {
        /// <summary>
        /// Logs information.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void Info(String format, params Object[] args);

        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void Error(String format, params Object[] args);

        /// <summary>
        /// Logs an error with a <seealso cref="Exception"/>
        /// </summary>
        /// <param name="error"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void Error(Exception error, String format, params Object[] args);
    }

    internal sealed class NoLogger : IRedisClientLog
    {
        private NoLogger() { }

        public void Info(String format, params Object[] args) {}
        public void Error(String format, params Object[] args) {}
        public void Error(Exception error, String format, params Object[] args){}

        internal static NoLogger Instance = new NoLogger();
    }

}
