using System.Threading.Tasks;
using Unity.Connection.Method;
using UnityEngine;

namespace Unity.Connection.State
{
    public class ClientConnectingState : ConfigurableState
    {
        //--------------------------------------------------------------------------------------
        // Properties
        public override ConnectionStateType Type => ConnectionStateType.CLIENTCONNECTING;

        //--------------------------------------------------------------------------------------
        public ClientConnectingState(Connection connection, ConnectionMethodBase baseConnectionMethod)
            :base(connection, baseConnectionMethod)
        {}

        //--------------------------------------------------------------------------------------
        public override void Enter()
        {
#pragma warning disable 4014
            ConnectClientAsync();
#pragma warning restore 4014
        }

        //--------------------------------------------------------------------------------------
        public override void Exit()
        {
            // Nothing to implement
        }

        //--------------------------------------------------------------------------------------
        public override void OnClientConnected(ulong _)
        {
            OnConnectionStatusChanged?.Invoke(ConnectionStatusType.SUCCESS);
            Connection.ChangeConnectionState(new ClientConnectedState(Connection));
        }

        //--------------------------------------------------------------------------------------
        public override void OnClientDisconnect(ulong _)
        {
            // client ID is for sure ours here
            StartingClientFailed();
        }

        //--------------------------------------------------------------------------------------
        private async Task ConnectClientAsync()
        {
            // Setup NGO with current connection method
            await ConnectionMethod.SetupClientConnectionAsync();

            Debug.Log("Joined relay allocation: " + Connection.LobbyServiceFacade.RelayCode);

            // NGO's StartClient launches everything
            if (!Connection.NetworkManager.StartClient())
            {
                Debug.LogError("Network Manager Error: StartClient failed.");
                StartingClientFailed();
            }
        }

        //--------------------------------------------------------------------------------------
        private void StartingClientFailed()
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