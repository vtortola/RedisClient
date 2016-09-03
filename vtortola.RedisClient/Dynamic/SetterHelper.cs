  
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics.Contracts;
using System.Linq; 
using System.Reflection;

namespace vtortola.Redis
{
	internal static class SetterHelper
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


		internal static Setter<T> CreateSetter<T>(PropertyInfo property)
        {
            var propertyAccessor = property.GetSetMethod();
			var propertyType = property.PropertyType;

            if (propertyType == StringType)
            {
                return new Setter<T>()
                {
                    StringSetter = GetStringStringSetter<T>(propertyAccessor),
                    NumericSetter = GetNumericStringSetter<T>(propertyAccessor)
                };
            }
			var underlying = Nullable.GetUnderlyingType(propertyType);
			if(underlying != null)
			{
			    if(underlying == DateTimeType)
				{
					return new Setter<T>()
					{
						StringSetter = GetNullableStringDateTimeSetter<T>(propertyAccessor),
						NumericSetter = GetNullableNumericDateTimeSetter<T>(propertyAccessor)
					};
				}
				if (underlying == CharType)
				{
					return new Setter<T>()
					{
						StringSetter = GetNullableStringCharSetter<T>(propertyAccessor),
						NumericSetter = GetNullableNumericCharSetter<T>(propertyAccessor)
					};
				}
				if (underlying == Int16Type)
				{
					return new Setter<T>()
					{
						StringSetter = GetNullableStringInt16Setter<T>(propertyAccessor),
						NumericSetter = GetNullableNumericInt16Setter<T>(propertyAccessor)
					};
				}
				if (underlying == Int32Type)
				{
					return new Setter<T>()
					{
						StringSetter = GetNullableStringInt32Setter<T>(propertyAccessor),
						NumericSetter = GetNullableNumericInt32Setter<T>(propertyAccessor)
					};
				}
				if (underlying == Int64Type)
				{
					return new Setter<T>()
					{
						StringSetter = GetNullableStringInt64Setter<T>(propertyAccessor),
						NumericSetter = GetNullableNumericInt64Setter<T>(propertyAccessor)
					};
				}
				if (underlying == SingleType)
				{
					return new Setter<T>()
					{
						StringSetter = GetNullableStringSingleSetter<T>(propertyAccessor),
						NumericSetter = GetNullableNumericSingleSetter<T>(propertyAccessor)
					};
				}
				if (underlying == DoubleType)
				{
					return new Setter<T>()
					{
						StringSetter = GetNullableStringDoubleSetter<T>(propertyAccessor),
						NumericSetter = GetNullableNumericDoubleSetter<T>(propertyAccessor)
					};
				}
				if (underlying == DecimalType)
				{
					return new Setter<T>()
					{
						StringSetter = GetNullableStringDecimalSetter<T>(propertyAccessor),
						NumericSetter = GetNullableNumericDecimalSetter<T>(propertyAccessor)
					};
				}
				if (underlying == UInt16Type)
				{
					return new Setter<T>()
					{
						StringSetter = GetNullableStringUInt16Setter<T>(propertyAccessor),
						NumericSetter = GetNullableNumericUInt16Setter<T>(propertyAccessor)
					};
				}
				if (underlying == UInt32Type)
				{
					return new Setter<T>()
					{
						StringSetter = GetNullableStringUInt32Setter<T>(propertyAccessor),
						NumericSetter = GetNullableNumericUInt32Setter<T>(propertyAccessor)
					};
				}
				if (underlying == UInt64Type)
				{
					return new Setter<T>()
					{
						StringSetter = GetNullableStringUInt64Setter<T>(propertyAccessor),
						NumericSetter = GetNullableNumericUInt64Setter<T>(propertyAccessor)
					};
				}
				}
			else
			{
				if (propertyType.IsEnum)
					propertyType = Enum.GetUnderlyingType(propertyType);

				if(propertyType == DateTimeType)
				{
					return new Setter<T>()
					{
						StringSetter = GetStringDateTimeSetter<T>(propertyAccessor),
						NumericSetter = GetNumericDateTimeSetter<T>(propertyAccessor)
					};
				}
				if (propertyType == CharType)
				{
					return new Setter<T>()
					{
						StringSetter = GetStringCharSetter<T>(propertyAccessor),
						NumericSetter = GetNumericCharSetter<T>(propertyAccessor)
					};
				}
				if (propertyType == Int16Type)
				{
					return new Setter<T>()
					{
						StringSetter = GetStringInt16Setter<T>(propertyAccessor),
						NumericSetter = GetNumericInt16Setter<T>(propertyAccessor)
					};
				}
				if (propertyType == Int32Type)
				{
					return new Setter<T>()
					{
						StringSetter = GetStringInt32Setter<T>(propertyAccessor),
						NumericSetter = GetNumericInt32Setter<T>(propertyAccessor)
					};
				}
				if (propertyType == Int64Type)
				{
					return new Setter<T>()
					{
						StringSetter = GetStringInt64Setter<T>(propertyAccessor),
						NumericSetter = GetNumericInt64Setter<T>(propertyAccessor)
					};
				}
				if (propertyType == SingleType)
				{
					return new Setter<T>()
					{
						StringSetter = GetStringSingleSetter<T>(propertyAccessor),
						NumericSetter = GetNumericSingleSetter<T>(propertyAccessor)
					};
				}
				if (propertyType == DoubleType)
				{
					return new Setter<T>()
					{
						StringSetter = GetStringDoubleSetter<T>(propertyAccessor),
						NumericSetter = GetNumericDoubleSetter<T>(propertyAccessor)
					};
				}
				if (propertyType == DecimalType)
				{
					return new Setter<T>()
					{
						StringSetter = GetStringDecimalSetter<T>(propertyAccessor),
						NumericSetter = GetNumericDecimalSetter<T>(propertyAccessor)
					};
				}
				if (propertyType == UInt16Type)
				{
					return new Setter<T>()
					{
						StringSetter = GetStringUInt16Setter<T>(propertyAccessor),
						NumericSetter = GetNumericUInt16Setter<T>(propertyAccessor)
					};
				}
				if (propertyType == UInt32Type)
				{
					return new Setter<T>()
					{
						StringSetter = GetStringUInt32Setter<T>(propertyAccessor),
						NumericSetter = GetNumericUInt32Setter<T>(propertyAccessor)
					};
				}
				if (propertyType == UInt64Type)
				{
					return new Setter<T>()
					{
						StringSetter = GetStringUInt64Setter<T>(propertyAccessor),
						NumericSetter = GetNumericUInt64Setter<T>(propertyAccessor)
					};
				}
							}

            throw new RedisClientBindingException("The type '" + propertyType.Name + "' is not supported as output member.\n" +
                                                "Only members of type DateTime, Char, String, Int16, Int32, Int64, Single, Double, Decimal, nullables and collections of them are supported.\n" +
                                                "Consider using Parameter.Collate to produce the right parameters.");
		}

