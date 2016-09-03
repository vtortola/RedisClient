using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    [MetadataType(typeof(IAnswerData))]
    public class AnswerCreateRequest : IAnswerData
    {
        public String Content { get; set; }

        public String QuestionId { get; set; }
    }
}