using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vtortola.Redis;

namespace UnitTest.RedisClient
{
    internal class DummySocketWriter : SocketWriter
    {
        readonly MemoryStream _ms;
        internal DummySocketWriter(MemoryStream ms)
            :base(ms, 8192)
        {
            _ms = ms;
        }
        public override string ToString()
        {
            this.Flush();
            _ms.Seek(0, SeekOrigin.Begin);

            return new StreamReader(_ms).ReadToEnd();
        }
    }
}
