using SimpleQA.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleQA.Models
{
    public sealed class QuestionNotification
    {
        public String Title { get; set; }
        public String Id { get; set; }
        public String Slug { get; set; }
    }

    public sealed class UserInboxModel : IModel
    {
        public QuestionNotification[] Questions { get; private set; }
        public UserInboxModel(IEnumerable<QuestionNotification> questions)
        {
            Questions = questions.ToArray() ;
        }
    }
}
