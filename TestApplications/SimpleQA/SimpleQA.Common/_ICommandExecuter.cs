using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleQA
{
    public interface ICommand<TResult>
    {

    }

    public interface ICommandExecuter<TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        Task<TResult> ExecuteAsync(TCommand command, SimpleQAIdentity user, CancellationToken cancel);
    }
}
