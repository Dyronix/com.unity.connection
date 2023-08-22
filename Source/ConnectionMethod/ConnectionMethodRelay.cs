using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.Connection.Method
{
    /// <summary>
    /// UTP's Relay connection setup using the Lobby integration
    /// </summary>
    public class ConnectionMethodRelay : ConnectionMethodBase
    {
        public class ClientConnectionCompleted : UnityEvent<JoinAllocation> { }
        public class ClientConnectionFailed : UnityEvent { }
        public class HostConnectionCompleted : UnityEvent<Allocation> { }
        public class HostConnectionFailed : UnityEvent { }

        public readonly ClientConnectionCompleted OnClientConnectionCompleted = new ClientConnectionCompleted();
        public readonly ClientConnectionFailed OnClientConnectionFailed = new ClientConnectionFailed();
        public readonly HostConnectionCompleted OnHostConnectionCompleted = new HostConnectionCompleted();
        public readonly HostConnectionFailed OnHostConnectionFailed = new HostConnectionFailed();

        //--------------------------------------------------------------------------------------
        public ConnectionMethodRelay(Connection connection, string playerName)
            : base(connection, playerName)
        { }

        //--------------------------------------------------------------------------------------
        public override async Task SetupClientConnectionAsync()
        {
            Debug.Log("Setting up Unity Relay client");

            SetConnectionPayload(GetPlayerId(), PlayerName);

            if (Connection.LobbyServiceFacade.RemoteLobby == null)
            {
                Debug.LogError("Trying to start relay while Lobby isn't set");
                OnClientConnectionFailed?.Invoke();

                return;
            }

            Debug.Log($"Setting Unity Relay client with join code {Connection.LobbyServiceFacade.RelayCode}");

            // Create client joining allocation from join code
            var result = await JoinAllocationWithRelayCode(Connection.LobbyServiceFacade.RelayCode);
            if (result.success)
            {
                Debug.Log
                    (
                        $"client: {result.allocation.ConnectionData[0]} {result.allocation.ConnectionData[1]}, " +
                        $"host: {result.allocation.HostConnectionData[0]} {result.allocation.HostConnectionData[1]}, " +
                        $"client: {result.allocation.AllocationId}"
                    );

                await UpdatePlayerRelayInfo(result.allocation.AllocationId.ToString(), Connection.LobbyServiceFacade.RelayCode);

                // Configure UTP with allocation
                UnityTransport utp = (UnityTransport)Connection.NetworkManager.NetworkConfig.NetworkTransport;
                utp.SetRelayServerData(new RelayServerData(result.allocation, ConnectionType));

                OnClientConnectionCompleted?.Invoke(result.allocation);
            }
            else
            {
                Debug.LogError("Client connection failed");
                OnClientConnectionFailed?.Invoke();
            }
        }

        //--------------------------------------------------------------------------------------
        public override async Task SetupHostConnectionAsync()
        {
            Debug.Log("Setting up Unity Relay host");

            SetConnectionPayload(GetPlayerId(), PlayerName); // Need to set connection payload for host as well, as host is a client too

            // Create relay allocation
            var result_create_allocation = await CreateRelayAllocation(Connection.MaxConnectedPlayers);
            if(result_create_allocation.success == false)
            {
                Debug.LogError("Host connection failed when creating relay allocation");
                OnHostConnectionFailed?.Invoke();
                return;
            }
            var result_join_code = await GetRelayCodeFromRelayAllocation(result_create_allocation.allocation);
            if(result_join_code.success == false)
            {
                Debug.LogError("Host connection failed when retrieving relay code");
                OnHostConnectionFailed?.Invoke();
                return;
            }

            Debug.Log
                (
                    $"server: connection data: {result_create_allocation.allocation.ConnectionData[0]} {result_create_allocation.allocation.ConnectionData[1]}, " +
                    $"allocation ID:{result_create_allocation.allocation.AllocationId}, region:{result_create_allocation.allocation.Region}"
                );

            //next line enable lobby and relay services integration
            await UpdateLobbyRelayCode(result_join_code.relayCode);
            await UpdatePlayerRelayInfo(result_create_allocation.allocation.AllocationIdBytes.ToString(), result_join_code.relayCode);

            // Setup UTP with relay connection info
            UnityTransport utp = (UnityTransport)Connection.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetRelayServerData(new RelayServerData(result_create_allocation.allocation, ConnectionType)); // This is with DTLS enabled for a secure connection

            OnHostConnectionCompleted?.Invoke(result_create_allocation.allocation);
        }

        //--------------------------------------------------------------------------------------
        private async Task<(bool success, JoinAllocation allocation)> JoinAllocationWithRelayCode(string relayCode)
        {
            return await Connection.RelayServiceFacade.JoinAllocation(relayCode);
        }
        //--------------------------------------------------------------------------------------
        private async Task<(bool success, Allocation allocation)> CreateRelayAllocation(int maxConnectedPlayers)
        {
            return await Connection.RelayServiceFacade.CreateAllocation(maxConnectedPlayers);
        }
        //--------------------------------------------------------------------------------------
        private async Task<(bool success, string relayCode)> GetRelayCodeFromRelayAllocation(Allocation allocation)
        {
            return await Connection.RelayServiceFacade.GetJoinCode(allocation);
        }

        //--------------------------------------------------------------------------------------
        private async Task UpdateLobbyRelayCode(string relayCode)
        {
            await Connection.LobbyServiceFacade.UpdateLobbyRelayCode(relayCode);
        }
        //--------------------------------------------------------------------------------------
        private async Task UpdatePlayerRelayInfo(string allocationId, string relayCode)
        {
            await Connection.LobbyServiceFacade.UpdatePlayerRelayInfo(allocationId, relayCode);
        }
    }
}