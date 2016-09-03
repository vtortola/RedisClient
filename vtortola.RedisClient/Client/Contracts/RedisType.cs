
namespace vtortola.Redis
{
    /// <summary>
    /// Kinds of Redis response
    /// </summary>
    public enum RedisType
    {
        /// <summary>
        /// String.
        /// </summary>
        String,

        /// <summary>
        /// 64 bits integer
        /// </summary>
        Integer,

        /// <summary>
        /// Array. Collection of other Redis types.
        /// </summary>
        Array,

        /// <summary>
        /// An error.
        /// </summary>
        Error
    }
}
