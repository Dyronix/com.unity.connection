using UnityEngine;

namespace Unity.Connection.State
{
    public class ClientConnectedState : OnlineState
    {
        //--------------------------------------------------------------------------------------
        // Properties
        public override ConnectionStateType Type => ConnectionStateType.CLIENTCONNECTED;

        //--------------------------------------------------------------------------------------
        public ClientConnectedState(Connection connection) 
            : base(connection) 
        { }

        //--------------------------------------------------------------------------------------
        public override void Enter()
        {
            Debug.Assert(Connection.LobbyServiceFacade.RemoteLobby != null, "Cannot track without a lobby");

            Connection.LobbyServiceFacade.BeginTrackingActiveLobby();
        }

        //--------------------------------------------------------------------------------------
        public override void Exit()
        {
            // Nothing to implement
        }

        //--------------------------------------------------------------------------------------
        public override void OnClientDisconnect(ulong clientId)
        {
            string disconnect_reason = Connection.NetworkManager.DisconnectReason;

            if (string.IsNullOrEmpty(disconnect_reason))
            {
                OnConnectionStatusChanged?.Invoke(ConnectionStatusType.STARTCLIENTFAILED);
            }
            else
            {
                ConnectionStatusType connect_status = JsonUtility.FromJson<ConnectionStatusType>(disconnect_reason);
                OnConnectionStatusChanged?.Invoke(connect_status);
            }

            Connection.ChangeConnectionState(new OfflineState(Connection));
        }
    }
}