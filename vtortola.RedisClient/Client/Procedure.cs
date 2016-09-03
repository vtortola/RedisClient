using System;

namespace vtortola.Redis
{
    internal static class Procedure
    {
        internal static void AmmendScriptErrors(RESPObject[] responses, String[] headers, ProcedureCollection procedures)
        {
            foreach (var item in responses)
            {
                if (item == null || item.Header != RESPHeaders.Error)
                    continue;

                var error = (RESPError)item;
                if (error.Prefix != null && error.Message != null && error.Prefix.Equals("ERR", StringComparison.Ordinal))
                {
                    FormatScriptError(procedures, error);
                }
            }
        }

        private static void FormatScriptError(ProcedureCollection procedures, RESPError error)
        {
            var pos = error.Message.IndexOf("Error running script (call to ");
            if (pos != -1 && error.Message.Length >= 72)
            {
                var scriptId = error.Message.Substring(30, 42);
                var sha1 = scriptId.Substring(2);
                ProcedureDefinition script;
                if (procedures.TryGetByDigest(sha1, out script))
                {
                    error.SetMessage(error.Message.Replace(scriptId, String.Format("{0} [sha1: {1}]", script.Name, sha1)));
                }
            }
        }
    }
}
