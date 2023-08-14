using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Connection.Method
{
    [Serializable]
    public class ConnectionPayload
    {
        public string player_id;
        public string player_name;

        public bool is_debug;
    }

    /// <summary>
    /// ConnectionMethod contains all setup needed to setup NGO to be ready to start a connection, either host or client side.
    /// Please override this abstract class to add a new transport or way of connecting.
    /// </summary>
    public abstract class ConnectionMethodBase
    {
        //--------------------------------------------------------------------------------------
        // Constants
        private const string _dtls_connection_type = "dtls";

        //--------------------------------------------------------------------------------------
        // Properties
        protected string ConnectionType
        {
            get
            {
                return _dtls_connection_type;
            }
        }

        protected Connection Connection
        {
            get { return _connection; }
        }

        protected string PlayerName
        {
            get
            {
                return _player_name;
            }
        }

        //--------------------------------------------------------------------------------------
        // Fields
        private readonly Connection _connection;
        private readonly string _player_name;

        //--------------------------------------------------------------------------------------
        public ConnectionMethodBase(Connection connection, string playerName)
        {
            _connection = connection;
            _player_name = playerName;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Setup the host connection prior to starting the NetworkManager
        /// </summary>
        /// <returns></returns>
        public abstract Task SetupHostConnectionAsync();

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Setup the client connection prior to starting the NetworkManager
        /// </summary>
        /// <returns></returns>
        public abstract Task SetupClientConnectionAsync();

        //--------------------------------------------------------------------------------------
        protected void SetConnectionPayload(string playerId, string playerName)
        {
            var payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                player_id = playerId,
                player_name = playerName,
                is_debug = Debug.isDebugBuild
            });

            var payload_bytes = System.Text.Encoding.UTF8.GetBytes(payload);

            Connection.NetworkManager.NetworkConfig.ConnectionData = payload_bytes;
        }

        //--------------------------------------------------------------------------------------
        /// Using authentication, this makes sure your session is associated with your account and not your device. This means you could reconnect 
        /// from a different device for example. A playerId is also a bit more permanent than player prefs. In a browser for example, 
        /// player prefs can be cleared as easily as cookies.

        protected string GetPlayerId()
        {
            return Connection.AuthenticationServiceFacade.PlayerId;
        }
    }
}