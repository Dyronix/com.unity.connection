using Unity.Connection.Method;

namespace Unity.Connection.State
{
    /// <summary>
    /// Connection state corresponding to when the NetworkManager is shut down. From this state we can transition to the
    /// ClientConnecting sate, if starting as a client, or the StartingHost state, if starting as a host.
    /// </summary>
    public class OfflineState : ConnectionState
    {
        //--------------------------------------------------------------------------------------
        // Properties
        public override ConnectionStateType Type => ConnectionStateType.OFFLINE;

        //--------------------------------------------------------------------------------------
        public OfflineState(Connection connection) 
            : base(connection) 
        { }

        //--------------------------------------------------------------------------------------
        public override void Enter()
        {
            Connection.LobbyServiceFacade.StopTrackingActiveLobby();

            Connection.NetworkManager.Shutdown();
        }

        //--------------------------------------------------------------------------------------
        public override void Exit()
        {
            // Nothing to implement
        }

        //--------------------------------------------------------------------------------------
        public override void StartClientLobby(string playerName)
        {
            var connection_method = new ConnectionMethodRelay(Connection, playerName);
            Connection.ChangeConnectionState(new ClientConnectingState(Connection, connection_method));
        }

        //--------------------------------------------------------------------------------------
        public override void StartHostLobby(string playerName)
        {
            var connection_method = new ConnectionMethodRelay(Connection, playerName);
            Connection.ChangeConnectionState(new StartingHostState(Connection, connection_method));
        }
    }
}