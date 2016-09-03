using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    public class QuestionVoteRequest : QuestionActionRequest
    {
        [Required]
        public Boolean Upvote { get; set; }
    }
}