using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisClientStressApplication
{
    public interface ITestScenario
    {
        Tuple<Int64, TimeSpan> Test(Int32 userCount, Int32 loops);
    }
}
