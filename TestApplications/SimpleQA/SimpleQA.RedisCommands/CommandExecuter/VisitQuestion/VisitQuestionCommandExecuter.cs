using SimpleQA.Commands;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public class VisitQuestionCommandExecuter : ICommandExecuter<VisitQuestionCommand, VisitQuestionCommandResult>
    {
        readonly IRedisChannel _channel;

        public VisitQuestionCommandExecuter(IRedisChannel channel)
        {
            _channel = channel;
        }

        public Task<VisitQuestionCommandResult> ExecuteAsync(VisitQuestionCommand command, IPrincipal user, CancellationToken cancel)
        {
            _channel.Dispatch("VisitQuestion {question} @QuestionId @Views", command);
            return Task.FromResult(VisitQuestionCommandResult.Nothing);
        }
    }
}
