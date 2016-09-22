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
                                        "QuestionRequest {question} @id @userId",
                                        new
                                        {
                                            id = request.Id,
                                            userId = user.GetSimpleQAIdentity().Id
                                        })
                                        .ConfigureAwait(false);

            CheckException(result);

            var complex = result[0].AsResults();
            var question = new QuestionViewModel();
            complex[0].AsObjectCollation(question);

            question.User = complex[1].GetString();
            question.Tags = complex[2].GetStringArray();
            question.Id = request.Id;
            question.AuthoredByUser = question.User == user.Identity.Name;
            question.UpVoted = GetVote(complex[3].GetString());
            question.UserVotedClose = complex[4].AsInteger() == 1;
            
            question.Answers = ExtractAnswers(user, complex[5].AsResults(), question);

            return question;
        }

        static void CheckException(IRedisResults result)
        {
            var error = result[0].GetException();
            if (error != null)
            {
                switch (error.Prefix)
                {
                    case "NOTFOUND":
                        throw new SimpleQAException("Question not found");

                    default: throw error;
                }
            }
        }

        private static List<AnswerViewModel> ExtractAnswers(IPrincipal user, IRedisResults results, QuestionViewModel question)
        {
            var answers = new List<AnswerViewModel>();
            foreach (var answerData in results)
            {
                var answerResult = answerData.AsResults();

                var answer = new AnswerViewModel();
                answerResult[0].AsObjectCollation(answer);
                answer.User = answerResult[1].GetString();
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
