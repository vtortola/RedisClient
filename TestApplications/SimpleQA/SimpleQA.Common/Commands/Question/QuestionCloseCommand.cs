using System;

namespace SimpleQA.Commands
{
    public class QuestionCloseCommand : ICommand<QuestionCloseCommandResult>
    {
        public String Id { get; private set; }
        public QuestionCloseCommand(String id)
        {
            Id = id;
        }
    }

    public class QuestionCloseCommandResult
    {
        public String Id { get; private set; }
        public String Slug { get; private set; }

        public QuestionCloseCommandResult(String id, String slug)
        {
            Id = id;
            Slug = slug;
        }
    }
}