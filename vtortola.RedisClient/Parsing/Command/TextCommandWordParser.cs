using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    internal static class TextCommandWordParser
    {
        const Char SpaceChar = ' ';
        const Char CRChar = '\r';
        const Char LFChar = '\n';
        const Char TabChar = '\t';
        const Char SingleQuoteChar = '\'';
        const Char DoubleQuoteChar = '"';
        const Char BackSlashChar = '\\';
        const Char ArrobaChar = '@';

        static Boolean isDelimiter(Char c)
        {
            return c == CRChar || c == LFChar || c == SpaceChar || c == TabChar;
        }

        static Boolean isCommandPartDelimiter(Char c)
        {
            return c == SpaceChar || c == TabChar;
        }

        static Boolean isCommandLineDelimiter(Char c)
        {
            return c == CRChar || c == LFChar;
        }

        // strive for O(n) command parsing
        internal static IEnumerable<TextCommandWord> Parse(String text)
        {
            Contract.Assert(!String.IsNullOrWhiteSpace(text), "Calling to parse empty text.");

            var current = new List<Char>();
            var isParameter = false;
            Char? context = null;
            
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];

                var escaped = DetectEscapedChar(text, ref c, ref i);

                if (!escaped && (c == SingleQuoteChar || c == DoubleQuoteChar))
                {
                    if (context.HasValue && context.Value == c)
                    {
                        context = null;
                        continue;
                    }
                    else if(!context.HasValue)
                    {
                        context = c;
                        continue;
                    }
                }

                if (context.HasValue)
                {
                    current.Add(c);
                    continue;
                }

                if (c == ArrobaChar && !current.Any() && (text[i - 1] == ArrobaChar || isDelimiter(text[i - 1])))
                {
                    isParameter = true;
                    continue;
                }

                if(isDelimiter(c))
                {
                    // move till new word, end of line or break is found
                    // to ensure that in 'hello  \t\nworld', 'hello' is
                    // considered end of a line as well.
                    var nextBreak = c;
                    if (isCommandPartDelimiter(c))
                    {
                        for (var j = i + 1; j <= text.Length; j++)
                        {
                            if (j == text.Length)
                            {
                                nextBreak = LFChar;
                                break;
                            }
                            nextBreak = text[j];
                            DetectEscapedChar(text, ref nextBreak, ref j);
                            if (!isCommandPartDelimiter(nextBreak))
                            {
                                break;
                            }
                            i++;
                        }
                    }

                    if (!current.Any())
                        continue;

                    yield return new TextCommandWord(new String(current.ToArray()), isParameter, isCommandLineDelimiter(nextBreak));

                    current.Clear();
                    isParameter = false;
                    continue;
                }

                current.Add(c);
            }

            if (current.Any())
                yield return new TextCommandWord(new String(current.ToArray()), isParameter, true);
        }

        static Boolean DetectEscapedChar(String text, ref Char c, ref Int32 i)
        {
            if (c != BackSlashChar)
                return false;

            if (i == text.Length - 1)
                return false;

            var next = text[i + 1];

            switch (next)
            {
                case 't':
                    c = TabChar;
                    i++;
                    break;

                case 'r':
                    c = CRChar;
                    i++;
                    break;

                case 'n':
                    c = LFChar;
                    i++;
                    break;

                case DoubleQuoteChar:
                    c = '"';
                    i++;
                    break;

                case SingleQuoteChar:
                    c = '\'';
                    i++;
                    break;

                case ArrobaChar:
                    c = ArrobaChar;
                    i++;
                    break;
            }
            return true;
        }
    }
}
