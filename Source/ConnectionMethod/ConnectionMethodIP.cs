using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;

namespace Unity.Connection.Method
{
    public class ConnectionMethodIP : ConnectionMethodBase
    {
        //--------------------------------------------------------------------------------------
        // Fields
        private string _ip_address;
        private ushort _port;

        //--------------------------------------------------------------------------------------
        public ConnectionMethodIP(string ip, ushort port, Connection connection, string playerName)
            : base(connection, playerName)
        {
            _ip_address = ip;
            _port = port;
        }

        //--------------------------------------------------------------------------------------
        public override Task SetupClientConnectionAsync()
        {
            SetConnectionPayload(GetPlayerId(), PlayerName);

            UnityTransport utp = (UnityTransport)Connection.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetConnectionData(_ip_address, _port);

            return Task.CompletedTask;
        }
        //--------------------------------------------------------------------------------------
        public override Task SetupHostConnectionAsync()
        {
            SetConnectionPayload(GetPlayerId(), PlayerName); // Need to set connection payload for host as well, as host is a client too
            UnityTransport utp = (UnityTransport)Connection.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetConnectionData(_ip_address, _port);

            return Task.CompletedTask;
        }
    }
}