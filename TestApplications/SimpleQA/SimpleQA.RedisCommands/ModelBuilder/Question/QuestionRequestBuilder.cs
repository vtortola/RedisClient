using SimpleQA.Models;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class QuestionRequestBuilder : IModelBuilder<QuestionRequest, QuestionViewModel>
    {
        readonly IRedisChannel _channel;
        public QuestionRequestBuilder(IRedisChannel channel)
        {
            _channel = channel;
        }

        static Boolean? GetVote(String value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return null;
            if (value == "0")
                return null;
            if (value == "1")
                return true;
            if (value == "-1")
                return false;
            throw new InvalidOperationException("Unexpected vote value: " + value);
        }

        public async Task<QuestionViewModel> BuildAsync(QuestionRequest request, IPrincipal user, CancellationToken cancel)
        {
            var result = await _channel.ExecuteAsync(
                                        "QuestionRequest {question} @id @user",
                                        new
                                        {
                                            id = request.Id,
                                            user = user.Identity.Name
                                        })
                                        .ConfigureAwait(false);

            var error = result[0].GetException();
            if(error != null)
            {
                switch (error.Prefix)
                {
                    case "NOTFOUND":
                        throw new SimpleQAException("Question not found");

                    default: throw error;
                }
            }

            var complex = result[0].AsResults();
            var question = new QuestionViewModel();
            complex[0].AsObjectCollation(question);

            question.Id = request.Id;
            question.AuthoredByUser = question.User == user.Identity.Name;
            question.UserVotedClose = complex[3].AsInteger() == 1;
            question.UpVoted = GetVote(complex[2].GetString());
            question.Tags = complex[1].GetStringArray();
            question.Answers = ExtractAnswers(user, complex, question);

            return question;
        }

        private static List<AnswerViewModel> ExtractAnswers(IPrincipal user, IRedisResults complex, QuestionViewModel question)
        {
            var answers = new List<AnswerViewModel>();
            foreach (var answerData in complex[4].AsResults())
            {
                var answerResult = answerData.AsResults();

                var answer = new AnswerViewModel();
                answerResult[0].AsObjectCollation(answer);
                answer.QuestionId = question.Id;
                answer.Editable = question.Status == QuestionStatus.Open;
                answer.Votable = question.Votable;
                answer.AuthoredByUser = answer.User == user.Identity.Name;
                answer.UpVoted = GetVote(answerResult[2].GetString());
                answers.Add(answer);
            }
            return answers;
        }
    }
}
