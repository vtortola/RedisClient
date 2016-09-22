using SimpleQA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var result = await _channel.ExecuteAsync(
                                        "GetInboxNotifications {user} @userId",
                                        new { userId = user.GetSimpleQAIdentity().Id })
                                        .ConfigureAwait(false);

            result.ThrowErrorIfAny();
            var ids = result[0].GetStringArray();
            List<QuestionNotification> list;
            if (ids.Any())
                list = await GetQuestions(ids).ConfigureAwait(false);
            else
                list = new List<QuestionNotification>();
            return new UserInboxModel(list);
        }

        async Task<List<QuestionNotification>> GetQuestions(IEnumerable<String> ids)
        {
            var result =
                await _channel.ExecuteAsync( // Reuse proc from HomeProcedures.rcproc
                               "GetQuestions {question} @ids",
                               new { ids })
                               .ConfigureAwait(false);

            result.ThrowErrorIfAny();

            return result[0]
                        .AsResults()
                        .Select(r => r.AsResults())
                        .Select(q => q[0].AsObjectCollation<QuestionNotification>())
                        .ToList();
        }
    }
}
