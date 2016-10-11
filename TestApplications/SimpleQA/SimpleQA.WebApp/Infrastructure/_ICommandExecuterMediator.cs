using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SimpleQA.WebApp
{
    public interface ICommandExecuterMediator
    {
        Task<TResult> ExecuteAsync<TCommand, TResult>(TCommand query, SimpleQAIdentity user, CancellationToken cancel)
            where TCommand : ICommand<TResult>;
    }
    
    public sealed class CommandExecuterMediator : ICommandExecuterMediator
    {
        public Task<TResult> ExecuteAsync<TCommand, TResult>(TCommand query, SimpleQAIdentity user, CancellationToken cancel) 
            where TCommand : ICommand<TResult>
        {
            var instance = DependencyResolver.Current.GetService<ICommandExecuter<TCommand, TResult>>();
            return instance.ExecuteAsync(query, user, cancel);
        }
    }
}