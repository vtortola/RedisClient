using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vtortola.Redis;

namespace UnitTest.RedisClient
{
    internal class DummySocketReader : SocketReader
    {
        internal DummySocketReader(String value, Int32 bufferSize = 8192)
            :base(value != null ? new MemoryStream(Encoding.UTF8.GetBytes(value)) : new MemoryStream(), bufferSize)
        {

        }
    }
}
