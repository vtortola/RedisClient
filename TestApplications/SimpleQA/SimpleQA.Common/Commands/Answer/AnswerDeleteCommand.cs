using System;

namespace SimpleQA.Commands
{
    public class AnswerDeleteCommand : ICommand<AnswerDeleteCommandResult>
    {
        public String QuestionId { get; private set; }
        public String AnswerId { get; private set; }

        public AnswerDeleteCommand(String questionId, String answerId)
        {
            QuestionId = questionId;
            AnswerId = answerId;
        }
    }

    public class AnswerDeleteCommandResult
    {
        public String QuestionId { get; private set; }
        public String Slug { get; private set; }

        public AnswerDeleteCommandResult(String questionId, String slug)
        {
            QuestionId = questionId;
            Slug = slug;
        }
    }
}