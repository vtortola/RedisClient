using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    [DebuggerDisplay("{Header} ({Count})")]
    internal sealed class RESPCommand : IEnumerable<RESPCommandPart>
    {
        readonly List<RESPCommandPart> _parts;
        readonly RESPCommandLiteral _header;

        internal Boolean IsSubscription { get; private set; }
        internal RESPCommandPart this[Int32 index] { get { return _parts[index]; } }
        internal Int32 Count { get { return _parts.Count; } }
        internal String Header { get { return _header.Value; } }

        internal RESPCommand(RESPCommandLiteral header, Boolean isSubscriptionCommand, Int32? capacity = null)
        {
            _parts = capacity.HasValue ? new List<RESPCommandPart>(capacity.Value) : new List<RESPCommandPart>();
            _header = header;
            _parts.Add(header);
            IsSubscription = isSubscriptionCommand;
        }
        
        internal void Add(RESPCommandPart commandPart)
        {
            _parts.Add(commandPart);
        }

        internal void AddRange(IEnumerable<RESPCommandPart> items)
        {
            Contract.Assert(items.Any(), "Calling RESPCommand.AddRange() with an empty collection.");

            _parts.AddRange(items);
        }

        internal TRESP Cast<TRESP>(Int32 index)
            where TRESP:RESPCommandPart
        {
            return (TRESP)_parts[index];
        }

        internal void WriteTo(SocketWriter writter)
        {
            const Char CRChar = '\r';
            const Char LFChar = '\n';

            var scount = _parts.Count.ToString();
            var array = new Char[1 + scount.Length + 2];
            
            var index = 0;
            array[index++] = RESPHeaders.Array;

            for (int i = 0; i < scount.Length; i++)
                array[index++] = scount[i];

            array[index++] = CRChar;
            array[index++] = LFChar;

            Contract.Assert(index == array.Length, "Literal construction does not fit.");

            writter.Write(array);

            foreach (var item in _parts)
                item.WriteTo(writter);
        }

        public IEnumerator<RESPCommandPart> GetEnumerator()
        {
            return _parts.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _parts.GetEnumerator();
        }
    }
}
