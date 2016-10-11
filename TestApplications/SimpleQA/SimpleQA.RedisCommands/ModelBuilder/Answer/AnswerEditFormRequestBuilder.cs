using SimpleQA.Models;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class AnswerEditFormRequestBuilder : IModelBuilder<AnswerEditFormRequest, AnswerEditFormViewModel>
    {
        readonly IRedisChannel _channel;

        public AnswerEditFormRequestBuilder(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<AnswerEditFormViewModel> BuildAsync(AnswerEditFormRequest request, SimpleQAIdentity user, CancellationToken cancel)
        {
            var result = await _channel.ExecuteAsync(
                                        "GetAnswerEditData {question} @answerId @userID",
                                        new 
                                        { 
                                            answerId = request.AnswerId,
                                            userId = user.Id
                                        })
                                        .ConfigureAwait(false);

            CheckException(result);
            return new AnswerEditFormViewModel()
            {
                AnswerId = request.AnswerId,
                QuestionId = request.QuestionId,
                Content = result[0].GetString()
            };
        }

        static void CheckException(IRedisResults result)
        {
            var error = result[0].GetException();
            if (error != null)
            {
                switch (error.Prefix)
                {
                    case "NOTOWNER":
                        throw new SimpleQAException("You are not the author of the answer you try to edit.");
                    default:
                        throw error;
                }
            }
        }
    }
}
