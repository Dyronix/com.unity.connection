using Unity.Connection.Method;

namespace Unity.Connection.State
{
    /// <summary>
    /// Base class representing a configurable connection state.
    /// </summary>
    public abstract class ConfigurableState : OnlineState
    {
        //--------------------------------------------------------------------------------------
        // Properties
        public ConnectionMethodBase ConnectionMethod { get; private set; }

        //--------------------------------------------------------------------------------------
        public ConfigurableState(Connection connection, ConnectionMethodBase connectionMethod)
            :base(connection)
        {
            ConnectionMethod = connectionMethod;
        }
    }
}