using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    internal static class ObjectBinder<T>
    {
        static Dictionary<String, Setter<T>> _setters;

        internal static void Bind(RESPArray array, T instance, Boolean ignoreMissingMembers = true, Boolean ignoreTypeMismatchMembers = true)
        {
            var type = typeof(T);

            if(_setters == null)
            {
                var setter = Create();
                Interlocked.CompareExchange(ref _setters, setter, null);
            }

            for (int i = 0; i < array.Count; i += 2)
            {
                var member = array[i];
                var memberName = GetPropertyName(member, i);

                Setter<T> setter;
                if(!_setters.TryGetValue(memberName, out setter))
                {
                    if (!ignoreMissingMembers)
                        throw new RedisClientBindingException("The result contains property '" + memberName + "' but the object to bind to does not have such member");
                }
                else
                {
                    if(setter == null)
                    {
                        if(!ignoreTypeMismatchMembers)
                            throw new RedisClientBindingException("The target object contains a member named '" + memberName + "' but the type is not compatible with the one in the result (" + member.GetType().ToString() + ")");
                    }
                    else
                    {
                        SetValue(memberName, instance, i < array.Count ? array[i + 1] : null, setter);
                    }
                }
            }
        }

        static void SetValue(String memberName, T instance, RESPObject obj, Setter<T> setter)
        {
            try
            {
                switch (obj.Header)
                {
                    case RESPHeaders.Integer:
                        setter.NumericSetter(instance, obj.GetInt64());
                        break;
                    case RESPHeaders.BulkString:
                    case RESPHeaders.SimpleString:
                        setter.StringSetter(instance, obj.GetString());
                        break;
                    case RESPHeaders.Error:
                        throw new RedisClientCommandException((RESPError)obj);
                    default:
                        throw new RedisClientBindingException("The result includes a member of type '" + obj.GetType().Name + "', but that type cannot be bound to an object member.");
                }
            }
            catch (Exception ex)
            {
                throw new RedisClientBindingException(String.Format("Cannot set member '{0}'", memberName), ex);
            }
        }

        static Dictionary<String, Setter<T>> Create()
        {
            var type = typeof(T);
            var properties = type.GetProperties();
            var setters = new Dictionary<String, Setter<T>>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in properties)
            {
                if (SetterHelper.IsSupported(property.PropertyType))
                    setters.Add(property.Name, SetterHelper.CreateSetter<T>(property));
                else
                    setters.Add(property.Name, null);
            }
            return setters;
        }

        static String GetPropertyName(RESPObject obj, Int32 index)
        {
            if (!RESPString.IsString(obj.Header))
                throw new RedisClientBindingException("Element at index " + index + " must be an string in order to be a member name");
            return obj.GetString();
        }
    }
}
