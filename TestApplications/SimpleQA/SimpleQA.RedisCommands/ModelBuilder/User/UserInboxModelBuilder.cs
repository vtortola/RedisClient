using SimpleQA.Models;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class UserInboxModelBuilder : IModelBuilder<UserInboxRequest, UserInboxModel>
    {
        IRedisChannel _channel;

        public UserInboxModelBuilder(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<UserInboxModel> BuildAsync(UserInboxRequest request, IPrincipal user, CancellationToken cancel)
        {
            var result = await _channel.ExecuteAsync(@"
                                        MULTI
                                        SMEMBERS @inbox
                                        DEL @inbox
                                        EXEC",
                                        new { inbox = Keys.UserInboxKey(user.Identity.Name) })
                                        .ConfigureAwait(false);

            var list = new List<QuestionNotification>();
            foreach (var questionId in result[1].GetStringArray())
            {
                var questionResult = await _channel.ExecuteAsync("HMGET @questionId Id Slug Title", new { questionId }).ConfigureAwait(false);
                var data = questionResult[0].GetStringArray();
                list.Add(new QuestionNotification(data[0], data[1], data[2]));
            }

            return new UserInboxModel(list.ToArray());
        }
    }
}
