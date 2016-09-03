using System;

namespace SimpleQA.Models
{
    public class TagSuggestionsModel : IModel
    {
        public String[] Suggestions { get; private set; }
        public TagSuggestionsModel(String[] suggestions)
        {
            Suggestions = suggestions;
        }
    }
}