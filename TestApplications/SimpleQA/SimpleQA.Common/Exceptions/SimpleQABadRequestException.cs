using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SimpleQA
{
    public class SimpleQABadRequestException : SimpleQAException
    {
        public IReadOnlyList<ModelError> Errors { get; private set; }

        public SimpleQABadRequestException(ModelStateDictionary modelState)
            :base("The request does not contain data in the expected format.")
        {
            Errors = modelState.Values.SelectMany(v => v.Errors).ToList().AsReadOnly();
        }
    }
}