using System;

namespace SimpleQA.Models
{
    public class UserModelRequest:IModelRequest<UserModel>
    {
        public String User { get; set; }
    }

    public class UserModel : IModel
    {
        public String Name { get; set; }
    }
}