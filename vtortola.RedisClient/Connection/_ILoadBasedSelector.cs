using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Linq;

namespace vtortola.Redis
{
    internal interface ILoadBasedSelector
    {
         T Select<T>(T[] elements) where T:ILoadMeasurable;
    }
}
