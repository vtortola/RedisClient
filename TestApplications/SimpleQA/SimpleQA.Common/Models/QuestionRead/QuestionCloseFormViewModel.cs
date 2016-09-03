using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    [MetadataType(typeof(IQuestionId))]
    public class QuestionCloseFormViewModel : IQuestionId, IModel
    {
        public String Id { get; set; }

        public Int64 Votes { get; set; }
    }
}