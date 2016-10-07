using SimpleQA;
using SimpleQA.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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

        public IDictionary<String, String> Process(XDocument doc, IDictionary<String, String> usermap)
        {
            
            var idmap = new ConcurrentDictionary<String, String>();

            var questions = doc.Element("posts").Elements().Where(q => q.Attribute("PostTypeId").Value == "1");
            Parallel.ForEach(questions, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, question =>
            {
                try
                {
                    AppendQuestion(question, idmap, usermap);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Q_ERROR: " + ex.Message);
                    Console.ResetColor();
                }
            });

            var answers = doc.Element("posts").Elements().Where(q => q.Attribute("PostTypeId").Value == "2" && q.Attribute("ParentId") != null);
            Parallel.ForEach(answers, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, answer =>
            {
                try
                {
                    AppendAnswer(idmap[answer.Attribute("ParentId").Value], answer, idmap, usermap);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("A_ERROR: " + ex.Message);
                    Console.ResetColor();
                }
            });

            return idmap;
        }

        private void AppendQuestion(XElement question, ConcurrentDictionary<String, String> idmap, IDictionary<String, String> usermap)
        {
            if (question.Attribute("OwnerUserId") == null || question.Attribute("Id") == null)
                return;

            var id = question.Attribute("Id").Value;
            var userId = question.Attribute("OwnerUserId").Value;

            var user = new SimpleQAPrincipal(usermap[userId], "whatever","", 0);

            var tags = Regex.Matches(question.Attribute("Tags").Value, "<(.*?)>")
                            .OfType<Match>()
                            .Select(m => m.ToString())
                            .Select(s => s.Substring(1, s.Length - 2))
                            .ToArray();

            var creationDate = DateTime.Parse(question.Attribute("CreationDate").Value);
            var views = Int32.Parse(question.Attribute("ViewCount").Value);

            var command = new QuestionCreateCommand(question.Attribute("Title").Value,
                                                    question.Attribute("Body").Value, 
                                                    question.Attribute("Body").Value,
                                                    creationDate,
                                                    tags);
            var sw = new Stopwatch();
            sw.Start();
            var result = _mediator.ExecuteAsync<QuestionCreateCommand, QuestionCreateCommandResult>(command, user, CancellationToken.None).Result;
            sw.Stop();

            var viewCommand = new VisitQuestionCommand(result.Id, views);
            _mediator.ExecuteAsync<VisitQuestionCommand, VisitQuestionCommandResult>(viewCommand, user, CancellationToken.None).Wait();

            Console.WriteLine("Added question: " + result.Slug + ",  " + question.Attribute("Body").Value.Length + " chars in " + sw.ElapsedMilliseconds + " ms.");

            idmap.TryAdd(id, result.Id);
        }

        private void AppendAnswer(String questionId, XElement answer, ConcurrentDictionary<String, String> idmap, IDictionary<String, String> usermap)
        {
            if (answer.Attribute("OwnerUserId") == null || answer.Attribute("Id") == null)
                return;

            var userId = answer.Attribute("OwnerUserId").Value;
            var creationDate = DateTime.Parse(answer.Attribute("CreationDate").Value);

            var command = new AnswerCreateCommand(questionId, 
                                                  creationDate,
                                                  answer.Attribute("Body").Value, 
                                                  answer.Attribute("Body").Value);

            var user = new SimpleQAPrincipal(usermap[userId], "whatever", "", 0);
            var result = _mediator.ExecuteAsync<AnswerCreateCommand, AnswerCreateCommandResult>(command, user, CancellationToken.None).Result;

            idmap.TryAdd(answer.Attribute("Id").Value, result.QuestionId + "@" + result.AnswerId);

            Console.WriteLine("Added answer for: " + questionId);
        }
    }
}
