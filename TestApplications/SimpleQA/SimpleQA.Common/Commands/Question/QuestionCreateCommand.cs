using System;

namespace SimpleQA.Commands
{
    public sealed class QuestionCreateCommand : ICommand<QuestionCreateCommandResult>
    {
        public String Title { get; private set; }
        public String Content { get; private set; }
        public String HtmlContent { get; private set; }
        public String[] Tags { get; private set; }

        public QuestionCreateCommand(String title, String content, String htmlContent, String[] tags)
        {
            Title = title.Trim();
            Content = content;
            Tags = tags;
            HtmlContent = htmlContent;
        }
    }

    public sealed class QuestionCreateCommandResult
    {
        public String Id { get; private set; }
        public String Slug { get; set; }
        public QuestionCreateCommandResult(String id, String slug)
        {
            Id = id;
            Slug = slug;
        }
    }
}