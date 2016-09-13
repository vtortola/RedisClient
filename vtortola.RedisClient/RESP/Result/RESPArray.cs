using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace vtortola.Redis
{
    internal sealed class RESPArray : RESPObject, IReadOnlyCollection<RESPObject>
    {
        internal static readonly RESPArray Empty = new RESPArray(0);

        readonly RESPObject[] _items;

        internal override Char Header { get { return RESPHeaders.Array; } }
        internal RESPObject this[Int32 index] { get { return _items[index]; } }


        internal RESPArray(Int32 length)
        {
            _items = new RESPObject[length];
        }

        internal T ElementAt<T>(Int32 index)
            where T:RESPObject
        {
            var item = _items.ElementAt(index);
            T result = item as T;
            if (result == null)
                throw new RedisClientCastException("Element at position " + index + " is not of type " + typeof(T).Name);
            return result;
        }

        internal static RESPArray Load(SocketReader reader)
        {
            Int32 itemCount = reader.ReadInt32();
            
            if (itemCount < 0)
                return RESPArray.Empty;

            var array = new RESPArray(itemCount);
            for (int i = 0; i < itemCount; i++)
            {
                var obj = RESPObject.Read(reader);
                if (obj == null)
                    throw new RESPException("Cannot read array elements.");
                array._items[i]=obj;
            }
            return array;
        }
       
        public Int32 Count
        {
            get { return _items.Length; }
        }

        public IEnumerator<RESPObject> GetEnumerator()
        {
            return _items.OfType<RESPObject>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public override String ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0}{1}\r\n[", this.Header, this.Count.ToString());
            for (var i = 0; i < _items.Length; i++)
            {
                sb.Append(_items[i].ToString());
                if (i != _items.Length - 1)
                    sb.Append(", ");
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
