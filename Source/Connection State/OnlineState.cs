namespace Unity.Connection.State
{
    /// <summary>
    /// Base class representing an online connection state.
    /// </summary>
    public abstract class OnlineState : ConnectionState
    {
        //--------------------------------------------------------------------------------------
        public OnlineState(Connection connection)
            : base(connection)
        { }

        //--------------------------------------------------------------------------------------
        public override void OnUserRequestedShutdown()
        {
            // This behaviour will be the same for every online state
            Connection.ChangeConnectionState(new OfflineState(Connection));
        }
        //--------------------------------------------------------------------------------------
        public override void OnTransportFailure()
        {
            // This behaviour will be the same for every online state
            Connection.ChangeConnectionState(new OfflineState(Connection));
        }
    }
}