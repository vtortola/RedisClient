using SimpleQA.Models;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SimpleQA.WebApp
{
    public interface IModelBuilderMediator
    {
        Task<TModel> BuildAsync<TRequest, TModel>(TRequest request, IPrincipal user, CancellationToken cancel)
            where TModel : IModel
            where TRequest : IModelRequest<TModel>;
    }

    public class ModelBuilderMediator : IModelBuilderMediator
    {
        public Task<TModel> BuildAsync<TRequest, TModel>(TRequest request, IPrincipal user, CancellationToken cancel)
            where TRequest : IModelRequest<TModel>
            where TModel : IModel
        {
            var instance = DependencyResolver.Current.GetService<IModelBuilder<TRequest, TModel>>();
            return instance.BuildAsync(request, user, cancel);
        }
    }
}