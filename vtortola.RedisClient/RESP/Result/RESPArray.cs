using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace vtortola.Redis
{
    internal sealed class RESPArray : RESPObject, IReadOnlyCollection<RESPObject>
    {
        internal static readonly RESPArray Null = new RESPArray() { IsNullArray=true };

        readonly List<RESPObject> _items;

        internal Boolean IsNullArray { get; private set; }
        internal override char Header { get { return RESPHeaders.Array; } }
        internal RESPObject this[Int32 index] { get { return _items[index]; } }


        internal RESPArray()
        {
            _items = new List<RESPObject>();
        }

        internal RESPArray(params RESPObject[] objects)
        {
            _items = new List<RESPObject>(objects);
        }

        internal RESPArray(params String[] literals)
        {
            _items = new List<RESPObject>(literals.Select(l => new RESPBulkString(l)));
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
                return Null;

            var array = new RESPArray();
            for (int i = 0; i < itemCount; i++)
            {
                var obj = RESPObject.Read(reader);
                if (obj == null)
                    throw new RESPException("Cannot read aray elements.");
                array._items.Add(obj);
            }
            return array;
        }
       
        public int Count
        {
            get { return _items.Count; }
        }

        public IEnumerator<RESPObject> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public override String ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0}{1}\r\n", this.Header, this.Count.ToString());
            foreach (var item in _items)
            {
                sb.AppendLine(item.ToString());
            }
            return sb.ToString();
        }
    }
}
