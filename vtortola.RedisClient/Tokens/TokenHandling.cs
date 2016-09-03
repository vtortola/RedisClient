using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace vtortola.Redis
{
    internal static class TokenHandling
    {
        internal static BlockingCollection<T> CreateQueue<T>(Int32? boundedCapacity)
            where T:ExecutionToken
        {
            if (boundedCapacity.HasValue)
                return new BlockingCollection<T>(boundedCapacity.Value);
            else
                return new BlockingCollection<T>();
        }

        internal static T SafeDequeue<T>(BlockingCollection<T> queue, CancellationToken cancel)
            where T : class
        {
            T token = null;

            try
            {
                token = queue.Take(cancel);
            }
            catch (OperationCanceledException) { }
            catch (InvalidOperationException) { }
            catch (ArgumentNullException) { }

            return token;
        }

        internal static void ProcessToken<T>(T token, Action<T, CancellationToken> process, CancellationToken cancel)
            where T : ExecutionToken
        {
            try
            {
                process(token, cancel);
            }
            catch (OperationCanceledException)
            {
                token.SetCancelled();
                throw;
            }
            catch (ObjectDisposedException) 
            {
                token.SetCancelled();
                throw;
            }
            catch (Exception ex)
            {
                token.SetFaulted(ex);
                throw;
            }
        }

        internal static void ProcessTokensLoop<T>(BlockingCollection<T> queue, Action<T, CancellationToken> process, CancellationToken cancel)
            where T : ExecutionToken
        {
            try
            {
                while (!cancel.IsCancellationRequested)
                {
                    var token = SafeDequeue<T>(queue, cancel);

                    if (token == null)
                        return;

                    ProcessToken(token, process, cancel);
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (IOException) { }
            catch (NullReferenceException) { }
        }
    }
}
