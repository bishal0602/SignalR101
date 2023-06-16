namespace SignalRServer.Contracts
{
    public interface IMessageHubClient
    {
        Task RecieveMessage(string message);
    }
}
