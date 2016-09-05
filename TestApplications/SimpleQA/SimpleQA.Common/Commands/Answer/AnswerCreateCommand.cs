using SimpleQA.Models;
using System;

namespace SimpleQA.Commands
{
    public class AnswerCreateCommand : ICommand<AnswerCreateCommandResult>
    {
        public String QuestionId { get; private set; }
        public String Content { get; private set; }
        public String HtmlContent { get; private set; }
        public DateTime CreationDate { get; private set; }

        public AnswerCreateCommand(String questionId, DateTime creationDate, String content, String htmlContent)
        {
            QuestionId = questionId;
            Content = content;
            HtmlContent = htmlContent;
            CreationDate = creationDate;
        }
    }

    public class AnswerCreateCommandResult
    {
        public String QuestionId { get; private set; }
        public String Slug { get; private set; }
        public String AnswerId { get; set; }

        public AnswerCreateCommandResult(String questionId, String slug, String answerId)
        {
            QuestionId = questionId;
            Slug = slug;
            AnswerId = answerId;
        }
    }
}