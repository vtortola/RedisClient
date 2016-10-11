using SimpleQA;
using SimpleQA.Commands;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StackExchangeDumpLoader
{
    public class VotesXMLProcessor
    {
        ICommandExecuterMediator _mediator;

        public VotesXMLProcessor(ICommandExecuterMediator mediator)
        {
            _mediator = mediator;
        }

        public void Process(XDocument doc, IDictionary<String, String> usermap, IDictionary<String, String> postmap)
        {
            var votes = doc.Element("votes").Elements();
            Parallel.ForEach(votes, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, vote =>
            {
                var voteType = vote.Attribute("VoteTypeId").Value;
                switch (voteType)
                {
                    case "5": VotePost(vote, usermap, postmap);
                        break;
                }
            });
        }

        private void VotePost(XElement vote, IDictionary<String, String> usermap, IDictionary<String, String> postmap)
        {
            var userId = vote.Attribute("UserId").Value;
            var user = new SimpleQAIdentity(usermap[userId], "whatever", "", 0);

            if (!postmap.ContainsKey(vote.Attribute("PostId").Value))
                return;

            try
            {
                var post = postmap[vote.Attribute("PostId").Value];
                if (post.Contains("@"))
                {
                    var parts = post.Split('@');
                    VoteAnswer(user, parts[0], parts[1]);
                    Console.WriteLine("Answer " + post + " voted.");
                }
                else
                {
                    VoteQuestion(user, post);
                    Console.WriteLine("Question " + post + " voted.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetBaseException().Message);
            }
        }

        private void VoteAnswer(SimpleQAIdentity user, String questionId, String answerId)
        {
            var command = new AnswerVoteCommand(questionId, answerId, true);
            var result = _mediator.ExecuteAsync<AnswerVoteCommand, AnswerVoteCommandResult>(command, user, CancellationToken.None).Result;

        }

        private void VoteQuestion(SimpleQAIdentity user, String questionId)
        {
            var command = new QuestionVoteCommand(questionId, true);
            var result = _mediator.ExecuteAsync<QuestionVoteCommand, QuestionVoteCommandResult>(command, user, CancellationToken.None).Result;
        }
    }
}