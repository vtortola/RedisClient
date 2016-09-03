  
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics.Contracts;
using System.Linq; 
using System.Reflection;

/*
     ###    THIS IS AN AUTOMATICALLY GENERATED FILE    ###
 */

namespace vtortola.Redis
{
	internal static class GetterHelper
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

        internal static Func<T, IEnumerable<String>> CreateGetter<T>(PropertyInfo property)
        {
            var propertyAccessor = property.GetGetMethod();
			var propertyType = property.PropertyType;

            if (propertyType == StringType)
            {
                return GetStringGetter<T>(propertyAccessor);
            }
            if (IEnumerableType.IsAssignableFrom(propertyType))
            {
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    propertyType = propertyType.GetGenericArguments()[0];
                }
                else
                {
                    propertyType = propertyType
                        .GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        .Select(i => i.GetGenericArguments()[0])
                        .Single();
                }

                if (propertyType.IsEnum)
                    propertyType = Enum.GetUnderlyingType(propertyType);

                 if (propertyType == StringType)
                    return GetStringEnumerableGetter<T>(propertyAccessor);
				 if (propertyType == DateTimeType)
                    return GetDateTimeEnumerableGetter<T>(propertyAccessor);
				 if (propertyType == CharType)
                    return GetCharEnumerableGetter<T>(propertyAccessor);
				 if (propertyType == Int16Type)
                    return GetInt16EnumerableGetter<T>(propertyAccessor);
				 if (propertyType == Int32Type)
                    return GetInt32EnumerableGetter<T>(propertyAccessor);
				 if (propertyType == Int64Type)
                    return GetInt64EnumerableGetter<T>(propertyAccessor);
				 if (propertyType == SingleType)
                    return GetSingleEnumerableGetter<T>(propertyAccessor);
				 if (propertyType == DoubleType)
                    return GetDoubleEnumerableGetter<T>(propertyAccessor);
				 if (propertyType == DecimalType)
                    return GetDecimalEnumerableGetter<T>(propertyAccessor);
				 if (propertyType == UInt16Type)
                    return GetUInt16EnumerableGetter<T>(propertyAccessor);
				 if (propertyType == UInt32Type)
                    return GetUInt32EnumerableGetter<T>(propertyAccessor);
				 if (propertyType == UInt64Type)
                    return GetUInt64EnumerableGetter<T>(propertyAccessor);
				            }
            else
            {
				if (propertyType.IsEnum)
	                propertyType = Enum.GetUnderlyingType(propertyType);

				if (propertyType == DateTimeType)
                    return GetDateTimeGetter<T>(propertyAccessor);
				 if (propertyType == CharType)
					return GetCharGetter<T>(propertyAccessor);
				 if (propertyType == Int16Type)
					return GetInt16Getter<T>(propertyAccessor);
				 if (propertyType == Int32Type)
					return GetInt32Getter<T>(propertyAccessor);
				 if (propertyType == Int64Type)
					return GetInt64Getter<T>(propertyAccessor);
				 if (propertyType == SingleType)
					return GetSingleGetter<T>(propertyAccessor);
				 if (propertyType == DoubleType)
					return GetDoubleGetter<T>(propertyAccessor);
				 if (propertyType == DecimalType)
					return GetDecimalGetter<T>(propertyAccessor);
				 if (propertyType == UInt16Type)
					return GetUInt16Getter<T>(propertyAccessor);
				 if (propertyType == UInt32Type)
					return GetUInt32Getter<T>(propertyAccessor);
				 if (propertyType == UInt64Type)
					return GetUInt64Getter<T>(propertyAccessor);
				            }

