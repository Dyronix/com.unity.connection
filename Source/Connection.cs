using Unity.Connection.State;
using Unity.Netcode;
using UnityEngine.Events;

namespace Unity.Connection
{
    public class Connection
    {
        public class ConnectionStateChangedUnityEvent : UnityEvent<ConnectionState, ConnectionState> { };
        public class ConnectionStatusChangedUnityEvent : UnityEvent<ConnectionStatusType> { };
        public class ConnectionApprovedUnityEvent : UnityEvent<ConnectionStateType, NetworkManager.ConnectionApprovalRequest, NetworkManager.ConnectionApprovalResponse> { }
        public class ConnectionDisapprovedUnityEvent : UnityEvent<ConnectionStateType, NetworkManager.ConnectionApprovalRequest, NetworkManager.ConnectionApprovalResponse> { }
        public class ClientConnectedUnityEvent : UnityEvent<ConnectionStatusType, ulong> { };
        public class ClientDisconnectedUnityEvent : UnityEvent<ConnectionStatusType, ulong> { };

        public ConnectionStateChangedUnityEvent OnConnectionStateChanged;
        public ConnectionStatusChangedUnityEvent OnConnectionStatusChanged;
        public ConnectionApprovedUnityEvent OnConnectionApproved;
        public ConnectionDisapprovedUnityEvent OnConnectionDisapproved;
        public ClientConnectedUnityEvent OnClientConnectedToHost;
        public ClientDisconnectedUnityEvent OnClientDisconnectedFromHost;

        //--------------------------------------------------------------------------------------
        // Properties
        public int MaxConnectedPlayers
        {
            get { return _max_connected_players; }
        }

        public NetworkManager NetworkManager
        {
            get { return _network_manager; }
        }

        public IConnectionLobbyServiceAPI LobbyServiceFacade
        {
            get { return _lobby_service; }
        }

        public IConnectionRelayServiceAPI RelayServiceFacade
        {
            get { return _relay_service; }
        }

        public IConnectionAuthenticationServiceAPI AuthenticationServiceFacade
        {
            get { return _authentication_service; }
        }

        public IConnectionSessionServiceAPI SessionServiceFacade
        {
            get { return _session_service; }
        }

        public ConnectionStatusType ConnectionStatus
        {
            get { return _connection_status; }
        }

        //--------------------------------------------------------------------------------------
        // Fields
        private int _max_connected_players;

        private NetworkManager _network_manager;

        private IConnectionLobbyServiceAPI _lobby_service;
        private IConnectionRelayServiceAPI _relay_service;
        private IConnectionAuthenticationServiceAPI _authentication_service;
        private IConnectionSessionServiceAPI _session_service;

        private ConnectionStatusType _connection_status;

        private ConnectionFSM _connection_fsm;

        //--------------------------------------------------------------------------------------
        public Connection(int maxConnectedPlayers, NetworkManager networkManager, IConnectionLobbyServiceAPI lobbyServiceAPI, IConnectionRelayServiceAPI relayServiceAPI, IConnectionAuthenticationServiceAPI authenticationServiceAPI, IConnectionSessionServiceAPI sessionServiceAPI)
        {
            _max_connected_players = maxConnectedPlayers;

            _network_manager = networkManager;

            _lobby_service = lobbyServiceAPI;
            _relay_service = relayServiceAPI;
            _authentication_service = authenticationServiceAPI;
            _session_service = sessionServiceAPI;

            _connection_status = ConnectionStatusType.UNDEFINED;

            _connection_fsm = new ConnectionFSM(new OfflineState(this), networkManager);
            _connection_fsm.OnConnectionStatusChanged.AddListener(ConnectionFSM_ConnectionStatusChanged);
            _connection_fsm.OnConnectionStateChanged.AddListener(ConnectionFSM_ConnectionStateChanged);
            _connection_fsm.OnConnectionApproved.AddListener(ConnectionFSM_ConnectionApproved);
            _connection_fsm.OnConnectionDisapproved.AddListener(ConnectionFSM_ConnectionDisapproved);
            _connection_fsm.OnClientConnectedToHost.AddListener(ConnectionFSM_ClientConnectedToHost);
            _connection_fsm.OnClientDisconnectedFromHost.AddListener(ConnectionFSM_ClientDisconnectedFromHost);
        }

        //--------------------------------------------------------------------------------------
        public void ChangeConnectionState(ConnectionState newState)
        {
            _connection_fsm.ChangeState(newState);
        }

        //--------------------------------------------------------------------------------------
        public void StartClientLobby(string playerName)
        {
            _connection_fsm.StartClientLobby(playerName);
        }

        //--------------------------------------------------------------------------------------
        public void StartHostLobby(string playerName)
        {
            _connection_fsm.StartHostLobby(playerName);
        }

        //--------------------------------------------------------------------------------------
        public void RequestShutdown()
        {
            _connection_fsm.RequestShutdown();
        }

        //--------------------------------------------------------------------------------------
        private void ConnectionFSM_ConnectionStateChanged(ConnectionState previous, ConnectionState current)
        {
            OnConnectionStateChanged?.Invoke(previous, current);
        }
        //--------------------------------------------------------------------------------------
        private void ConnectionFSM_ConnectionStatusChanged(ConnectionStatusType statusType)
        {
            _connection_status = statusType;

            OnConnectionStatusChanged?.Invoke(statusType);
        }
        //--------------------------------------------------------------------------------------
        private void ConnectionFSM_ConnectionApproved(ConnectionStateType connectionStateType, NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse respone)
        {
            OnConnectionApproved?.Invoke(connectionStateType, request, respone);
        }
        //--------------------------------------------------------------------------------------
        private void ConnectionFSM_ConnectionDisapproved(ConnectionStateType connectionStateType, NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            OnConnectionDisapproved?.Invoke(connectionStateType, request, response);
        }
        //--------------------------------------------------------------------------------------
        private void ConnectionFSM_ClientConnectedToHost(ConnectionStatusType connectionStatus, ulong clientId) 
        {
            OnClientConnectedToHost?.Invoke(connectionStatus, clientId);
        }
        //--------------------------------------------------------------------------------------
        private void ConnectionFSM_ClientDisconnectedFromHost(ConnectionStatusType connectionStatus, ulong clientId)
        {
            OnClientDisconnectedFromHost?.Invoke(connectionStatus, clientId);
        }
    }
}