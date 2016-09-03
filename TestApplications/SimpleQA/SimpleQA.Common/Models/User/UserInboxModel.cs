using SimpleQA.Models;
using System;

namespace SimpleQA.Models
{
    public sealed class QuestionNotification
    {
        public String Title { get; private set; }
        public String Id { get; private set; }
        public String Slug { get; private set; }
        public QuestionNotification(String id, String slug, String title)
        {
            Id = id;
            Slug = slug;
            Title = title;
        }
    }

    public sealed class UserInboxModel : IModel
    {
        public QuestionNotification[] Questions { get; private set; }
        public UserInboxModel(QuestionNotification[] questions)
        {
            Questions = questions;
        }
    }
}
