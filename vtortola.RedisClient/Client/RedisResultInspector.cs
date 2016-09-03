using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    [DebuggerDisplay("{RedisType}: {_response.ToString()} ")]
    internal sealed class RedisResultInspector : IRedisResultInspector
    {
        readonly RESPObject _response;
        readonly Int32 _lineNumber;

        public RedisType RedisType { get; private set; }

        internal RedisResultInspector(RESPObject response, Int32 lineNumber)
        {
            Contract.Assert(response != null, "Result instpector without response object.");
            Contract.Assert(lineNumber > 0, "Result inspectors start counting numbers from 1.");

            _response = response;
            _lineNumber = lineNumber;
            RedisType = GetRedisType();
        }
        private RedisType GetRedisType()
        {
            switch (_response.Header)
            {
                case RESPHeaders.SimpleString:
                case RESPHeaders.BulkString:
                    return RedisType.String;

                case RESPHeaders.Integer:
                    return RedisType.Integer;

                case RESPHeaders.Array:
                    return RedisType.Array;

                case RESPHeaders.Error:
                    return RedisType.Error;

                default: throw new InvalidOperationException("Unexpected RESP header " + _response.Header);
            }
        }
        public String GetString()
        {
            CheckException();
            return _response.GetString();
        }
        public Int64 GetInteger()
        {
            CheckException();
            return _response.GetInt64();
        }
        public String AsString()
        {
            CheckException();
            return _response.AsString();
        }
        public Int64 AsInteger()
        {
            CheckException();
            return _response.AsInt64();
        }
        public Double AsDouble()
        {
            CheckException();
            return _response.AsDouble();
        }
        public TEnum AsEnum<TEnum>()
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("TEnum must be an enum.");

            CheckException();

            return (TEnum)Enum.Parse(typeof(TEnum), _response.AsString());
        }

        public void AssertOK()
        {
            CheckException();
            if (!RESPString.IsString(_response.Header))
                throw new RedisClientAssertException("Expected result for command was 'OK' but a '" + this.RedisType + "' was received instead");
            var value = _response.GetString();
            if (!"OK".Equals(value, StringComparison.Ordinal))
                throw new RedisClientAssertException("Expected result for command was 'OK' but result was '" + (value ?? "<null>") + "'");
        }
        public RedisClientCommandException GetException()
        {
            if (_response.Header == RESPHeaders.Error)
            {
                return new RedisClientCommandException((RESPError)_response, _lineNumber);
            }
            else
                return null;
        }
        private void CheckException()
        {
            if (_response.Header == RESPHeaders.Error)
                throw new RedisClientCommandException((RESPError)_response, _lineNumber);
        }

        public IRedisResults AsResults()
        {
            CheckException();
            return new RedisResults(_response.Cast<RESPArray>().ToArray(), null);
        }

        public IDictionary<TKey, TValue> AsDictionaryCollation<TKey, TValue>()
        {
            CheckException();

            var dictionary = new Dictionary<TKey, TValue>();
            var complex = _response.Cast<RESPArray>();

            var keyFormatter = FormatterHelper.Formatter<TKey>();
            var valueFormatter = FormatterHelper.Formatter<TValue>();

            try
            {
                for (int i = 0; i < complex.Count; i += 2)
                    dictionary.Add(keyFormatter(complex[i]), valueFormatter(i < complex.Count ? complex[i + 1] : null));
            }
            catch(Exception ex)
            {
                throw new RedisClientBindingException(String.Format("Cannot create Dictionary<{0},{1}> from response to command in line number {2}.", typeof(TKey).Name, typeof(TValue).Name, _lineNumber ),ex);
            }

            return dictionary;
        }

        public void AsObjectCollation<T>(T instance, Boolean ignoreMissingMembers=true, Boolean ignoreTypeMissmatchMembers = true)
        {
            var complex = _response.Cast<RESPArray>();
            ObjectBinder<T>.Bind(complex, instance, ignoreMissingMembers, ignoreTypeMissmatchMembers);
        }

        public T AsObjectCollation<T>(Boolean ignoreMissingMembers = true, Boolean ignoreTypeMissmatchMembers = true) where T : new()
        {
            T instance = new T();
            AsObjectCollation(instance, ignoreMissingMembers, ignoreTypeMissmatchMembers);
            return instance;
        }

        public String[] GetStringArray()
        {
            CheckException();

            var complex = _response.Cast<RESPArray>();
            var result = new String[complex.Count];

            for (int i = 0; i < complex.Count; i++)
                result[i] = complex[i].GetString();

            return result;
        }

        public String[] AsStringArray()
        {
            CheckException();

            var complex = _response.Cast<RESPArray>();
            var result = new String[complex.Count];

            for (int i = 0; i < complex.Count; i++)
                result[i] = complex[i].AsString();

            return result;
        }

        public Int64[] AsIntegerArray()
        {
            CheckException();

            var complex = _response.Cast<RESPArray>();
            var result = new Int64[complex.Count];

            for (int i = 0; i < complex.Count; i++)
                result[i] = complex[i].AsInt64();

            return result;
        }

        public Int64[] GetIntegerArray()
        {
            CheckException();

            var complex = _response.Cast<RESPArray>();
            var result = new Int64[complex.Count];

            for (int i = 0; i < complex.Count; i++)
                result[i] = complex[i].GetInt64();

            return result;
        }
        public Object[] GetArray()
        {
            CheckException();

            var complex = _response.Cast<RESPArray>();
            var result = new Object[complex.Count];

            for (int i = 0; i < complex.Count; i++)
            {
                var element = complex[i];
                switch (element.Header)
                {
                    case RESPHeaders.Array:
                        throw new RedisClientBindingException("Redis arrays cannot be bound to a member of a .NET array. Use .AsResults() instead.");
                    case RESPHeaders.Integer:
                        result[i] = element.GetInt64();
                        break;
                    case RESPHeaders.BulkString:
                    case RESPHeaders.SimpleString:
                        result[i] = element.GetString();
                        break;
                    case RESPHeaders.Error:
                        result[i] = new RedisClientCommandException((RESPError)element);
                        break;
                    default:
                        throw new InvalidOperationException("Unexpected RESP header " + element.Header);
                }
            }

            return result;
        }
    }
}
