using System;

namespace vtortola.Redis
{
    internal static class DisposeHelper
    {
        internal static void SafeDispose(IDisposable disposable)
        {
            if (disposable != null)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception) { }
            }
        }
    }
}
