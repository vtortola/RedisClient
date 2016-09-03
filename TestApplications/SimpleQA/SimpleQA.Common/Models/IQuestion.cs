using SimpleQA.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    public interface IQuestionId
    {
        [Required]
        String Id { get; set; }
    }

    public interface IQuestionData
    {
        [Required]
        [StringLength(1000, MinimumLength = 10)]
        String Title { get; set; }

        [Required]
        [StringLength(10000, MinimumLength = 50)]
        String Content { get; set; }

        [TagsValidation(1, 5)]
        String[] Tags { get; set; }
    }

    // fields are repeated so MetadataType can be used, 
    // otherwise validations are not infered
    public interface IQuestion : IQuestionId, IQuestionData
    {
        [Required]
        String Id { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        String Title { get; set; }

        [Required]
        [StringLength(10000, MinimumLength = 50)]
        String Content { get; set; }

        [TagsValidation(1, 5)]
        String[] Tags { get; set; }
    }
}