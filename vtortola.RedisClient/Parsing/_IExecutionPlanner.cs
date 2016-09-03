using System;

namespace vtortola.Redis
{
    internal interface IExecutionPlanner
    {
        ExecutionPlan Build(String command);
    }
}
