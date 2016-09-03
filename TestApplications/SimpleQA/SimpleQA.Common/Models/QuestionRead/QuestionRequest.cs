using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    [MetadataType(typeof(IQuestionId))]
    public class QuestionRequest : IModelRequest<QuestionViewModel>, IQuestionId
    {
        public String Id { get; set; }
    }
}