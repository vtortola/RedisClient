using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    [MetadataType(typeof(IQuestionId))]
    public class QuestionEditFormRequest : IModelRequest<QuestionEditFormViewModel>, IQuestionId
    {
        public String Id { get; set; }
    }
}