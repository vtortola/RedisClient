using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    [MetadataType(typeof(IQuestionId))]
    public class QuestionActionRequest : IQuestionId
    {
        [Required]
        public String Id { get; set; }
    }
}