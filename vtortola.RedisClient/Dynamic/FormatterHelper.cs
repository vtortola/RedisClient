  
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics.Contracts;
using System.Linq; 
using System.Reflection;

namespace vtortola.Redis
{
	internal static class FormatterHelper
	{
		static readonly Type IEnumerableType = typeof(IEnumerable);
		static readonly Type StringType = typeof(String);
		static readonly Type DateTimeType = typeof(DateTime);
		static readonly Type CharType = typeof(Char);
		static readonly Type Int16Type = typeof(Int16);
		static readonly Type Int32Type = typeof(Int32);
		static readonly Type Int64Type = typeof(Int64);
		static readonly Type SingleType = typeof(Single);
		static readonly Type DoubleType = typeof(Double);
		static readonly Type DecimalType = typeof(Decimal);
		static readonly Type UInt16Type = typeof(UInt16);
		static readonly Type UInt32Type = typeof(UInt32);
		static readonly Type UInt64Type = typeof(UInt64);
		
		static readonly HashSet<Type> _supported = new HashSet<Type>(new[]
		{
			StringType, DateTimeType, typeof(Nullable<DateTime>),  CharType, typeof(Nullable<Char>),  Int16Type, typeof(Nullable<Int16>),  Int32Type, typeof(Nullable<Int32>),  Int64Type, typeof(Nullable<Int64>),  SingleType, typeof(Nullable<Single>),  DoubleType, typeof(Nullable<Double>),  DecimalType, typeof(Nullable<Decimal>),  UInt16Type, typeof(Nullable<UInt16>),  UInt32Type, typeof(Nullable<UInt32>),  UInt64Type, typeof(Nullable<UInt64>),  
		});

        internal static Boolean IsSupported(Type type)
        {
			var underlying = Nullable.GetUnderlyingType(type);
            
			if(underlying != null && underlying.IsEnum)
				throw new RedisClientBindingException("Nullable enums are not supported at this moment. Consider including a 'None' value in your enumeration.");

			return type.IsEnum || _supported.Contains(type);
        }

		internal static Func<T, String> StringFormatter<T>()
        {
            var type = typeof(T);
            if(!_supported.Contains(type))
            {
				throw new RedisClientBindingException("The type '" + type.Name + "' is not supported as parameter member.\n" +
                                    "Only members of type Char, String, Int16, Int32, Int64, Single, Double, Decimal and collections of them are supported.\n" +
                                    "Consider using Parameter.Collate to produce the right parameters.");

			}

            return (T obj) => (String)Convert.ChangeType(obj, StringType, RESPObject.FormatInfo);
        }

        internal static Func<RESPObject, T> Formatter<T>()
        {
            return (RESPObject obj) =>
            {
                if (obj == null)
                    return default(T);

                switch (obj.Header)
                {
                    case RESPHeaders.Integer:
                        return (T)Convert.ChangeType(obj.AsInt64(), typeof(T), RESPObject.FormatInfo);
                    case RESPHeaders.BulkString:
                    case RESPHeaders.SimpleString:
                        return (T)Convert.ChangeType(obj.AsString(), typeof(T), RESPObject.FormatInfo);
                    case RESPHeaders.Error:
                        throw new RedisClientCommandException((RESPError)obj);
                    default:
                        throw new RedisClientBindingException(obj.GetType().Name + " cannot be formatted into " + typeof(T));
                }
            };
        }
	}
}