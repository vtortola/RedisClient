using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    internal static class RESPObjectExtensions
    {
        internal static TRESP Cast<TRESP>(this RESPObject obj)
            where TRESP : RESPObject
        {
            TRESP value = obj as TRESP;
            if (value == null)
            {
                if (obj.Header == RESPHeaders.Error)
                      throw new RedisClientCommandException((RESPError)obj);

                throw new RedisClientCastException("Object '" + obj.GetType().Name + "' cannot be casted to '" + typeof(TRESP).Name + "'");
            }
            return value;
        }

        internal static Int64 GetInt64(this RESPObject obj)
        {
            if (obj.Header == RESPHeaders.Integer)
                return ((RESPInteger)obj).Value;
            else
                throw new RedisClientCastException("Type '" + obj.GetType().Name + "' cannot be cast to 'Int64'");
        }

        internal static String GetString(this RESPObject obj)
        {
            if (RESPString.IsString(obj.Header))
                return obj.ToString();
            else
                throw new RedisClientCastException("Type '" + obj.GetType().Name + "' cannot be cast to 'String'");
        }

        internal static String AsString(this RESPObject obj)
        {
            return obj.ToString();
        }

        internal static Int64 AsInt64(this RESPObject obj)
        {
            if (RESPString.IsString(obj.Header))
            {
                var val = obj.ToString();
                return String.IsNullOrWhiteSpace(val) ? 0 : Int64.Parse(val);
            }
            else if (obj.Header == RESPHeaders.Integer)
                return ((RESPInteger)obj).Value;
            else
                throw new RedisClientCastException("Type '" + obj.GetType().Name + "' cannot be formatted as 'Int64'");
        }

        internal static Double AsDouble(this RESPObject obj)
        {
            if (RESPString.IsString(obj.Header))
            {
                var val = obj.ToString();
                return String.IsNullOrWhiteSpace(val) ? 0 : Double.Parse(val, RESPObject.FormatInfo);
            }
            else if (obj.Header == RESPHeaders.Integer)
                return ((RESPInteger)obj).Value;
            else
                throw new RedisClientCastException("Type '" + obj.GetType().Name + "' cannot be formatted as 'Double'");
        }
    }
}
