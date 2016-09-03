using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    [MetadataType(typeof(IQuestionId))]
    public class QuestionCloseFormRequest : IModelRequest<QuestionCloseFormViewModel>, IQuestionId
    {
        public String Id { get; set; }
    }
}