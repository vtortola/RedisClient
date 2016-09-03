using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PerformanceComparison.Tests
{
    public abstract class TransactionTest : ITest
    {
        protected const Int32 Iterations = 1000;

        public abstract Task Init(IPEndPoint endpoint, CancellationToken cancel);

        public abstract Task<Int64> RunClient(Int32 id, CancellationToken cancel);

        public abstract void ClearData();
    }
}
