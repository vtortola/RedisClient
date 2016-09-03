using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    [MetadataType(typeof(IAnswerId))]
    public class AnswerDeleteFormRequest : IAnswerId, IModelRequest<AnswerDeleteFormViewModel>
    {
        public String QuestionId { get; set; }

        public String AnswerId { get; set; }
    }
}