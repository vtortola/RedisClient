using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    [MetadataType(typeof(IQuestionId))]
    public class QuestionDeleteFormRequest : IModelRequest<QuestionDeleteFormViewModel>, IQuestionId
    {
        public String Id { get; set; }
    }
}