using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;

namespace Unity.Connection
{
    public interface IConnectionLobbyServiceAPI
    {
        Lobby RemoteLobby { get; }

        string RelayCode { get; }

        void BeginTrackingActiveLobby();
        void StopTrackingActiveLobby();

        Task<(bool Success, Lobby Lobby)> UpdateLobbyRelayCode(string relayCode);
        Task<(bool Success, Lobby Lobby)> UpdatePlayerRelayInfo(string allocationId, string relayCode);

        Task KickPlayer(string playerId);
        Task DeleteLobby();
    }
}