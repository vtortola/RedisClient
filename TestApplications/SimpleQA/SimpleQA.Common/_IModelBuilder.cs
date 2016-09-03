using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleQA.Models
{
    public interface IModel
    {
    }

    public interface IModelRequest<TResult>
        where TResult : IModel
    {
    }

    public interface IModelBuilder<TRequest,TModel>
        where TRequest : IModelRequest<TModel>
        where TModel : IModel
    {
        Task<TModel> BuildAsync(TRequest request, IPrincipal user, CancellationToken cancel);
    }
}