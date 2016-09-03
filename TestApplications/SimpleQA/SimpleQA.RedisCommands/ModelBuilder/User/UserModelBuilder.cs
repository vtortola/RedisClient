using SimpleQA.Models;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class UserModelBuilder : IModelBuilder<UserModelRequest, UserModel>
    {
        IRedisChannel _channel;

        public UserModelBuilder(IRedisChannel channel)
        {
            _channel = channel;
        }

        public Task<UserModel> BuildAsync(UserModelRequest request, IPrincipal user, CancellationToken cancel)
        {
            return Task.FromResult(new UserModel() { Name = request.User });
        }
    }
}
