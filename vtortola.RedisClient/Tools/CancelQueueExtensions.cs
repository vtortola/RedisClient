using System.Collections.Concurrent;

namespace vtortola.Redis
{
    internal static class CancelQueueExtensions
    {
        internal static void CancelTokens(this ConcurrentQueue<ExecutionToken> queue)
        {
            ExecutionToken token;
            while(queue.TryDequeue(out token))
                CancelToken(token);
        }

        internal static void CancelTokens(this BlockingCollection<ExecutionToken> queue)
        {
            ExecutionToken token;
            while (queue.TryTake(out token))
                CancelToken(token);
        }

        private static void CancelToken(ExecutionToken token)
        {
            try
            {
                token.SetCancelled();
            }
            catch
            {
                // it does not matter 
            }
        }
    }
}
