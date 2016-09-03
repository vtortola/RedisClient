using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace vtortola.Redis
{
    
    internal static class ParameterGuard
    {
        [Conditional("DEBUG")]
        private static void ParameterNameCannotBeNullOrEmpty(String guardName, String parameterName)
        {
            if(String.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException("When calling '" + guardName + "' the parameter name cannot be null or empty.");
            }
        }
        internal static void CannotBeNull(Object parameter, String parameterName)
        {
            ParameterNameCannotBeNullOrEmpty("CannotBeNull", parameterName);
            if (parameter == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        internal static void CannotBeNullOrEmpty(String parameter, String parameterName)
        {
            ParameterNameCannotBeNullOrEmpty("CannotBeNullOrEmpty", parameterName);
            if(String.IsNullOrWhiteSpace(parameter))
            {
                throw new ArgumentException("Parameter '" + parameterName + "' cannot be null or empty.");
            }
        }

        internal static void CannotBeNullOrEmpty<T>(IEnumerable<T> parameter, String parameterName)
        {
            ParameterNameCannotBeNullOrEmpty("CannotBeNullOrEmpty", parameterName);
            if (parameter == null || !parameter.Any())
            {
                throw new ArgumentException("Parameter '" + parameterName + "' cannot be null or empty.");
            }
        }

        internal static void CannotBeZeroOrNegative(Int64 parameter, string parameterName)
        {
            ParameterNameCannotBeNullOrEmpty("CannotBeZeroOrNegative", parameterName);
            if (parameter <= 0)
            {
                throw new ArgumentException("Parameter '" + parameterName + "' cannot be zero or less.");
            }
        }

        internal static void CannotBeNegative(Int64 parameter, string parameterName)
        {
            ParameterNameCannotBeNullOrEmpty("CannotBeNegative", parameterName);
            if (parameter < 0)
            {
                throw new ArgumentException("Parameter '" + parameterName + "' cannot be negative.");
            }
        }

        internal static void MustBeBetween(Int64 parameter, String parameterName, Int32 fromInclusive, Int32 toInclusive)
        {
            ParameterNameCannotBeNullOrEmpty("MustBeBetween", parameterName);
            if(parameter > toInclusive || parameter < fromInclusive)
            {
                throw new ArgumentOutOfRangeException("Parameter '" + parameterName + "' must be between " + fromInclusive + " and " + toInclusive + " but it is " + parameter);
            }
        }

        internal static void MustBeBiggerOrEqualThan<T>(T parameter, String parameterName, T comparand)
            where T:IComparable<T>
        {
            ParameterNameCannotBeNullOrEmpty("MustBeBiggerThan", parameterName);

            if (parameter.CompareTo(comparand) < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        internal static void IndexMustFitIn<T>(Int64 parameter, String parameterName, T[] array)
        {
            ParameterNameCannotBeNullOrEmpty("IndexMustFitIn", parameterName);
            CannotBeNegative(parameter, parameterName);

            if(array.Length == 0)
            {
                throw new IndexOutOfRangeException("Parameter '"+parameterName+"' tries to access an empty array.");
            }

            if (parameter > array.Length - 1)
            {
                throw new IndexOutOfRangeException("Parameter '" + parameterName + "' tries to access out of array limits.");
            }
        }
    }
}
