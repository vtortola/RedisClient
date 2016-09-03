using System;
using System.Threading;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    /// <summary>
    /// <see cref="IRedisChannel"/> extension methods.
    /// </summary>
    public static class RedisChannelExtensions
    {
        /// <summary>
        /// Executes asynchronously the given command.
        /// </summary>
        /// <param name="channel"><see cref="IRedisChannel"/></param>
        /// <param name="command">The Redis command.</param>
        public static Task<IRedisResults> ExecuteAsync(this IRedisChannel channel, String command)
        {
            return channel.ExecuteAsync<Object>(command, null, CancellationToken.None);
        }

        /// <summary>
        /// Executes asynchronously the given command.
        /// </summary>
        /// <param name="channel"><see cref="IRedisChannel"/></param>
        /// <param name="command">The Redis command.</param>
        /// <param name="cancel">A token that can cancel the execution.</param>
        public static Task<IRedisResults> ExecuteAsync(this IRedisChannel channel, String command, CancellationToken cancel)
        {
            return channel.ExecuteAsync<Object>(command, null, cancel);
        }

        /// <summary>
        /// Executes asynchronously the given command.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"><see cref="IRedisChannel"/></param>
        /// <param name="command">The Redis command.</param>
        /// <param name="parameters">The object with properties will be used as parameters.</param>
        public static Task<IRedisResults> ExecuteAsync<T>(this IRedisChannel channel, String command, T parameters) where T : class
        {
            return channel.ExecuteAsync(command, parameters, CancellationToken.None);
        }

        /// <summary>
        /// Executes synchronously the given command.
        /// </summary>
        /// <param name="channel"><see cref="IRedisChannel"/></param>
        /// <param name="command">The Redis command.</param>
        /// <returns><seealso cref="IRedisResults"/></returns>
        public static IRedisResults Execute(this IRedisChannel channel, String command)
        {
            return channel.Execute<Object>(command, null, CancellationToken.None);
        }

        /// <summary>
        /// Executes synchronously the given command.
        /// </summary>
        /// <param name="channel"><see cref="IRedisChannel"/></param>
        /// <param name="command">The Redis command.</param>
        /// <param name="cancel">A token that can cancel the execution.</param>
        /// <returns><seealso cref="IRedisResults"/></returns>
        public static IRedisResults Execute(this IRedisChannel channel, String command, CancellationToken cancel)
        {
            return channel.Execute<Object>(command, null, cancel);
        }

        /// <summary>
        /// Executes synchronously the given command.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"><see cref="IRedisChannel"/></param>
        /// <param name="command">The Redis command.</param>
        /// <param name="parameters">The object with properties will be used as parameters.</param>
        /// <returns><seealso cref="IRedisResults"/></returns>
        public static IRedisResults Execute<T>(this IRedisChannel channel, String command, T parameters) where T : class
        {
            return channel.Execute(command, parameters, CancellationToken.None);
        }

        /// <summary>
        /// Executes the given command without waiting.
        /// </summary>
        /// <param name="channel"><see cref="IRedisChannel"/></param>
        /// <param name="command">The Redis command.</param>
        public static void Dispatch(this IRedisChannel channel, String command)
        {
            channel.Dispatch<Object>(command, null);
        }
    }
}
