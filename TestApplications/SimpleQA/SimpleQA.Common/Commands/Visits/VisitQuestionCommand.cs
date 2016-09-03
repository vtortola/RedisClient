using SimpleQA.Commands;
using SimpleQA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQA.Commands
{
    public class VisitQuestionCommand : ICommand<VisitQuestionCommandResult>
    {
        public String QuestionId { get; private set; }

        public VisitQuestionCommand(String questionId)
	    {
            QuestionId = questionId;
	    }
    }

    public class VisitQuestionCommandResult
    {
        public static VisitQuestionCommandResult Nothing = new VisitQuestionCommandResult();
    }
}
