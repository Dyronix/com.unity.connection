using Unity.Netcode;
using UnityEngine.Events;

namespace Unity.Connection.State
{
    public enum ConnectionStatusType
    {
        UNDEFINED,
        SUCCESS,                  //client successfully connected. This may also be a successful reconnect.
        SERVERFULL,               //can't join, server is already at capacity.
        LOGGEDINAGAIN,            //logged in on a separate client, causing this one to be kicked out.
        USERREQUESTEDDISCONNECT,  //Intentional Disconnect triggered by the user.
        GENERICDISCONNECT,        //server disconnected, but no specific reason given.
        INCOMPATIBLEBUILDTYPE,    //client build type is incompatible with server.
        HOSTENDEDSESSION,         //host intentionally ended the session.
        STARTHOSTFAILED,          // server failed to bind
        STARTCLIENTFAILED         // failed to connect to server and/or invalid network endpoint
    }

    public enum ConnectionStateType
    {
        CLIENTCONNECTED,
        CLIENTCONNECTING,
        HOSTING,
        STARTHOSTING,
        OFFLINE
    }

    public abstract class ConnectionState
    {
        //--------------------------------------------------------------------------------------
        // Events
        public class ConnectionStatusChangedUnityEvent : UnityEvent<ConnectionStatusType> { };
        public class ClientConnectedUnityEvent : UnityEvent<ConnectionStatusType, ulong> { };
        public class ClientDisconnectedUnityEvent : UnityEvent<ConnectionStatusType, ulong> { };

        public readonly ConnectionStatusChangedUnityEvent OnConnectionStatusChanged = new ConnectionStatusChangedUnityEvent();
        public readonly ClientConnectedUnityEvent OnClientConnectedToHost = new ClientConnectedUnityEvent();
        public readonly ClientDisconnectedUnityEvent OnClientDisconnectedFromHost = new ClientDisconnectedUnityEvent();

        //--------------------------------------------------------------------------------------
        // Properties
        public abstract ConnectionStateType Type { get; }

        public Connection Connection { get; private set; }

        //--------------------------------------------------------------------------------------
        public ConnectionState(Connection connection)
        {
            Connection = connection;
        }

        public abstract void Enter();

        public abstract void Exit();

        public virtual void OnClientConnected(ulong clientId) { }

        public virtual void OnClientDisconnect(ulong clientId) { }

        public virtual void OnServerStarted() { }

        public virtual void OnServerStopped() { }

        public virtual void StartClientLobby(string playerName) { }

        public virtual void StartHostLobby(string playerName) { }

        public virtual void OnUserRequestedShutdown() { }

        public virtual void OnTransportFailure() { }

        public virtual void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) { }
    }
}