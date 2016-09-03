using System;
using System.Threading;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    /// <summary>
    /// Delegate to handle Redis notifications.
    /// </summary>
    /// <param name="message"></param>
    public delegate void RedisMessageHandler(RedisNotification message);

    /// <summary>
    /// Virtual connection to Redis through <see cref="RedisClient"/>.
    /// Provides seamless access to both data and subscriptions.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IRedisChannel : IDisposable
    {
        /// <summary>
        /// Gets or sets the notification handler that will be called when this channel receives
        /// a message.
        /// </summary>
        RedisMessageHandler NotificationHandler { get; set; }

        /// <summary>
        /// Executes asynchronously the given command.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command">The Redis command.</param>
        /// <param name="parameters">The object with properties will be used as parameters.</param>
        /// <param name="cancel">A token that can cancel the execution.</param>
        /// <returns><seealso cref="IRedisResults"/></returns>
        Task<IRedisResults> ExecuteAsync<T>(String command, T parameters, CancellationToken cancel) where T : class;

        /// <summary>
        /// Executes synchronously the given command.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command">The Redis command.</param>
        /// <param name="parameters">The object with properties will be used as parameters.</param>
        /// <param name="cancel">A token that can cancel the execution.</param>
        /// <returns><seealso cref="IRedisResults"/></returns>
        IRedisResults Execute<T>(String command, T parameters, CancellationToken cancel) where T : class;

        /// <summary>
        /// Executes the given command without waiting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command">The Redis command.</param>
        /// <param name="parameters">The object with properties will be used as parameters.</param>
        void Dispatch<T>(String command, T parameters) where T : class;
    }
}
