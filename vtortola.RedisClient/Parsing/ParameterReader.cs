using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace vtortola.Redis
{
    internal static class ParameterReader<T>
    {
        static Dictionary<String, Func<T, IEnumerable<String>>> _accessorCollection;

        internal static IEnumerable<String> GetValues(T obj, String parameterName, Boolean failOnMissingParameter = true)
        {
            if (_accessorCollection == null)
            {
                var accessors = Parameter.Create<T>();
                Interlocked.CompareExchange(ref _accessorCollection, accessors, null);
            }

            if (_accessorCollection == null)
                throw new RedisClientBindingException("There are no parameters defined.");

            Func<T, IEnumerable<String>> accessor;
            if (!_accessorCollection.TryGetValue(parameterName, out accessor))
            {
                if (failOnMissingParameter)
                    throw new RedisClientBindingException("The parameter key '" + parameterName + "' does not exists in the given parameter data.");
                else
                    return Enumerable.Empty<String>();
            }
            return accessor(obj);
        }
    }
}
