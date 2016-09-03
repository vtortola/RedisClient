
using System;
using System.Collections.Generic;

namespace vtortola.Redis
{
    /// <summary>
    /// Allows to inspect the command result returned by Redis
    /// </summary>
    public interface IRedisResultInspector
    {
        /// <summary>
        /// Gets the type of the Redis response.
        /// </summary>
        RedisType RedisType { get; }

        /// <summary>
        /// Gets a 64bit integer. Throws an exception if the value is not an integer.
        /// </summary>
        Int64 GetInteger();

        /// <summary>
        /// Gets a string. Throws an exception if the value is not a string.
        /// </summary>
        String GetString();

        /// <summary>
        /// Gets a 64bit integer array. Throws an exception if the value is not an array of integers.
        /// </summary>
        Int64[] GetIntegerArray();

        /// <summary>
        /// Gets a string array. Throws an exception if the value is not an array of strings.
        /// </summary>
        String[] GetStringArray();

        /// <summary>
        /// Gets an arbitrary array. Throws an exception if the value is not an array.
        /// </summary>
        Object[] GetArray();

        /// <summary>
        /// Gets the error result if any.
        /// </summary>
        RedisClientCommandException GetException();

        /// <summary>
        /// Gets the result as 64bit integer. If result is not an integer, it tries to parse it as such.
        /// </summary>
        Int64 AsInteger();

        /// <summary>
        /// Gets the result as string. If result is not a string, it returns the string representation.
        /// </summary>
        String AsString();

        /// <summary>
        /// Gets the result a double precision floating point number. If result is not an double, it tries to parse it as such.
        /// </summary>
        Double AsDouble();

        /// <summary>
        /// Gets the result as enum.
        /// </summary>
        TEnum AsEnum<TEnum>() where TEnum : struct, IConvertible, IComparable, IFormattable;

        /// <summary>
        /// Gets the result as string array. If result does not contain strings, it returns an array of the their string representations.
        /// </summary>
        String[] AsStringArray();

        /// <summary>
        /// Gets the result as 64bit integer array. If result is not an integer array, it tries to parse it as such.
        /// </summary>
        Int64[] AsIntegerArray();

        /// <summary>
        /// Returns a dictionary collating a vector of key-value as key value pairs.
        /// Ex: "key1 value1 key2 value2" will become "{key1:value1, key2:value2}"
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <returns></returns>
        IDictionary<TKey, TValue> AsDictionaryCollation<TKey, TValue>();

        /// <summary>
        /// Collates a vector of key-values to the properties of an object of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ignoreMissingMembers">If <c>true</c> does not fail if there is a key without property.</param>
        /// <param name="ignoreTypeMismatchMembers">If <c>true</c> does not fail if cannot bind a value to the property indicated by the key.</param>
        /// <returns></returns>
        T AsObjectCollation<T>(Boolean ignoreMissingMembers = true, Boolean ignoreTypeMismatchMembers = true) where T : new();

        /// <summary>
        /// Collates a vector of key-values to the properties of an object of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The object which properties will be set.</param>
        /// <param name="ignoreMissingMembers">If <c>true</c> does not fail if there is a key without property.</param>
        /// <param name="ignoreTypeMissmatchMembers">If <c>true</c> does not fail if cannot bind a value to the property indicated by the key.</param>
        void AsObjectCollation<T>(T instance, Boolean ignoreMissingMembers = true, Boolean ignoreTypeMissmatchMembers = true);

        /// <summary>
        /// Expand the current command result, into an inspectionalbe <see cref="IRedisResults"/> that contains many <see cref="IRedisResultInspector"/>
        /// </summary>
        /// <returns></returns>
        IRedisResults AsResults();

        /// <summary>
        /// Throws exception if command result is not a string with the value "OK"
        /// </summary>
        void AssertOK();
    }
}
