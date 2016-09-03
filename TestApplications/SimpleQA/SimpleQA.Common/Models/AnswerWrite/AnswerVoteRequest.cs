using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    [MetadataType(typeof(IAnswerId))]
    public class AnswerVoteRequest : IAnswerId
    {
        public String AnswerId { get; set; }

        public String QuestionId { get; set; }

        [Required]
        public Boolean Upvote { get; set; }
    }
}