using System;
using System.Collections.Generic;

namespace SimpleQA.Models
{
    public class QuestionViewModel : QuestionHeaderViewModel, IVotable
    {
        public String HtmlContent { get; set; }
        public Boolean? UpVoted { get; set; }
        public Boolean AuthoredByUser { get; set; }
        public Int32 CloseVotes { get; set; }
        public Boolean UserVotedClose { get; set; }
        public QuestionStatus Status { get; set; }
        public List<AnswerViewModel> Answers { get; set; }
        public Boolean Votable { get { return this.Status == QuestionStatus.Open; } }
        public QuestionViewModel()
        {
            Answers = new List<AnswerViewModel>();
        }


        
    }
}