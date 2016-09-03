using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SimpleQA.Models
{
    [MetadataType(typeof(IAnswerId))]
    public class AnswerDeleteFormViewModel : IModel, IAnswerId
    {
        public String QuestionId { get; set; }
        public String AnswerId { get; set; }
    }
}