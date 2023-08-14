namespace Unity.Connection
{
    public interface IConnectionSessionServiceAPI
    {
        string GetPlayerId(ulong clientId);

        bool IsDuplicateConnection(string playerId);

        void StartSession();

        void EndSession();

        void OnServerEnded();

        void DisconnectClient(ulong clientId);
    }
}