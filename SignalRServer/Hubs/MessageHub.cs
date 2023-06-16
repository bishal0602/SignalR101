using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRServer.Contracts;
using System.Runtime.CompilerServices;

namespace SignalRServer.Hubs
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme, Policy = "BasicAuth")]
    public class MessageHub : Hub<IMessageHubClient>
    {
        public async Task BroadcastMessage(string message)
        {
            await Clients.All.RecieveMessage(GetMessageToSend(message));
        }
        public async Task BroadcastStream(IAsyncEnumerable<string> messageStream)
        {
            await foreach (var message in messageStream)
            {
                await Clients.All.RecieveMessage(GetMessageToSend(message));
                await Task.Delay(500); // for visual effects
            }
        }
        //[Authorize("IsAdmin")]
        public async IAsyncEnumerable<string> TriggerStream(int count, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (int i = 1; i <= count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return $"Server - Counting {i}";
                await Task.Delay(500); // for visual effects
            }
        }
        public async Task SendToOthers(string message)
        {
            await Clients.Others.RecieveMessage(GetMessageToSend(message));
        }
        public async Task SendToCaller(string message)
        {
            await Clients.Caller.RecieveMessage(GetMessageToSend(message));
        }
        public async Task SendToSpecificUser(string connectionId, string message)
        {
            await Clients.Client(connectionId).RecieveMessage(GetMessageToSend(message));
        }

        public async Task SendToGroup(string groupName, string message)
        {
            await Clients.Group(groupName).RecieveMessage(GetMessageToSend(message));
        }

        public async Task AddUserToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.RecieveMessage($"You have been added to {groupName} group");
            await Clients.Others.RecieveMessage($"User {Context.ConnectionId} has been added to {groupName} group");
        }
        public async Task RemoveUserFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.RecieveMessage($"You have been removed from {groupName} group");
            await Clients.Others.RecieveMessage($"User {Context.ConnectionId} has been removed from {groupName} group");

        }




        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }


        private string GetMessageToSend(string originalMessage)
        {
            return $"User Connection Id: {Context.ConnectionId}, Message: {originalMessage}";
        }
    }
}
