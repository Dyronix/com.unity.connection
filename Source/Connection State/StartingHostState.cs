using Unity.Connection.Method;
using Unity.Netcode;
using UnityEngine;

namespace Unity.Connection.State
{
    public class StartingHostState : ConfigurableState
    {
        //--------------------------------------------------------------------------------------
        // Properties
        public override ConnectionStateType Type => ConnectionStateType.STARTHOSTING;

        //--------------------------------------------------------------------------------------
        public StartingHostState(Connection connection, ConnectionMethodBase baseConnectionMethod)
            :base(connection, baseConnectionMethod)
        {}

        //--------------------------------------------------------------------------------------
        public override void Enter()
        {
            StartHost();
        }

        //--------------------------------------------------------------------------------------
        public override void Exit()
        {
            // Nothing to implement
        }

        //--------------------------------------------------------------------------------------
        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var client_id = request.ClientNetworkId;

            // This happens when starting as a host, before the end of the StartHost call. In that case, we simply approve ourselves.
            if (client_id == Connection.NetworkManager.LocalClientId)
            {
                // connection approval will create a player object for you
                response.Approved = true;
                response.CreatePlayerObject = true;
            }
        }

        //--------------------------------------------------------------------------------------
        public override void OnServerStarted()
        {
            OnConnectionStatusChanged?.Invoke(ConnectionStatusType.SUCCESS);
            Connection.ChangeConnectionState(new HostingState(Connection));
        }

        //--------------------------------------------------------------------------------------
        public override void OnServerStopped()
        {
            StartHostFailed();
        }

        //--------------------------------------------------------------------------------------
        private async void StartHost()
        {
            await ConnectionMethod.SetupHostConnectionAsync();

            Debug.Log("Created relay allocation: " + Connection.LobbyServiceFacade.RelayCode);

            // NGO's StartHost launches everything
            if (!Connection.NetworkManager.StartHost())
            {
                Debug.LogError("Network Manager Error: StartHost failed.");
                StartHostFailed();
            }
        }

        //--------------------------------------------------------------------------------------
        private void StartHostFailed()
        {
            string disconnect_reason = Connection.NetworkManager.DisconnectReason;

            if (string.IsNullOrEmpty(disconnect_reason))
            {
                OnConnectionStatusChanged?.Invoke(ConnectionStatusType.STARTHOSTFAILED);
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