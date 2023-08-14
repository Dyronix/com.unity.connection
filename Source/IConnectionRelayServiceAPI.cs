using System.Threading.Tasks;
using Unity.Services.Relay.Models;

namespace Unity.Connection
{
    public interface IConnectionRelayServiceAPI
    {
        Task<(bool success, Allocation allocation)> CreateAllocation(int maxConnections, string region = null);
        Task<(bool success, JoinAllocation allocation)> JoinAllocation(string relayServerCode);
        Task<(bool success, string joinCode)> GetJoinCode(Allocation hostAllocation);
    }
}