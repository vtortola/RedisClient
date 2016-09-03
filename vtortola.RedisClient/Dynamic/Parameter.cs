using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;

namespace vtortola.Redis
{
    /// <summary>
    /// Helper class to sequence parameters
    /// </summary>
    public static class Parameter
    {
        static class ObjectParameterHelper<T> where T : class
        {
            static Dictionary<String, Func<T, IEnumerable<String>>> _accessors;

            internal static IEnumerable<String> SequenceProperties(T obj)
            {
                if (obj == null)
                    yield break;

                var type = typeof(T);

                if (_accessors == null)
                {
                    var accessors = Create<T>();
                    Interlocked.CompareExchange(ref _accessors, accessors, null);
                }

                foreach (var accessor in _accessors)
                {
                    yield return accessor.Key;
                    foreach (var result in accessor.Value(obj))
                        yield return result;
                }
            }
        }
        static class TupleParameterHelper<TKey, TValue>
        {
            static Dictionary<String, Func<Tuple<TKey, TValue>, IEnumerable<String>>> _accessors;

            internal static IEnumerable<String> Sequence(IEnumerable<Tuple<TKey, TValue>> tuples)
            {
                if (_accessors == null)
                {
                    var accessors = Create<Tuple<TKey, TValue>>("Item1", "Item2");
                    Interlocked.CompareExchange(ref _accessors, accessors, null);
                }

                foreach (var item in tuples)
                {
                    foreach (var accessor in _accessors)
                    {
                        foreach (var result in accessor.Value(item))
                            yield return result;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a sequence the key-values representing the properties and their values.
        /// Ex: property1 value1 property2 value2 ... etc...
        /// </summary>
        public static IEnumerable<String> SequenceProperties<T>(T obj)
            where T : class
        {
            return ObjectParameterHelper<T>.SequenceProperties(obj);
        }

        /// <summary>
        /// Returns a sequence the key-values representing the properties and their values.
        /// Ex: Item1 Item2 Item1 Item2 ... etc...
        /// </summary>
        public static IEnumerable<String> Sequence<TKey, TValue>(IEnumerable<Tuple<TKey, TValue>> tuples)
        {
            return TupleParameterHelper<TKey, TValue>.Sequence(tuples);
        }

        /// <summary>
        /// Returns a sequence the key-values.
        /// Ex: Key Value Key Value ... etc...
        /// </summary>
        public static IEnumerable<String> Sequence<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> enumeration)
        {
            var formatKey = FormatterHelper.StringFormatter<TKey>();
            var formatValue = FormatterHelper.StringFormatter<TValue>();

            foreach (var item in enumeration)
            {
                yield return formatKey(item.Key);
                yield return formatValue(item.Value);
            }
        }

        internal static Dictionary<String, Func<T, IEnumerable<String>>> Create<T>(params String[] names)
        {
            var type = typeof(T);
            var accessors = new Dictionary<String, Func<T, IEnumerable<String>>>(StringComparer.OrdinalIgnoreCase);
            if (names != null && names.Length > 0)
            {
                foreach (var name in names)
                    accessors.Add(name, GetterHelper.CreateGetter<T>(type.GetProperty(name)));
            }
            else
            {
                foreach (var property in type.GetProperties())
                    accessors.Add(property.Name, GetterHelper.CreateGetter<T>(property));
            }
            return accessors;
        }
    }
}
