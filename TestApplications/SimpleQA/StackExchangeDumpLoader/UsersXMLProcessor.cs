using SimpleQA.Commands;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Xml.Linq;

namespace StackExchangeDumpLoader
{
    public class UsersXMLProcessor
    {
        ICommandExecuterMediator _mediator;

        public UsersXMLProcessor(ICommandExecuterMediator mediator)
        {
            _mediator = mediator;
        }

        public Dictionary<String, String> Process(XDocument doc)
        {
            var users = doc.Element("users").Elements();
            var idmap = new Dictionary<String, String>();
            var anonymous = new GenericPrincipal(new GenericIdentity("anon"), null);
            foreach (var user in users)
            {
                var command = new AuthenticateCommand(user.Attribute("DisplayName").Value, "whatever");
                var result = _mediator.ExecuteAsync<AuthenticateCommand, AuthenticateCommandResult>(command, anonymous, CancellationToken.None).Result;
                idmap.Add(user.Attribute("Id").Value, command.Username);
            }
            return idmap;
        }
    }
}
