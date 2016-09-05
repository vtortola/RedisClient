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
        public Int32 Views { get; private set; }

        public VisitQuestionCommand(String questionId)
            :this(questionId, 1)
	    {

	    }

        public VisitQuestionCommand(String questionId, Int32 views)
        {
            QuestionId = questionId;
            Views = views;
        }
    }

    public class VisitQuestionCommandResult
    {
        public static VisitQuestionCommandResult Nothing = new VisitQuestionCommandResult();
    }
}
