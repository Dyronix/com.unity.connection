using Unity.Connection.State;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.Connection
{
    public class ConnectionFSM
    {
        //--------------------------------------------------------------------------------------
        // Events
        public class ConnectionStateChangedUnityEvent : UnityEvent<ConnectionState, ConnectionState> { };
        public class ConnectionStatusChangedUnityEvent : UnityEvent<ConnectionStatusType> { };
        public class ConnectionApprovedUnityEvent : UnityEvent<ConnectionStateType, NetworkManager.ConnectionApprovalRequest, NetworkManager.ConnectionApprovalResponse> { }
        public class ConnectionDisapprovedUnityEvent : UnityEvent<ConnectionStateType, NetworkManager.ConnectionApprovalRequest, NetworkManager.ConnectionApprovalResponse> { }
        public class ClientConnectedToHostUnityEvent : UnityEvent<ConnectionStatusType, ulong> { };
        public class ClientDisconnectedFromHostUnityEvent : UnityEvent<ConnectionStatusType, ulong> { };

        public ConnectionStateChangedUnityEvent OnConnectionStateChanged;
        public ConnectionStatusChangedUnityEvent OnConnectionStatusChanged;
        public ConnectionApprovedUnityEvent OnConnectionApproved;
        public ConnectionDisapprovedUnityEvent OnConnectionDisapproved;
        public ClientConnectedToHostUnityEvent OnClientConnectedToHost;
        public ClientDisconnectedFromHostUnityEvent OnClientDisconnectedFromHost;

        //--------------------------------------------------------------------------------------
        // Fields
        private ConnectionState _current_state;
        private NetworkManager _network_manager;

        public ConnectionFSM(ConnectionState initialState, NetworkManager networkManager)
        {
            _current_state = initialState;
            SubscribeToClientConnectionEvents(_current_state);

            _network_manager = networkManager;

            _network_manager.OnClientConnectedCallback += Network_OnClientConnectedCallback;
            _network_manager.OnClientDisconnectCallback += Network_OnClientDisconnectCallback;
            _network_manager.OnServerStarted += Network_OnServerStarted;
            _network_manager.OnServerStopped += Network_OnServerStopped;
            _network_manager.ConnectionApprovalCallback += Network_ApprovalCheck;
            _network_manager.OnTransportFailure += Network_OnTransportFailure;
        }

        ~ConnectionFSM()
        {
            _network_manager.OnClientConnectedCallback -= Network_OnClientConnectedCallback;
            _network_manager.OnClientDisconnectCallback -= Network_OnClientDisconnectCallback;
            _network_manager.OnServerStarted -= Network_OnServerStarted;
            _network_manager.OnServerStopped -= Network_OnServerStopped;
            _network_manager.ConnectionApprovalCallback -= Network_ApprovalCheck;
            _network_manager.OnTransportFailure -= Network_OnTransportFailure;
        }

        //--------------------------------------------------------------------------------------
        public void ChangeState(ConnectionState state)
        {
            if (_current_state == state)
            {
                return;
            }

            ConnectionState previous_state = null;
            if (_current_state != null)
            {
                _current_state.Exit();

                UnsubscribeClientConnectionEvents(_current_state);
                
                previous_state = _current_state;
            }

            _current_state = state;
            _current_state.Enter();
            
            SubscribeToClientConnectionEvents(_current_state);

            OnConnectionStateChanged?.Invoke(previous_state, _current_state);

            LogConnectionStateChanged(previous_state, _current_state);
        }

        //--------------------------------------------------------------------------------------
        public void StartClientLobby(string playerName)
        {
            _current_state.StartClientLobby(playerName);
        }

        //--------------------------------------------------------------------------------------
        public void StartHostLobby(string playerName)
        {
            _current_state.StartHostLobby(playerName);
        }

        //--------------------------------------------------------------------------------------
        public void RequestShutdown()
        {
            _current_state.OnUserRequestedShutdown();
        }

        //--------------------------------------------------------------------------------------
        private void LogConnectionStateChanged(ConnectionState previous, ConnectionState current)
        {
            string message = "Changed state from ";

            message += "\"";
            message += previous != null
                ? previous.GetType().Name
                : "OfflineState";
            message += "\"";

            message += " to ";
            
            message += "\"";
            message += current.GetType().Name;
            message += "\"";

            Debug.Log(message);
        }

        //--------------------------------------------------------------------------------------
        private void Network_OnClientDisconnectCallback(ulong clientId)
        {
            _current_state.OnClientDisconnect(clientId);
        }

        //--------------------------------------------------------------------------------------
        private void Network_OnClientConnectedCallback(ulong clientId)
        {
            _current_state.OnClientConnected(clientId);
        }

        //--------------------------------------------------------------------------------------
        private void Network_OnServerStarted()
        {
            _current_state.OnServerStarted();
        }

        //--------------------------------------------------------------------------------------
        private void Network_OnServerStopped(bool _) // we don't need this parameter as the ConnectionState already carries the relevant information
        {
            _current_state.OnServerStopped();
        }

        //--------------------------------------------------------------------------------------
        private void Network_ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            _current_state.ApprovalCheck(request, response);

            if(response.Approved)
            {
                OnConnectionApproved?.Invoke(_current_state.Type, request, response);
            }
            else
            {
                OnConnectionDisapproved?.Invoke(_current_state.Type, request, response);
            }
        }

        //--------------------------------------------------------------------------------------
        private void Network_OnTransportFailure()
        {
            _current_state.OnTransportFailure();
        }

        //--------------------------------------------------------------------------------------
        private void ConnectionState_OnClientConnectedToHost(ConnectionStatusType statusType, ulong clientId)
        {
            OnClientConnectedToHost?.Invoke(statusType, clientId);
        }

        //--------------------------------------------------------------------------------------
        private void ConnectionState_OnClientDisconnectedFromHost(ConnectionStatusType statusType, ulong clientId)
        {
            OnClientDisconnectedFromHost?.Invoke(statusType, clientId);
        }

        //--------------------------------------------------------------------------------------
        private void ConnectionState_OnConnectionStatusChanged(ConnectionStatusType statusType)
        {
            OnConnectionStatusChanged?.Invoke(statusType);
        }

        //--------------------------------------------------------------------------------------
        private void SubscribeToClientConnectionEvents(ConnectionState state)
        {
            state.OnClientConnectedToHost.AddListener(ConnectionState_OnClientConnectedToHost);
            state.OnClientDisconnectedFromHost.AddListener(ConnectionState_OnClientDisconnectedFromHost);
            state.OnConnectionStatusChanged.AddListener(ConnectionState_OnConnectionStatusChanged);
        }

        //--------------------------------------------------------------------------------------
        private void UnsubscribeClientConnectionEvents(ConnectionState state)
        {
            state.OnClientConnectedToHost.RemoveListener(ConnectionState_OnClientConnectedToHost);
            state.OnClientDisconnectedFromHost.RemoveListener(ConnectionState_OnClientDisconnectedFromHost);
            state.OnConnectionStatusChanged.RemoveListener(ConnectionState_OnConnectionStatusChanged);
        }
    }
}