using RedisInside;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.RedisClientTests
{
    public static class RedisInstance
    {
        static readonly Redis Redis = new Redis();

        public static IPEndPoint Endpoint { get { return (IPEndPoint)Redis.Endpoint; } }

        static RedisInstance()
        {
            AppDomain.CurrentDomain.DomainUnload += (s, e) => Redis.Dispose();
        }
    }
}
