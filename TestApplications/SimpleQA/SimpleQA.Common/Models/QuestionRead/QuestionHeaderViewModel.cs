using System;

namespace SimpleQA.Models
{
    public enum QuestionStatus { Open, Closed, Deleted}

    public class QuestionHeaderViewModel : IModel, IAuthored
    {
        public String Id { get; set; }
        public String Slug { get; set; }
        public String Title { get; set; }
        public String User { get; set; }
        public DateTime CreatedOn { get; set; }
        public String[] Tags { get; set; }
        public Int64 Score { get; set; }
        public Int32 UpVotes { get; set; }
        public Int32 DownVotes { get; set; }
        public Int32 AnswerCount { get; set; }
        public Int32 ViewCount { get; set; }

        static readonly String[] _empty = new String[0];
        public QuestionHeaderViewModel()
        {
            Tags = _empty;
        }
    }
}