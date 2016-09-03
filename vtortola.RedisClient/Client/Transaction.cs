using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    internal static class Transaction
    {
        internal static void Consolidate(RESPObject[] results, String[] commandHeaders)
        {
            Contract.Assert(results.Any(), "Calling ConsolidateTransaction with an empty collection.");
            Contract.Assert(commandHeaders.Any(), "There is not command headers");
            Contract.Assert(results.Length == commandHeaders.Length, "The length of the results must be the same than the command headers.");

            for (int i = results.Length - 1; i >= 0; i--)
            {
                if (commandHeaders[i].Equals("EXEC", StringComparison.Ordinal))
                {
                    i = ProcessResults(results, i);
                }
                else if (commandHeaders[i].Equals("DISCARD", StringComparison.Ordinal))
                {
                    SetQueuedAsDiscarded(results, commandHeaders, ref i);
                }
            }
        }

        private static int ProcessResults(RESPObject[] results, int i)
        {
            var result = results[i];
            if (result.Header == RESPHeaders.Error)
            {
                var errors = GetAllErrors(results);
                throw new RedisClientMultipleCommandException((RESPError)result, errors.ToArray());
            }
            else
            {
                ZipQueuedAndResults(results, ref i);
            }
            return i;
        }

        static List<RedisClientCommandException> GetAllErrors(RESPObject[] results)
        {
            var errors = new List<RedisClientCommandException>();
            for (int j = 0; j < results.Length - 1; j++)
            {
                var eresult = results[j];
                if (eresult.Header != RESPHeaders.Error)
                    continue;

                errors.Add(new RedisClientCommandException((RESPError)eresult, j + 1));
            }
            return errors;
        }

        static void SetQueuedAsDiscarded(RESPObject[] results, String[] commandHeaders, ref Int32 i)
        {
            while (i >= 0 && !commandHeaders[i].Equals("MULTI", StringComparison.Ordinal))
            {
                var element = results[i];
                if (RESPString.IsString(element.Header) && RESPString.Same(element, "QUEUED"))
                    results[i] = RESPSimpleString.DISCARDED;
                i--;
            }
        }

        static void ZipQueuedAndResults(RESPObject[] results, ref Int32 index)
        {
            Contract.Assert(results.Any(), "Passing empty results to zip.");

            var result = results[index];

            if (result.Header == RESPHeaders.BulkString && RESPString.Same(result, null))
            {
                // EXEC failed due WATCH
                results[index] = RESPError.EXECWATCHFAILED;
            }
            else
            {
                index = ZipUpResults(results, index, result);
            }
        }

        static int ZipUpResults(RESPObject[] results, Int32 index, RESPObject result)
        {
            var array = result.Cast<RESPArray>();
            results[index] = RESPSimpleString.OK;
            index--;
            var arrayIndex = array.Count - 1;

            while (index >= 0 && arrayIndex >= 0)
            {
                var element = results[index];
                if (RESPString.IsString(element.Header) && RESPString.Same(element, "QUEUED"))
                {
                    results[index] = array[arrayIndex];
                    arrayIndex--;
                }

                index--;
            }
            return index;
        }
    }
}
