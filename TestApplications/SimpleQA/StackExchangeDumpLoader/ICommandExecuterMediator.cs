using SimpleInjector;
using SimpleQA;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace StackExchangeDumpLoader
{
    public interface ICommandExecuterMediator
    {
        Task<TResult> ExecuteAsync<TCommand, TResult>(TCommand query, SimpleQAIdentity user, CancellationToken cancel)
            where TCommand : ICommand<TResult>;
    }
    
    public sealed class CommandExecuterMediator : ICommandExecuterMediator
    {
        readonly Container _container;
        public CommandExecuterMediator(Container container)
        {
            _container = container;
        }

        public Task<TResult> ExecuteAsync<TCommand, TResult>(TCommand query, SimpleQAIdentity user, CancellationToken cancel) 
            where TCommand : ICommand<TResult>
        {
            var instance = _container.GetInstance<ICommandExecuter<TCommand, TResult>>();
            return instance.ExecuteAsync(query, user, cancel);
        }
    }
}