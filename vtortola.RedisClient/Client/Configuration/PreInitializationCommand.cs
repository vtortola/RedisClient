using System;

namespace vtortola.Redis
{
    /// <summary>
    /// Redis command that will be executed right after
    /// connecting in each connection.
    /// </summary>
    public sealed class PreInitializationCommand
    {
        /// <summary>
        /// The Redis command.
        /// </summary>
        public String Command { get; set; }

        /// <summary>
        /// The object which properties will be bind as parameters.
        /// </summary>
        public Object Parameters { get; set; }

        /// <summary>
        /// Initializes an empty instance of <see cref="PreInitializationCommand"/>
        /// </summary>
        public PreInitializationCommand() { /* Default constructor for serialization */ }

        /// <summary>
        /// Initializes an instance of <see cref="PreInitializationCommand"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        public PreInitializationCommand(String command, Object parameters = null)
        {
            Command = command;
            Parameters = parameters;
        }
    }
}
