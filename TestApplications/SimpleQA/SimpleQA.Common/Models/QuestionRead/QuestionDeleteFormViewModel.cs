using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    [MetadataType(typeof(IQuestionId))]
    public class QuestionDeleteFormViewModel : IQuestionId, IModel
    {
        public String Id { get; set; }
    }
}