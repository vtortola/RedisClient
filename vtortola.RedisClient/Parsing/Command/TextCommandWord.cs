using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace vtortola.Redis
{
    [DebuggerDisplay("{Value}")]
    internal sealed class TextCommandWord
    {
        internal Boolean IsParameter { get; private set; }
        internal String Value { get; private set; }
        internal Boolean IsEndOfLine { get; private set; }

        internal TextCommandWord(String value, Boolean isParameter,Boolean isEndOfLine)
        {
            Contract.Assert(!String.IsNullOrWhiteSpace(value), "Text command needs a value.");

            if (IsParameter)
                throw new RedisClientParsingException("A parameter cannot be parameter.");

            Value = value;
            IsParameter = isParameter;
            IsEndOfLine = isEndOfLine;
        }
    }
}
