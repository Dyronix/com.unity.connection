using TFR.Channels.Local;
using TFR.Channels.Messages;
using TFR.Services.Authentication;
using TFR.Services.Lobbies;
using Unity.Connection.Method;
using Unity.Connection.State;
using TFR.Services.Session;
using Unity.Netcode;
using Unity.Sessions;
using UnityEngine;

namespace Unity.Connection
{
    public class ConnectionFacade 
    {
        //--------------------------------------------------------------------------------------
        // Properties
        public static ConnectionFacade Instance 
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new ConnectionFacade(UnBufferedMessageChannel.Instance);
                }

                return _instance;
            }
        }

        //--------------------------------------------------------------------------------------
        // Fields
        private static ConnectionFacade _instance;

        private Connection _connection;
        private MessageChannel _message_channel;

        //--------------------------------------------------------------------------------------
        public ConnectionFacade(MessageChannel messageChannel)
        {
            _message_channel = messageChannel;
            _connection = new Connection(ConnectionFacadeData.MaxConnections
                , NetworkManager.Singleton
                , LobbyServiceFacade.Instance
                , RelayServiceFacade.Instance
                , AuthenticationServiceFacade.Instance
                , SessionServiceFacade.Instance);

            _connection.OnConnectionApproved.AddListener(Connection_OnConnectionApproved);

            _connection.OnClientConnectedToHost.AddListener(Connection_OnClientConnectedToHost);
            _connection.OnClientDisconnectedFromHost.AddListener(Connection__OnClientDisconnedtedFromHost);
        }

        //--------------------------------------------------------------------------------------
        private void Connection_OnConnectionApproved(ConnectionStateType stateType, NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            switch(stateType)
            {
                case ConnectionStateType.STARTHOSTING:
                    StartingHostConnectionApproved(request, response);
                    break;
                case ConnectionStateType.HOSTING:
                    HostingConnectionApproved(request, response);
                    break;
            }
        }
        //--------------------------------------------------------------------------------------
        private void Connection_OnClientConnectedToHost(ConnectionStatusType statusType, ulong clientId)
        {
            var player_id = SessionServiceFacade.Instance.GetPlayerId(clientId);
            if (player_id != null)
            {
                var session_data = SessionServiceFacade.Instance.GetPlayerData(player_id);
                if (session_data.HasValue)
                {
                    _message_channel.Publish(new ConnectionEventMessage(statusType, session_data.Value.PlayerName));
                }
            }
        }
        //--------------------------------------------------------------------------------------
        private void Connection__OnClientDisconnedtedFromHost(ConnectionStatusType statusType, ulong clientId)
        {
            var player_id = SessionServiceFacade.Instance.GetPlayerId(clientId);
            if (player_id != null)
            {
                var session_data = SessionServiceFacade.Instance.GetPlayerData(player_id);
                if (session_data.HasValue)
                {
                    _message_channel.Publish(new ConnectionEventMessage(statusType, session_data.Value.PlayerName));
                }
            }
        }
        //--------------------------------------------------------------------------------------
        private void StartingHostConnectionApproved(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var connection_data = request.Payload;
            var client_id = request.ClientNetworkId;

            // This happens when starting as a host, before the end of the StartHost call. In that case, we simply approve ourselves.
            if (client_id == _connection.NetworkManager.LocalClientId)
            {
                var payload = System.Text.Encoding.UTF8.GetString(connection_data);
                var connection_payload = JsonUtility.FromJson<ConnectionPayload>(payload);

                SessionServiceFacade.Instance.SetupConnectingPlayerSessionData(client_id, connection_payload.player_id, new SessionPlayerData(client_id, connection_payload.player_name, true));
            }
        }
        //--------------------------------------------------------------------------------------
        private void HostingConnectionApproved(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var connection_data = request.Payload;
            var client_id = request.ClientNetworkId;

            var payload = System.Text.Encoding.UTF8.GetString(connection_data);
            var connection_payload = JsonUtility.FromJson<ConnectionPayload>(payload);

            SessionServiceFacade.Instance.SetupConnectingPlayerSessionData(client_id, connection_payload.player_id, new SessionPlayerData(client_id, connection_payload.player_name, true));
        }
    }
}