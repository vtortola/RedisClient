using SimpleQA.Commands;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
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
            foreach (var vote in votes)
            {
                var voteType = vote.Attribute("VoteTypeId").Value;
                switch (voteType)
                {
                    case "5": VotePost(vote, usermap, postmap);
                        break;
                }
            }
        }

        private void VotePost(XElement vote, IDictionary<String, String> usermap, IDictionary<String, String> postmap)
        {
            var userId = vote.Attribute("UserId").Value;
            var user = new GenericPrincipal(new GenericIdentity(usermap[userId]), null);

            if (!postmap.ContainsKey(vote.Attribute("PostId").Value))
                return;

            try
            {
                var post = postmap[vote.Attribute("PostId").Value];
                if (post.Contains("@"))
                {
                    var parts = post.Split('@');
                    VoteAnswer(user, parts[0], parts[1]);
                }
                else
                {
                    VoteQuestion(user, post);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetBaseException().Message);
            }
        }

        private void VoteAnswer(GenericPrincipal user, String questionId, String answerId)
        {
            var command = new AnswerVoteCommand(questionId, answerId, true);
            var result = _mediator.ExecuteAsync<AnswerVoteCommand, AnswerVoteCommandResult>(command, user, CancellationToken.None).Result;

        }

        private void VoteQuestion(GenericPrincipal user, String questionId)
        {
            var command = new QuestionVoteCommand(questionId, true);
            var result = _mediator.ExecuteAsync<QuestionVoteCommand, QuestionVoteCommandResult>(command, user, CancellationToken.None).Result;
        }
    }
}