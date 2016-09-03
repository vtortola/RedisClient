using System;

namespace vtortola.Redis
{
    internal sealed class ExecutionContextOperation
    {
        internal Boolean OpensTransaction { get; private set; }
        internal Boolean ClosesTransaction { get; private set; }
        internal Boolean OpensWatch { get; private set; }
        internal Boolean ClosesWatch { get; private set; }
        internal Boolean HasBlockingCommand { get; private set; }
        internal Boolean RequiresStandaloneConnection { get { return OpensTransaction || OpensWatch || HasBlockingCommand; } }

        internal readonly static ExecutionContextOperation Default = new ExecutionContextOperation();

        private ExecutionContextOperation() { }

        internal static ExecutionContextOperation Parse(String[] headers)
        {
            var op = new ExecutionContextOperation();
            foreach (var header in headers)
            {
                switch (header)
                {
                    case "WATCH":
                        op.OpensWatch = true;
                        break;

                    case "UNWATCH":
                        op.ClosesWatch = true;
                        op.OpensWatch = false;
                        break;

                    case "MULTI":
                        op.OpensTransaction = true;
                        break;

                    case "EXEC":
                    case "DISCARD":
                        op.ClosesTransaction = true;
                        op.ClosesWatch = true;
                        op.OpensTransaction = false;
                        op.OpensWatch = false;
                        break;

                    case "BLPOP":
                    case "BRPOP":
                    case "BRPOPLPUSH":
                        if (!op.OpensTransaction) // blocking commands inside transactions
                            op.HasBlockingCommand = true; // behaves as non blocking
                        break;
                }
            }
            return op;
        }
    }
}
