using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

Console.WriteLine("Enter your access token: ");
string? accessToken = Console.ReadLine();

string url = "https://localhost:7056/messageHub";
HubConnection hubConnection = new HubConnectionBuilder()
    .WithUrl(url, options =>
    {
        options.AccessTokenProvider = () => Task.FromResult(accessToken);
    })
    .ConfigureLogging(logging =>
    {
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .WithAutomaticReconnect()
    .Build();


hubConnection.On<string>("RecieveMessage", (message) =>
{
    Console.WriteLine($">> {message}");
});

try
{
    await hubConnection.StartAsync();
    while (true)
    {
        Console.WriteLine("Please specify the action: ");
        Console.WriteLine("0 - Broadcast message");
        Console.WriteLine("1 - Send Others");
        Console.WriteLine("2 - Send Self");
        Console.WriteLine("3 - Send To Specific User");
        Console.WriteLine("4 - Join Group");
        Console.WriteLine("5 - Leave Group");
        Console.WriteLine("6 - Send To Group");
        Console.WriteLine("7 - Trigger Stream");
        Console.WriteLine("exit - Exit the program");
        string? action = Console.ReadLine();
        if (action == "exit")
            break;

        string? groupName = string.Empty;
        string? message = string.Empty;

        if (action == "4" || action == "5" || action == "6")
        {
            Console.WriteLine("Enter group name: ");
            groupName = Console.ReadLine();
        }
        if (action != "4" && action != "5" && action != "7")
        {
            Console.WriteLine("Enter message: ");
            message = Console.ReadLine();
        }

        switch (action)
        {
            case "0":
                if (message.Contains(';'))
                {
                    var channel = Channel.CreateBounded<string>(10);
                    await hubConnection.SendAsync("BroadcastStream", channel.Reader);
                    foreach (var msg in message.Split(';'))
                    {
                        await channel.Writer.WriteAsync(msg);
                    }
                    channel.Writer.Complete();
                }
                else
                {
                    await hubConnection.SendAsync("BroadcastMessage", message);
                }
                break;
            case "1":
                await hubConnection.SendAsync("SendToOthers", message);
                break;
            case "2":
                await hubConnection.SendAsync("SendToCaller", message);
                break;
            case "3":
                Console.WriteLine("Enter Connection Id: ");
                var connectionId = Console.ReadLine();
                await hubConnection.SendAsync("SendToSpecificUser", connectionId, message);
                break;
            case "4":
                await hubConnection.SendAsync("AddUserToGroup", groupName);
                break;
            case "5":
                await hubConnection.SendAsync("RemoveUserFromGroup", groupName);
                break;
            case "6":
                await hubConnection.SendAsync("SendToGroup", groupName, message);
                break;
            case "7":
                Console.WriteLine("Enter the count: ");
                int count = int.Parse(Console.ReadLine() ?? "0");

                var cancellationTokenSource = new CancellationTokenSource();
                var stream = hubConnection.StreamAsync<string>("TriggerStream", count, cancellationTokenSource.Token);

                await foreach (var msg in stream)
                {
                    Console.WriteLine(msg);
                }
                break;
            default:
                Console.WriteLine("Invalid action!");
                break;
        }

    }

}
catch (Exception ex)
{
    Console.WriteLine("Something went wrong!");
    Console.WriteLine(ex.Message);
    Console.ReadKey();
    return;
}