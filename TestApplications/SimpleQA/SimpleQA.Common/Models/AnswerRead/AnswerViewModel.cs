using System;

namespace SimpleQA.Models
{
    public class AnswerViewModel : IVotable, IAuthored, IModel
    {
        public String Id { get; set; }
        public String QuestionId { get; set; }
        public String HtmlContent { get; set; }
        public String User { get; set; }
        public DateTime CreatedOn { get; set; }
        public Int64 Score { get; set; }
        public Int32 UpVotes { get; set; }
        public Int32 DownVotes { get; set; }
        public Boolean? UpVoted { get; set; }
        public Boolean AuthoredByUser { get; set; }
        public Boolean Editable { get; set; }
        public Boolean Votable { get; set; }
    }
}