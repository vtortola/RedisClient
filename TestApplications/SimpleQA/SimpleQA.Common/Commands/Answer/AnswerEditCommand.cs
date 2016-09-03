using System;

namespace SimpleQA.Commands
{
    public class AnswerEditCommand : ICommand<AnswerEditCommandResult>
    {
        public String QuestionId { get; private set; }
        public String AnswerId { get; private set; }
        public String Content { get; private set; }
        public String HtmlContent { get; private set; }

        public AnswerEditCommand(String questionId, String answerId, String content, String htmlContent)
        {
            QuestionId = questionId;
            AnswerId = answerId;
            Content = content;
            HtmlContent = htmlContent;
        }
    }

    public class AnswerEditCommandResult
    {
        public String QuestionId { get; private set; }
        public String Slug { get; private set; }
        public String AnswerId { get; set; }

        public AnswerEditCommandResult(String questionId, String slug, String answerId)
        {
            QuestionId = questionId;
            Slug = slug;
            AnswerId = answerId;
        }
    }
}