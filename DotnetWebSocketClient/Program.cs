using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

namespace DotnetWebSocketClient
{
    class Program
    {
        static ClientWebSocket clientWebSocket;

        static async Task Main()
        {
            clientWebSocket = new ClientWebSocket();

            var uri = new Uri("wss://localhost:7056/messageHub");

            await clientWebSocket.ConnectAsync(uri, CancellationToken.None);
            Console.WriteLine("Web socket connection established!");

            var handshake = new List<byte>(Encoding.UTF8.GetBytes(@"{""protocol"":""json"", ""version"":1}"))
                {
                    0x1e // delimeter
                };
            await clientWebSocket.SendAsync(new ArraySegment<byte>(handshake.ToArray()), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine("Handshake successfull!");

            _ = ReceiveMessages();

            Console.ReadLine();

            await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client exited", CancellationToken.None);
        }

        static async Task ReceiveMessages()
        {
            var buffer = new byte[1024];
            var receiveBuffer = new ArraySegment<byte>(buffer);

            while (clientWebSocket.State == WebSocketState.Open)
            {
                var result = await clientWebSocket.ReceiveAsync(receiveBuffer, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    //Console.WriteLine(receivedMessage);
                    try
                    {
                        string messageToParse = receivedMessage.TrimEnd('\u001e');

                        var parsedMessage = JsonConvert.DeserializeObject<dynamic>(messageToParse);
                        if (parsedMessage != null && parsedMessage?.type == 1)
                        {
                            string message = parsedMessage?.arguments[0];

                            Console.WriteLine(">> " + message);
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("unable to parse");

                    }
                }
            }
        }
    }
}