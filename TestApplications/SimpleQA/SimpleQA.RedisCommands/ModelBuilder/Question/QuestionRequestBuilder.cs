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

        private Boolean? GetVote(String value)
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
            var result = await _channel.ExecuteAsync(@"
                                        HGETALL @key
                                        SMEMBERS @tagKey
                                        SMEMBERS @answerKey
                                        ZSCORE @votesKey @key
                                        SISMEMBER @closeVotes @user",
                                        new
                                        {
                                            key = Keys.QuestionKey(request.Id),
                                            tagKey = Keys.QuestionTagsKey(request.Id),
                                            answerKey = Keys.QuestionAnswerCollectionKey(request.Id),
                                            votesKey = Keys.UserVotesKey(user.Identity.Name),
                                            user = user.Identity.Name,
                                            closeVotes = Keys.QuestionCloseVotesCollectionKey(request.Id)
                                        }).ConfigureAwait(false);

            if (result[0].GetArray().Length == 0)
                throw new SimpleQAException("Question not found");

            var question = new QuestionViewModel();
            result[0].AsObjectCollation(question);

            question.Id = request.Id;
            question.AuthoredByUser = question.User == user.Identity.Name;
            question.UserVotedClose = result[4].GetInteger() == 1;
            question.UpVoted = GetVote(result[3].GetString());
            question.Tags = result[1].GetStringArray();

            var answers = new List<AnswerViewModel>();
            foreach (var answerId in result[2].GetStringArray())
            {
                result = await _channel.ExecuteAsync(@"
                                        HGETALL @answerId
                                        ZSCORE @votesKey @user",
                                        new
                                        {
                                            answerId,
                                            votesKey = answerId + ":votes",
                                            user = user.Identity.IsAuthenticated ? user.Identity.Name : "__anon__"
                                        }).ConfigureAwait(false);

                var answer = new AnswerViewModel();
                result[0].AsObjectCollation(answer);
                answer.QuestionId = question.Id;
                answer.Editable = question.Status == QuestionStatus.Open;
                answer.Votable = question.Votable;
                answer.AuthoredByUser = answer.User == user.Identity.Name;
                answer.UpVoted = result[1].GetString() == null ? (Boolean?)null : (Int32.Parse(result[1].GetString()) > 0 ? true : false);
                answers.Add(answer);
            }

            question.Answers = answers;

            return question;
        }
    }
}