		// STRING
		static Action<T, Int64> GetNumericStringSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, String>)Delegate.CreateDelegate(typeof(Action<T, String>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, value.ToString(RESPObject.FormatInfo));
        }
        static Action<T, String> GetStringStringSetter<T>(MethodInfo propertyAccessor)
        {
            return (Action<T, String>)Delegate.CreateDelegate(typeof(Action<T, String>), propertyAccessor);
        }
		// DATETIME
        static Action<T, String> GetStringDateTimeSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, DateTime>)Delegate.CreateDelegate(typeof(Action<T, DateTime>), propertyAccessor);
            return (T obj, String str) => func(obj, DateTime.FromBinary(Int64.Parse(str)));
        }
        static Action<T, String> GetNullableStringDateTimeSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<DateTime>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<DateTime>>), propertyAccessor);
            return (T obj, String str) => func(obj, str == null ? (Nullable<DateTime>)null : DateTime.FromBinary(Int64.Parse(str)));
        }
        static Action<T, Int64> GetNumericDateTimeSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, DateTime>)Delegate.CreateDelegate(typeof(Action<T, DateTime>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, DateTime.FromBinary(value));
        }
        static Action<T, Int64> GetNullableNumericDateTimeSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<DateTime>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<DateTime>>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, DateTime.FromBinary(value));
        }
		 
		// CHAR
        static Action<T, String> GetStringCharSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Char>)Delegate.CreateDelegate(typeof(Action<T, Char>), propertyAccessor);
            return (T obj, String str) => func(obj, Char.Parse(str));
        }
		static Action<T, String> GetNullableStringCharSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<Char>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<Char>>), propertyAccessor);
            return (T obj, String str) => func(obj, str == null? (Nullable<Char>)null : Char.Parse(str));
        }
        static Action<T, Int64> GetNumericCharSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Char>)Delegate.CreateDelegate(typeof(Action<T, Char>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToChar(value));
        }
		static Action<T, Int64> GetNullableNumericCharSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<Char>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<Char>>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToChar(value));
        }
		 
		// INT16
        static Action<T, String> GetStringInt16Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Int16>)Delegate.CreateDelegate(typeof(Action<T, Int16>), propertyAccessor);
            return (T obj, String str) => func(obj, Int16.Parse(str));
        }
		static Action<T, String> GetNullableStringInt16Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<Int16>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<Int16>>), propertyAccessor);
            return (T obj, String str) => func(obj, str == null? (Nullable<Int16>)null : Int16.Parse(str));
        }
        static Action<T, Int64> GetNumericInt16Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Int16>)Delegate.CreateDelegate(typeof(Action<T, Int16>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToInt16(value));
        }
		static Action<T, Int64> GetNullableNumericInt16Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<Int16>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<Int16>>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToInt16(value));
        }
		 
		// INT32
        static Action<T, String> GetStringInt32Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Int32>)Delegate.CreateDelegate(typeof(Action<T, Int32>), propertyAccessor);
            return (T obj, String str) => func(obj, Int32.Parse(str));
        }
		static Action<T, String> GetNullableStringInt32Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<Int32>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<Int32>>), propertyAccessor);
            return (T obj, String str) => func(obj, str == null? (Nullable<Int32>)null : Int32.Parse(str));
        }
        static Action<T, Int64> GetNumericInt32Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Int32>)Delegate.CreateDelegate(typeof(Action<T, Int32>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToInt32(value));
        }
		static Action<T, Int64> GetNullableNumericInt32Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<Int32>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<Int32>>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToInt32(value));
        }
		 
		// INT64
        static Action<T, String> GetStringInt64Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Int64>)Delegate.CreateDelegate(typeof(Action<T, Int64>), propertyAccessor);
            return (T obj, String str) => func(obj, Int64.Parse(str));
        }
		static Action<T, String> GetNullableStringInt64Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<Int64>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<Int64>>), propertyAccessor);
            return (T obj, String str) => func(obj, str == null? (Nullable<Int64>)null : Int64.Parse(str));
        }
        static Action<T, Int64> GetNumericInt64Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Int64>)Delegate.CreateDelegate(typeof(Action<T, Int64>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToInt64(value));
        }
		static Action<T, Int64> GetNullableNumericInt64Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<Int64>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<Int64>>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToInt64(value));
        }
		 
		// SINGLE
        static Action<T, String> GetStringSingleSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Single>)Delegate.CreateDelegate(typeof(Action<T, Single>), propertyAccessor);
            return (T obj, String str) => func(obj, Single.Parse(str));
        }
		static Action<T, String> GetNullableStringSingleSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<Single>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<Single>>), propertyAccessor);
            return (T obj, String str) => func(obj, str == null? (Nullable<Single>)null : Single.Parse(str));
        }
        static Action<T, Int64> GetNumericSingleSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Single>)Delegate.CreateDelegate(typeof(Action<T, Single>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToSingle(value));
        }
		static Action<T, Int64> GetNullableNumericSingleSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<Single>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<Single>>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToSingle(value));
        }
		 
		// DOUBLE
        static Action<T, String> GetStringDoubleSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Double>)Delegate.CreateDelegate(typeof(Action<T, Double>), propertyAccessor);
            return (T obj, String str) => func(obj, Double.Parse(str));
        }
		static Action<T, String> GetNullableStringDoubleSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<Double>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<Double>>), propertyAccessor);
            return (T obj, String str) => func(obj, str == null? (Nullable<Double>)null : Double.Parse(str));
        }
        static Action<T, Int64> GetNumericDoubleSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Double>)Delegate.CreateDelegate(typeof(Action<T, Double>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToDouble(value));
        }
		static Action<T, Int64> GetNullableNumericDoubleSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<Double>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<Double>>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToDouble(value));
        }
		 
		// DECIMAL
        static Action<T, String> GetStringDecimalSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Decimal>)Delegate.CreateDelegate(typeof(Action<T, Decimal>), propertyAccessor);
            return (T obj, String str) => func(obj, Decimal.Parse(str));
        }
		static Action<T, String> GetNullableStringDecimalSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<Decimal>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<Decimal>>), propertyAccessor);
            return (T obj, String str) => func(obj, str == null? (Nullable<Decimal>)null : Decimal.Parse(str));
        }
        static Action<T, Int64> GetNumericDecimalSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Decimal>)Delegate.CreateDelegate(typeof(Action<T, Decimal>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToDecimal(value));
        }
		static Action<T, Int64> GetNullableNumericDecimalSetter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<Decimal>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<Decimal>>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToDecimal(value));
        }
		 
		// UINT16
        static Action<T, String> GetStringUInt16Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, UInt16>)Delegate.CreateDelegate(typeof(Action<T, UInt16>), propertyAccessor);
            return (T obj, String str) => func(obj, UInt16.Parse(str));
        }
		static Action<T, String> GetNullableStringUInt16Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<UInt16>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<UInt16>>), propertyAccessor);
            return (T obj, String str) => func(obj, str == null? (Nullable<UInt16>)null : UInt16.Parse(str));
        }
        static Action<T, Int64> GetNumericUInt16Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, UInt16>)Delegate.CreateDelegate(typeof(Action<T, UInt16>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToUInt16(value));
        }
		static Action<T, Int64> GetNullableNumericUInt16Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<UInt16>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<UInt16>>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToUInt16(value));
        }
		 
		// UINT32
        static Action<T, String> GetStringUInt32Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, UInt32>)Delegate.CreateDelegate(typeof(Action<T, UInt32>), propertyAccessor);
            return (T obj, String str) => func(obj, UInt32.Parse(str));
        }
		static Action<T, String> GetNullableStringUInt32Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<UInt32>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<UInt32>>), propertyAccessor);
            return (T obj, String str) => func(obj, str == null? (Nullable<UInt32>)null : UInt32.Parse(str));
        }
        static Action<T, Int64> GetNumericUInt32Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, UInt32>)Delegate.CreateDelegate(typeof(Action<T, UInt32>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToUInt32(value));
        }
		static Action<T, Int64> GetNullableNumericUInt32Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<UInt32>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<UInt32>>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToUInt32(value));
        }
		 
		// UINT64
        static Action<T, String> GetStringUInt64Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, UInt64>)Delegate.CreateDelegate(typeof(Action<T, UInt64>), propertyAccessor);
            return (T obj, String str) => func(obj, UInt64.Parse(str));
        }
		static Action<T, String> GetNullableStringUInt64Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<UInt64>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<UInt64>>), propertyAccessor);
            return (T obj, String str) => func(obj, str == null? (Nullable<UInt64>)null : UInt64.Parse(str));
        }
        static Action<T, Int64> GetNumericUInt64Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, UInt64>)Delegate.CreateDelegate(typeof(Action<T, UInt64>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToUInt64(value));
        }
		static Action<T, Int64> GetNullableNumericUInt64Setter<T>(MethodInfo propertyAccessor)
        {
            var func = (Action<T, Nullable<UInt64>>)Delegate.CreateDelegate(typeof(Action<T, Nullable<UInt64>>), propertyAccessor);
            return (T obj, Int64 value) => func(obj, Convert.ToUInt64(value));
        }
			}

	internal sealed class Setter<T>
    {
        internal Action<T, String> StringSetter { get; set; }
        internal Action<T, Int64> NumericSetter { get; set; }
    }
}