using System;

namespace SimpleQA.Commands
{
    public class QuestionEditCommand : ICommand<QuestionEditCommandResult>
    {
        public String Id { get; private set; }
        public String Title { get; private set; }
        public String Content { get; private set; }
        public String HtmlContent { get; private set; }
        public String[] Tags { get; private set; }

        public QuestionEditCommand(String id, String title, String content, String htmlContent, String[] tags)
        {
            Id = id;
            Title = title;
            Content = content;
            Tags = tags;
            HtmlContent = htmlContent;
        }
    }

    public class QuestionEditCommandResult
    {
        public String Id { get; private set; }
        public String Slug { get; set; }
        public QuestionEditCommandResult(String id, String slug)
        {
            Id = id;
            Slug = slug;
        }
    }
}