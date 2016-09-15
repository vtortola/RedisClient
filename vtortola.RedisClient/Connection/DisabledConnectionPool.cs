using System;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    internal sealed class DisabledConnectionPool : IConnectionProvider<ICommandConnection>
    {
        internal static readonly DisabledConnectionPool Instance = new DisabledConnectionPool();

        private DisabledConnectionPool(){}

        public ICommandConnection Provide()
        {
            throw new RedisClientConfigurationException("The exclusive connection pool is diabled by default, check the ExclusivePoolOptions.Minimum and ExclusivePoolOptions.Maximum.");
        }

        public Task ConnectAsync(System.Threading.CancellationToken cancel)
        {
            return Task.FromResult<Object>(null);
        }

        public void Dispose(){ }
    }
}
