using System;

namespace SimpleQA.Commands
{
    public class QuestionDeleteCommand : ICommand<QuestionDeleteCommandResult>
    {
        public String Id { get; private set; }
        public QuestionDeleteCommand(String id)
        {
            Id = id;
        }
    }

    public class QuestionDeleteCommandResult
    {
        public String Id { get; private set; }
        public String Slug { get; private set; }

        public QuestionDeleteCommandResult(String id, String slug)
        {
            Id = id;
            Slug = slug;
        }
    }
}