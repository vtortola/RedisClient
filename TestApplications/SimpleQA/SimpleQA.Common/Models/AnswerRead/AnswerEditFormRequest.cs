using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    [MetadataType(typeof(IAnswerId))]
    public class AnswerEditFormRequest : IAnswerId, IModelRequest<AnswerEditFormViewModel>
    {
        public String QuestionId { get; set; }

        public String AnswerId { get; set; }
    }
}