            throw new RedisClientBindingException("The type '" + propertyType.Name + "' is not supported as parameter member.\n" +
                                                "Only members of type Enum, DateTime, Char, String, Int16, Int32, Int64, Single, Double, Decimal and collections of them are supported.\n" +
                                                "Consider using Parameter.Collate to produce the right parameters.");
        }

		// STRING
		static Func<T, IEnumerable<String>> GetStringEnumerableGetter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, IEnumerable<String>>)Delegate.CreateDelegate(typeof(Func<T, IEnumerable<String>>), propertyAccessor);
            return (T obj) => function(obj);
        }
		static Func<T, IEnumerable<String>> GetStringGetter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, String>)Delegate.CreateDelegate(typeof(Func<T, String>), propertyAccessor);
            return (T obj) => YieldReturn(function(obj));
        }
		// DATETIME
        static Func<T, IEnumerable<String>> GetDateTimeEnumerableGetter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, IEnumerable<DateTime>>)Delegate.CreateDelegate(typeof(Func<T, IEnumerable<DateTime>>), propertyAccessor);
            return (T obj) => function(obj).Select(x=>x.ToBinary().ToString(RESPObject.FormatInfo));
        }
        static Func<T, IEnumerable<String>> GetDateTimeGetter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, DateTime>)Delegate.CreateDelegate(typeof(Func<T, DateTime>), propertyAccessor);
            return (T obj) => YieldReturn(function(obj).ToBinary().ToString(RESPObject.FormatInfo));
        }
		 
		// CHAR
		static Func<T, IEnumerable<String>> GetCharEnumerableGetter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, IEnumerable<Char>>)Delegate.CreateDelegate(typeof(Func<T, IEnumerable<Char>>), propertyAccessor);
            return (T obj) => function(obj).Select(c => c.ToString(RESPObject.FormatInfo));
        }
		static Func<T, IEnumerable<String>> GetCharGetter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, Char>)Delegate.CreateDelegate(typeof(Func<T, Char>), propertyAccessor);
            return (T obj) => YieldReturn(function(obj).ToString());
        }
		 
		// INT16
		static Func<T, IEnumerable<String>> GetInt16EnumerableGetter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, IEnumerable<Int16>>)Delegate.CreateDelegate(typeof(Func<T, IEnumerable<Int16>>), propertyAccessor);
            return (T obj) => function(obj).Select(c => c.ToString(RESPObject.FormatInfo));
        }
		static Func<T, IEnumerable<String>> GetInt16Getter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, Int16>)Delegate.CreateDelegate(typeof(Func<T, Int16>), propertyAccessor);
            return (T obj) => YieldReturn(function(obj).ToString());
        }
		 
		// INT32
		static Func<T, IEnumerable<String>> GetInt32EnumerableGetter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, IEnumerable<Int32>>)Delegate.CreateDelegate(typeof(Func<T, IEnumerable<Int32>>), propertyAccessor);
            return (T obj) => function(obj).Select(c => c.ToString(RESPObject.FormatInfo));
        }
		static Func<T, IEnumerable<String>> GetInt32Getter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, Int32>)Delegate.CreateDelegate(typeof(Func<T, Int32>), propertyAccessor);
            return (T obj) => YieldReturn(function(obj).ToString());
        }
		 
		// INT64
		static Func<T, IEnumerable<String>> GetInt64EnumerableGetter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, IEnumerable<Int64>>)Delegate.CreateDelegate(typeof(Func<T, IEnumerable<Int64>>), propertyAccessor);
            return (T obj) => function(obj).Select(c => c.ToString(RESPObject.FormatInfo));
        }
		static Func<T, IEnumerable<String>> GetInt64Getter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, Int64>)Delegate.CreateDelegate(typeof(Func<T, Int64>), propertyAccessor);
            return (T obj) => YieldReturn(function(obj).ToString());
        }
		 
		// SINGLE
		static Func<T, IEnumerable<String>> GetSingleEnumerableGetter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, IEnumerable<Single>>)Delegate.CreateDelegate(typeof(Func<T, IEnumerable<Single>>), propertyAccessor);
            return (T obj) => function(obj).Select(c => c.ToString(RESPObject.FormatInfo));
        }
		static Func<T, IEnumerable<String>> GetSingleGetter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, Single>)Delegate.CreateDelegate(typeof(Func<T, Single>), propertyAccessor);
            return (T obj) => YieldReturn(function(obj).ToString());
        }
		 
		// DOUBLE
		static Func<T, IEnumerable<String>> GetDoubleEnumerableGetter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, IEnumerable<Double>>)Delegate.CreateDelegate(typeof(Func<T, IEnumerable<Double>>), propertyAccessor);
            return (T obj) => function(obj).Select(c => c.ToString(RESPObject.FormatInfo));
        }
		static Func<T, IEnumerable<String>> GetDoubleGetter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, Double>)Delegate.CreateDelegate(typeof(Func<T, Double>), propertyAccessor);
            return (T obj) => YieldReturn(function(obj).ToString());
        }
		 
		// DECIMAL
		static Func<T, IEnumerable<String>> GetDecimalEnumerableGetter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, IEnumerable<Decimal>>)Delegate.CreateDelegate(typeof(Func<T, IEnumerable<Decimal>>), propertyAccessor);
            return (T obj) => function(obj).Select(c => c.ToString(RESPObject.FormatInfo));
        }
		static Func<T, IEnumerable<String>> GetDecimalGetter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, Decimal>)Delegate.CreateDelegate(typeof(Func<T, Decimal>), propertyAccessor);
            return (T obj) => YieldReturn(function(obj).ToString());
        }
		 
		// UINT16
		static Func<T, IEnumerable<String>> GetUInt16EnumerableGetter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, IEnumerable<UInt16>>)Delegate.CreateDelegate(typeof(Func<T, IEnumerable<UInt16>>), propertyAccessor);
            return (T obj) => function(obj).Select(c => c.ToString(RESPObject.FormatInfo));
        }
		static Func<T, IEnumerable<String>> GetUInt16Getter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, UInt16>)Delegate.CreateDelegate(typeof(Func<T, UInt16>), propertyAccessor);
            return (T obj) => YieldReturn(function(obj).ToString());
        }
		 
		// UINT32
		static Func<T, IEnumerable<String>> GetUInt32EnumerableGetter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, IEnumerable<UInt32>>)Delegate.CreateDelegate(typeof(Func<T, IEnumerable<UInt32>>), propertyAccessor);
            return (T obj) => function(obj).Select(c => c.ToString(RESPObject.FormatInfo));
        }
		static Func<T, IEnumerable<String>> GetUInt32Getter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, UInt32>)Delegate.CreateDelegate(typeof(Func<T, UInt32>), propertyAccessor);
            return (T obj) => YieldReturn(function(obj).ToString());
        }
		 
		// UINT64
		static Func<T, IEnumerable<String>> GetUInt64EnumerableGetter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, IEnumerable<UInt64>>)Delegate.CreateDelegate(typeof(Func<T, IEnumerable<UInt64>>), propertyAccessor);
            return (T obj) => function(obj).Select(c => c.ToString(RESPObject.FormatInfo));
        }
		static Func<T, IEnumerable<String>> GetUInt64Getter<T>(MethodInfo propertyAccessor)
        {
            var function = (Func<T, UInt64>)Delegate.CreateDelegate(typeof(Func<T, UInt64>), propertyAccessor);
            return (T obj) => YieldReturn(function(obj).ToString());
        }
		
		static IEnumerable<String> YieldReturn(String value)
        {
			if(value == null)
				throw new RedisClientBindingException("Null values are not accepted as parameters.");
            yield return value;
        }
	}
}