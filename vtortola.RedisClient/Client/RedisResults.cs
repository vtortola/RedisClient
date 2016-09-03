using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    internal sealed class RedisResults : IReadOnlyList<IRedisResultInspector>, IRedisResults
    {
        readonly RESPObject[] _responses;
        readonly IRedisResultInspector[] _results;

        public Int32 Count { get { return _responses.Length; } }

        internal RedisResults(RESPObject[] responses, String[] commandHeaders = null)
        {
            ParameterGuard.CannotBeNull(responses, "responses");
                        
            _responses = responses;
            _results = new RedisResultInspector[_responses.Length];
        }

        public IRedisResultInspector this[Int32 index]
        {
            get 
            {
                ParameterGuard.IndexMustFitIn(index, "index", _results);
                IRedisResultInspector result = _results[index];
                if (result == null)
                    result = _results[index] = new RedisResultInspector(_responses[index], index+1);
                return result;
            }
        }
        public void ThrowErrorIfAny()
        {
            List<Exception> errors = null;
            for (var i = 0; i < _responses.Length; i++)
            {
                var response = _responses[i];
                if (response.Header == RESPHeaders.Error)
                {
                    errors = errors ?? new List<Exception>();
                    errors.Add(new RedisClientCommandException((RESPError)response, i + 1));
                }
            }
            if (errors != null)
                throw new AggregateException(errors);
        }

        public IEnumerator<IRedisResultInspector> GetEnumerator()
        {
            for (int i = 0; i < _results.Length; i++)
            {
                yield return this[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
