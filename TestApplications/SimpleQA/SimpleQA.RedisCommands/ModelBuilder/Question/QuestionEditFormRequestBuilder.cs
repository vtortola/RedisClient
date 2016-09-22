using SimpleQA.Models;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class QuestionEditFormRequestBuilder : IModelBuilder<QuestionEditFormRequest, QuestionEditFormViewModel>
    {
        readonly IRedisChannel _channel;
        public QuestionEditFormRequestBuilder(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<QuestionEditFormViewModel> BuildAsync(QuestionEditFormRequest request, IPrincipal user, CancellationToken cancel)
        {
            var result = await _channel.ExecuteAsync(
                                        "QuestionEditForm {question} @id @userId", 
                                        new
                                        {
                                            id = request.Id,
                                            userId = user.GetSimpleQAIdentity().Id
                                        })
                                        .ConfigureAwait(false);

            CheckException(result);

            result = result[0].AsResults();

            var model = new QuestionEditFormViewModel();
            model.Content = result[0].GetString();
            model.Title = result[1].GetString();
            model.Tags = result[2].GetStringArray();
            model.Id = request.Id;
            return model;
        }

        private static void CheckException(IRedisResults result)
        {
            var error = result[0].GetException();
            if (error != null)
            {
                switch (error.Prefix)
                {
                    case "NOTOWNER":
                        throw new SimpleQAException("You are not the author of the question you try to edit.");
                    default:
                        throw error;
                }
            }
        }
    }
}
