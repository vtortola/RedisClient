using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PerformanceComparison.Tests
{
    public interface ITest
    {
        Task Init(IPEndPoint endpoint, CancellationToken cancel);
        Task RunClient(Int32 id, CancellationToken cancel);
        void ClearData();
    }
}
