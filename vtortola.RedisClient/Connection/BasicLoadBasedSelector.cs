using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace vtortola.Redis
{
    internal sealed class BasicLoadBasedSelector : ILoadBasedSelector
    {
        Int32 _last;

        internal BasicLoadBasedSelector(Int32 initialValue = 0)
        {
            _last = initialValue - 1;
        }

        // balances load, chosing the first with 0 load, or the one with less load
        public T Select<T>(T[] elements) where T : ILoadMeasurable
        {
            Contract.Assert(elements.Any(), "Cannot select ILoadMeasurable element from an empty list.");

            T minElement = default(T);
            var minValue = Int32.MaxValue;
            unchecked
            {
                var last = Interlocked.Increment(ref _last);
                for (int i = 0; i < elements.Length; i++)
                {
                    var index = Math.Abs((last + i) % elements.Length);
                    var load = elements[index].CurrentLoad;
                    if (load == 0)
                    {
                        return elements[index];
                    }
                    else if (load < minValue)
                    {
                        minValue = load;
                        minElement = elements[index];
                    }
                }
            }
            return minElement;
        }
    }
}
