using SimpleQA.Models;

namespace SimpleQA.Models
{
    public sealed class UserInboxRequest : IModelRequest<UserInboxModel>
    {
        private UserInboxRequest()
        {
                
        }

        public static readonly UserInboxRequest Empty = new UserInboxRequest();
    }
}
