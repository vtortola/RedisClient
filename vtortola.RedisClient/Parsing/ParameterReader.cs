using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Linq;
using System.Threading;

namespace vtortola.Redis
{
    internal sealed class ParameterReader<T>
    {
        internal static readonly ParameterReader<T> Empty = new ParameterReader<T>();
        static Dictionary<String, Func<T, IEnumerable<String>>> _accessorCollection;
        readonly T _obj;

        internal ParameterReader()
        { 
        }

        internal ParameterReader(T obj)
        {
            Contract.Assert(obj != null, "Calling ParameterReader(obj) with a null value.");
            _obj = obj;
        }

        internal IEnumerable<String> GetValues(String parameterName, Boolean failOnMissingParameter = true)
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
            return accessor(_obj);
        }
    }
}
