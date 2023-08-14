using Unity.Connection.Method;
using Unity.Netcode;
using UnityEngine;

namespace Unity.Connection.State
{
    public class HostingState : OnlineState
    {
        //--------------------------------------------------------------------------------------
        // Constants
        // This is intended as a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
        private const int c_max_connect_payload = 1024;

        //--------------------------------------------------------------------------------------
        // Properties
        public override ConnectionStateType Type => ConnectionStateType.HOSTING;

        //--------------------------------------------------------------------------------------
        public HostingState(Connection connection) 
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
            Connection.SessionServiceFacade.OnServerEnded();
        }

        //--------------------------------------------------------------------------------------
        public override void OnClientConnected(ulong clientId)
        {
            ConnectionStatusType status_type = ConnectionStatusType.SUCCESS;

            OnClientConnectedToHost?.Invoke(status_type, clientId);
            OnConnectionStatusChanged?.Invoke(status_type);
        }

        //--------------------------------------------------------------------------------------
        public override void OnClientDisconnect(ulong clientId)
        {
            if (clientId != Connection.NetworkManager.LocalClientId)
            {
                var player_id = Connection.SessionServiceFacade.GetPlayerId(clientId);
                if (player_id != null)
                {
                    Connection.SessionServiceFacade.DisconnectClient(clientId);

                    ConnectionStatusType status_type = ConnectionStatusType.GENERICDISCONNECT;

                    OnClientDisconnectedFromHost?.Invoke(status_type, clientId);
                    OnConnectionStatusChanged?.Invoke(status_type);
                }
            }
        }

        //--------------------------------------------------------------------------------------
        public override void OnUserRequestedShutdown()
        {
            string reason = JsonUtility.ToJson(ConnectStatus.HostEndedSession);

            for (int i = Connection.NetworkManager.ConnectedClientsIds.Count - 1; i >= 0; i--)
            {
                ulong id = Connection.NetworkManager.ConnectedClientsIds[i];
                if (id != Connection.NetworkManager.LocalClientId)
                {
                    Connection.NetworkManager.DisconnectClient(id, reason);
                }
            }

            Connection.ChangeConnectionState(new OfflineState(Connection));
        }

        //--------------------------------------------------------------------------------------
        public override void OnServerStopped()
        {
            OnConnectionStatusChanged?.Invoke(ConnectionStatusType.GENERICDISCONNECT);
            Connection.ChangeConnectionState(new OfflineState(Connection));
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// This logic plugs into the "ConnectionApprovalResponse" exposed by Netcode.NetworkManager. It is run every time a client connects to us.
        /// The complementary logic that runs when the client starts its connection can be found in ClientConnectingState.
        /// </summary>
        /// <remarks>
        /// Multiple things can be done here, some asynchronously. For example, it could authenticate your user against an auth service like UGS' auth service. It can
        /// also send custom messages to connecting users before they receive their connection result (this is useful to set status messages client side
        /// when connection is refused, for example).
        /// Note on authentication: It's usually harder to justify having authentication in a client hosted game's connection approval. Since the host can't be trusted,
        /// clients shouldn't send it private authentication tokens you'd usually send to a dedicated server.
        /// </remarks>
        /// <param name="request"> The initial request contains, among other things, binary data passed into StartClient. In our case, this is the client's GUID,
        /// which is a unique identifier for their install of the game that persists across app restarts.
        ///  <param name="response"> Our response to the approval process. In case of connection refusal with custom return message, we delay using the Pending field.
        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var connection_data = request.Payload;

            if (connection_data.Length > c_max_connect_payload)
            {
                // If connection_data too high, deny immediately to avoid wasting time on the server. This is intended as
                // a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
                response.Approved = false;
                return;
            }

            var payload = System.Text.Encoding.UTF8.GetString(connection_data);
            var connection_payload = JsonUtility.FromJson<ConnectionPayload>(payload);
            var game_return_status = GetConnectStatus(connection_payload);

            if (game_return_status == ConnectStatus.Success)
            {
                // connection approval will create a player object for you
                response.Approved = true;
                response.CreatePlayerObject = true;
                response.Position = Vector3.zero;
                response.Rotation = Quaternion.identity;

                return;
            }

            response.Approved = false;
            response.Reason = JsonUtility.ToJson(game_return_status);

            Debug.Assert(Connection.LobbyServiceFacade.RemoteLobby != null, "Cannot track without a lobby");

            Connection.LobbyServiceFacade.KickPlayer(connection_payload.player_id);
        }

        //--------------------------------------------------------------------------------------
        ConnectStatus GetConnectStatus(ConnectionPayload connectionPayload)
        {
            if (Connection.NetworkManager.ConnectedClientsIds.Count >= Connection.MaxConnectedPlayers)
            {
                return ConnectStatus.ServerFull;
            }

            if (connectionPayload.is_debug != Debug.isDebugBuild)
            {
                return ConnectStatus.IncompatibleBuildType;
            }

            return Connection.SessionServiceFacade.IsDuplicateConnection(connectionPayload.player_id)
                ? ConnectStatus.LoggedInAgain
                : ConnectStatus.Success;
        }
    }
}