using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UnitTests.Common
{
    public class ExpectedExceptionPatternAttribute : ExpectedExceptionBaseAttribute
    {
        public String MessagePattern { get; set; }
        public String StackTracePatter { get; set; }

        readonly Type _exceptionType, _innerExceptionType;

        public ExpectedExceptionPatternAttribute(Type exceptionType)
        {
            _exceptionType = exceptionType;
        }

        public ExpectedExceptionPatternAttribute(Type exceptionType, Type innerExceptionType)
            :this(exceptionType)
        {
            _innerExceptionType = innerExceptionType;
        }

        protected override void Verify(Exception exception)
        {
            if(exception == null)
                throw new AssertFailedException(NoExceptionMessage, exception);

            if (exception.GetType() != _exceptionType)
                throw new AssertFailedException("Expected exception type '"+ _exceptionType.Name+"' but found '"+exception.GetType().Name+"' ('"+exception.Message+"')", exception);

            if (_innerExceptionType != null && (exception.InnerException == null || exception.InnerException.GetType() != _innerExceptionType))
                throw new AssertFailedException("Expected inner exception type '" + _innerExceptionType.Name + "' but found '" + (exception.InnerException!=null?exception.InnerException.GetType().Name + "' ('" + exception.InnerException.Message + "')":"null"), exception);

            if(MessagePattern != null)
                if(!Regex.IsMatch(exception.Message, MessagePattern))
                    throw new AssertFailedException("Expected exception which message matches '" + MessagePattern + "', but message was '"+exception.Message+"'.", exception);

            if (StackTracePatter != null)
                if (!Regex.IsMatch(exception.Message, StackTracePatter))
                    throw new AssertFailedException("Expected exception which stacktrace matches '" + StackTracePatter + ", but message was '" + exception.Message + "'.", exception);

        }

        protected override String NoExceptionMessage
        {
            get
            {
                return "Expected exception type '" + _exceptionType.Name + "' but no exception found.";
            }
        }
    }
}
