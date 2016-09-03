using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace vtortola.Redis
{
    // TODO: This implementation is very patchy and
    // will probably contain bugs. Multiline parameter not supported.
    // Consider a better one that use tokens but do not need to parse
    // the whole Lua script into such tokens.

    internal static class ProcedureParser
    {
        const Char LF = '\n';
        const Char CR = '\r';
        const Char Space = ' ';
        const Char Tab = '\t';
        const Char OpenParenthesis = '(';
        const Char CloseParenthesis = ')';
        const Char OpenArray = '[';
        const Char CloseArray = ']';
        const Char KeyDecorator = '$';
        const Char Comma = ',';

        static readonly Char[] _split = new[] { ',' };

        internal static IEnumerable<ProcedureDefinition> Parse(TextReader reader)
        {
            String line = String.Empty;
            ProcedureDefinition current = null;
            StringBuilder current_body = new StringBuilder(1024);
            Boolean parsingParameters = false;
            List<ProcedureParameter> parameters = new List<ProcedureParameter>();
            while(line != null)
            {
                line = reader.ReadLine();
                if (line == null)
                    continue;

                var index = 0;
                if(IsProcStart(line, ref index))
                {
                    if (current != null)
                        throw new RedisClientProcedureParsingException("Procedure start found in the context of other procedure.");

                    current = new ProcedureDefinition();
                    current.Name = GetName(line, ref index);
                    GetParameters(line, ref index, ref parsingParameters, parameters);
                }
                else if(parsingParameters)
                {
                    GetParameters(line, ref index, ref parsingParameters, parameters);
                }
                else if (IsProcEnd(line))
                {
                    if (current == null)
                        throw new RedisClientProcedureParsingException("Procedure end found without beginning.");

                    current.Parameters = parameters.ToArray();
                    AppendParameterInitialization(current_body, current.Parameters);
                    current.Body = current_body.ToString();
                    current.Digest = Hash(current.Body);

                    yield return current;
                    current = null;
                    current_body.Clear();
                    parameters.Clear();
                }
                else if(current != null)
                {
                    current_body.AppendLine(line);
                }
            }

            if(current != null)
                throw new RedisClientProcedureParsingException("Procedure end missing.");
        }

        static void AppendParameterInitialization(StringBuilder body, ProcedureParameter[] procedureParameters)
        {
            var template = new RedisLuaParameterBinding(procedureParameters);
            body.Insert(0, template.TransformText());
            body.AppendLine();
        }

        static void GetParameters(String line, ref Int32 index, ref Boolean parsingParameter, IList<ProcedureParameter> parameters)
        {
            while (IsSpaceChar(line[index]))
                index++;

            var start = index;
            var buffer = new Char[line.Length - index];
            var bufferIndex = 0;
            var processed = new HashSet<String>(StringComparer.OrdinalIgnoreCase);
            var closingFound = false;
            var expectedNext = false;

            for (; index < line.Length; index++)
            {
                var c = line[index];
                if (c == CloseParenthesis || c == Comma)
                {
                    closingFound = c == CloseParenthesis;
                    expectedNext |= c == Comma;

                    if (bufferIndex == 0 && !expectedNext)
                        continue; // no parameters

                    var parameter = CreateParameter(buffer, bufferIndex);
                    if (!processed.Add(parameter.Name))
                        throw new RedisClientProcedureParsingException("Duplicated parameter names.");

                    parameters.Add(parameter);
                    bufferIndex = 0;
                }
                else if(bufferIndex > 0 || !IsSpaceChar(c))
                {
                    buffer[bufferIndex++] = c;
                    expectedNext = false;
                }
            }

            if (!closingFound && !expectedNext)
                throw new RedisClientProcedureParsingException("Cannot parse procedure parameters.");

            parsingParameter = !closingFound;
        }

        static Boolean IsEmptyOrSpace(Char[] buffer, Int32 count)
        {
            if (count == 0)
                return true;

            for (int i = 0; i < count; i++)
                if (!IsSpaceChar(buffer[i]))
                    return false;

            return true;
        }

        static ProcedureParameter CreateParameter(Char[] buffer, Int32 count)
        {
            if (IsEmptyOrSpace(buffer, count))
                throw new RedisClientProcedureParsingException("Empty parameter name is not allowed.");

            var current = 0;
            var start = 0;
            var parameter = new ProcedureParameter();
            if(buffer[current] == KeyDecorator)
            {
                current++; start++;
                parameter.IsKey = true;
            }

            if (!Char.IsLetter(buffer[current]))
                throw new RedisClientProcedureParsingException("Parameter names must start by a letter.");

            for (current++; current < count; current++)
            {
                var c = buffer[current];
                if (!Char.IsLetterOrDigit(c))
                {
                    if (c == OpenArray)
                    {
                        if (current < count - 1 && buffer[current + 1] == CloseArray)
                        {
                            parameter.IsArray = true;
                            break;
                        }
                        else
                            throw new RedisClientProcedureParsingException("Unclosed array indicator in parameter.");
                    }
                    else
                        throw new RedisClientProcedureParsingException("Invalid character in parameter name '" + c + "'");
                }
            }

            parameter.Name = new String(buffer, start, current - start);
            return parameter;
        }

        static String GetName(String line, ref Int32 index)
        {
            while (index < line.Length && IsSpaceChar(line[index]))
                index++;

            if(!Char.IsLetter(line, index))
                throw new RedisClientProcedureParsingException("Procedure name must start by a letter.");

            var spaceCharFound = false;
            var start = index;
            for (; index < line.Length; index++)
            {
                var c = line[index];
                if (c == OpenParenthesis)
                    return line.Substring(start, index++ - start).Trim();
                else if (spaceCharFound)
                    throw new RedisClientProcedureParsingException("Procedure name cannot contain spaces.");
                else
                    spaceCharFound |= IsSpaceChar(c);
            }

            throw new RedisClientProcedureParsingException("Cannot parse procedure name.");
        }

        static Boolean IsProcStart(String line, ref Int32 index)
        {
            while (index < line.Length && IsSpaceChar(line[index]))
                index++;

            if (line.Length - index < 5)
                return false;

            if (line[index] == 'p' && line[index + 1] == 'r' && line[index + 2] == 'o' && line[index + 3] == 'c' && line[index + 4] == Space)
            {
                index += 5;
                return true;
            }
            return false;
        }

        static Boolean IsProcEnd(String line)
        {
            var i = 0;
            while (i < line.Length && IsSpaceChar(line[i]))
                i++;

            if (line.Length - i < 7)
                return false;

            if (line[i] == 'e' && line[i + 1] == 'n' && line[i + 2] == 'd' && line[i + 3] == 'p' && line[i + 4] == 'r' && line[i + 5] == 'o' && line[i + 6] == 'c' && ( i+7==line.Length || IsSpaceChar(line[i+7])))
                return true;

            return false;
        }

        static Boolean IsSpaceChar(Char c)
        {
            return c == Space || c == Tab || c == CR || c == LF;
        }

        static string Hash(String input)
        {
            Contract.Assert(!String.IsNullOrWhiteSpace(input), "Cannot sha1 hash an empty string.");

            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
