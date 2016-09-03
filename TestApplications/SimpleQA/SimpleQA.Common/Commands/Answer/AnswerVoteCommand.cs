using System;

namespace SimpleQA.Commands
{
    public class AnswerVoteCommand : ICommand<AnswerVoteCommandResult>
    {
        public String QuestionId { get; private set; }
        public String AnswerId { get; private set; }
        public Boolean Upvote { get; private set; }

        public AnswerVoteCommand(String questionId, String answerId, Boolean upvote)
        {
            QuestionId = questionId;
            AnswerId = answerId;
            Upvote = upvote;
        }
    }

    public class AnswerVoteCommandResult
    {
        public Int64 Votes { get; private set; }

        public AnswerVoteCommandResult(Int64 votes)
        {
            Votes = votes;
        }
    }
}