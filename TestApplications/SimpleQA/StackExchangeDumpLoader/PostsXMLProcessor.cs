using SimpleQA.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml.Linq;

namespace StackExchangeDumpLoader
{
    public class PostsXMLProcessor
    {
        ICommandExecuterMediator _mediator;

        public PostsXMLProcessor(ICommandExecuterMediator mediator)
        {
            _mediator = mediator;
        }

        public void Process(XDocument doc)
        {
            var posts = doc.Element("posts").Elements();
            foreach (var post in posts)
            {
                var postType = post.Attribute("PostTypeId").Value;
                switch (postType)
                {
                    case "1": AppendQuestion(post, posts);
                        break;
                }
            }
        }

        private void AppendQuestion(XElement question, IEnumerable<XElement> posts)
        {
            var id = question.Attribute("Id").Value;
            var userId = question.Attribute("OwnerUserId").Value;

            var user = new GenericPrincipal(new GenericIdentity(userId), null);

            var tags = Regex.Matches(question.Attribute("Tags").Value, "<(.*?)>")
                            .OfType<Match>()
                            .Select(m => m.ToString())
                            .Select(s => s.Substring(1, s.Length - 2))
                            .ToArray();

            var command = new QuestionCreateCommand(question.Attribute("Title").Value,
                                                    HttpUtility.HtmlDecode(question.Attribute("Body").Value), 
                                                    question.Attribute("Body").Value,
                                                    tags);

            var result = _mediator.ExecuteAsync<QuestionCreateCommand, QuestionCreateCommandResult>(command, user, CancellationToken.None).Result;

            var answers = posts.Where(p => p.Attribute("PostTypeId").Value == "2" && p.Attribute("ParentId").Value == id);
            foreach (var answer in answers)
            {
                AppendAnswer(result.Id, answer);
            }

        }

        private void AppendAnswer(String questionId, XElement answer)
        {
            var userId = answer.Attribute("OwnerUserId").Value; 

            var command = new AnswerCreateCommand(questionId, 
                                                  HttpUtility.HtmlDecode(answer.Attribute("Body").Value), 
                                                  answer.Attribute("Body").Value);

            var user = new GenericPrincipal(new GenericIdentity(userId), null);
            _mediator.ExecuteAsync<AnswerCreateCommand, AnswerCreateCommandResult>(command, user, CancellationToken.None).Wait();
        }
    }
}
