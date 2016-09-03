using System;

namespace SimpleQA.Commands
{
    public sealed class QuestionVoteCommand : ICommand<QuestionVoteCommandResult>
    {
        public String QuestionId { get; private set; }
        public Boolean Upvote { get; private set; }

        public QuestionVoteCommand(String questionId, Boolean upvote)
        {
            QuestionId = questionId;
            Upvote = upvote;
        }
    }

    public sealed class QuestionVoteCommandResult
    {
        public Int64 Votes { get; private set; }

        public QuestionVoteCommandResult(Int64 votes)
        {
            Votes = votes;
        }
    }
}