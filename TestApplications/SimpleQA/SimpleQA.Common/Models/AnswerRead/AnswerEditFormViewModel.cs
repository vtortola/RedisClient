using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    [MetadataType(typeof(IAnswer))]
    public class AnswerEditFormViewModel : IModel, IAnswer
    {
        public String QuestionId { get; set; }
        public String AnswerId { get; set; }
        public String Content { get; set; }
    }
}