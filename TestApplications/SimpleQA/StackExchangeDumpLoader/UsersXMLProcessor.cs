using SimpleQA.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using vtortola.Redis;
using System.Linq;

namespace StackExchangeDumpLoader
{
    public class UsersXMLProcessor
    {
        ICommandExecuterMediator _mediator;
        IRedisChannel _channel;

        public UsersXMLProcessor(ICommandExecuterMediator mediator, IRedisChannel channel)
        {
            _mediator = mediator;
            _channel = channel;
        }

        public IDictionary<String, String> Process(XDocument doc)
        {
            var users = doc.Element("users").Elements();
            var idmap = new ConcurrentDictionary<String, String>();
            var anonymous = new GenericPrincipal(new GenericIdentity("dumpprocessor"), null);

            Parallel.ForEach(users, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, user =>
            {
                var command = new AuthenticateCommand(user.Attribute("DisplayName").Value, "whatever");
                var result = _mediator.ExecuteAsync<AuthenticateCommand, AuthenticateCommandResult>(command, anonymous, CancellationToken.None).Result;
                idmap.TryAdd(user.Attribute("Id").Value, command.Username);
                _channel.Execute("sadd users:builtin @user", new { user = command.Username }).ThrowErrorIfAny();
                Console.WriteLine("Added user: " + command.Username);
            });

            return idmap;
        }
    }
}
