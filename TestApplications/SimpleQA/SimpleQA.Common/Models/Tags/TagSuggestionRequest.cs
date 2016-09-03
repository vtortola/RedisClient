using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    public class TagSuggestionRequest : IModelRequest<TagSuggestionsModel>
    {
        [Required]
        public String Query { get; set; }
    }
}