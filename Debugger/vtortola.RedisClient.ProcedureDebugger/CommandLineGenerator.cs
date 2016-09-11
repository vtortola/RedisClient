using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vtortola.Redis;

namespace vtortola.RedisClient.ProcedureDebugger
{
    internal static class CommandLineGenerator
    {
        internal static DebuggingFileSession Generate(params String[] args)
        {
            // Parse input
            var model = InputModel.Parse(args);

            // Parse procedures
            ProcedureDefinition[] procedures;
            try
            {
                using (var sr = new StreamReader(model.FileName))
                    procedures = ProcedureParser.Parse(sr).ToArray();
            }
            catch (RedisClientException rcex)
            {
                throw new SyntaxException(String.Format("The file \"{0}\" is not a valid vtortola.RedisClient procedures files or there is a syntax problem in it.", model.FileName), rcex);
            }

            var procedure = procedures.SingleOrDefault(p => p.Name.Equals(model.Procedure, StringComparison.InvariantCultureIgnoreCase));
            if (procedure == null)
                throw new SyntaxException(String.Format("Cannot find the specified procedure '{0}' in '{1}'", model.Procedure, model.FileName));

            // generate temporary LUA file with the procedure content
            return new DebuggingFileSession(model, procedure);
        }
    }
}
