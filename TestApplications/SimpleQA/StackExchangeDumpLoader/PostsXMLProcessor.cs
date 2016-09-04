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

        public Dictionary<String, String> Process(XDocument doc, IDictionary<String,String> usermap)
        {
            var posts = doc.Element("posts").Elements();
            var idmap = new Dictionary<String, String>();
            foreach (var post in posts)
            {
                var postType = post.Attribute("PostTypeId").Value;
                switch (postType)
                {
                    case "1": AppendQuestion(post, posts, idmap, usermap);
                        break;
                }
            }
            return idmap;
        }

        private void AppendQuestion(XElement question, IEnumerable<XElement> posts, IDictionary<String, String> idmap, IDictionary<String,String> usermap)
        {
            var id = question.Attribute("Id").Value;
            var userId = question.Attribute("OwnerUserId").Value;

            var user = new GenericPrincipal(new GenericIdentity(usermap[userId]), null);

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

            idmap.Add(id, result.Id);

            var answers = posts.Where(p => p.Attribute("PostTypeId").Value == "2" && p.Attribute("ParentId").Value == id);
            foreach (var answer in answers)
            {
                AppendAnswer(result.Id, answer, idmap, usermap);
            }
        }

        private void AppendAnswer(String questionId, XElement answer, IDictionary<String, String> idmap, IDictionary<String, String> usermap)
        {
            var userId = answer.Attribute("OwnerUserId").Value; 

            var command = new AnswerCreateCommand(questionId, 
                                                  HttpUtility.HtmlDecode(answer.Attribute("Body").Value), 
                                                  answer.Attribute("Body").Value);

            var user = new GenericPrincipal(new GenericIdentity(usermap[userId]), null);
            var result = _mediator.ExecuteAsync<AnswerCreateCommand, AnswerCreateCommandResult>(command, user, CancellationToken.None).Result;

            idmap.Add(answer.Attribute("Id").Value, result.QuestionId + "@" + result.AnswerId);
        }
    }
}
