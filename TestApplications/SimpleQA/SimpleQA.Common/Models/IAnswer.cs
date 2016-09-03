using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    public interface IAnswerId
    {
        [Required]
        String AnswerId { get; set; }

        [Required]
        String QuestionId { get; set; }
    }

    public interface IAnswerData
    {
        [Required]
        String QuestionId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        String Content { get; set; }
    }

    // fields are repeated so MetadataType can be used, 
    // otherwise validations are not infered
    public interface IAnswer : IAnswerId, IAnswerData
    {
        [Required]
        String AnswerId { get; set; }

        [Required]
        String QuestionId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        String Content { get; set; }
    }
}