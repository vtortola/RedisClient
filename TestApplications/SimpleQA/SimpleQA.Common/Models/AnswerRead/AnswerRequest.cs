using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQA.Models
{
    [MetadataType(typeof(IAnswerId))]
    public class AnswerRequest : IAnswerId, IModelRequest<AnswerViewModel>
    {
        public String QuestionId { get; set; }

        public String AnswerId { get; set; }
    }
}
