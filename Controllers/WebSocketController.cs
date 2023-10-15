using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebSocketsSample.Controllers
{
    public class WebSocketController : ControllerBase
    {
        private static WebSocketChatManager _chatManager = new WebSocketChatManager();

        [Route("/ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var username = await ReadUsername(webSocket);

                if (username != null)
                {
                    await _chatManager.AddWebSocket(webSocket, username);
                    await Echo(webSocket, username);
                    await _chatManager.RemoveWebSocket(webSocket);
                }
                else
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Invalid username", CancellationToken.None);
                }
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private static async Task Echo(WebSocket webSocket, string username)
        {
            var buffer = new byte[1024 * 4];

            while (true)
            {
                var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                    await _chatManager.SendMessageToAll($"{username}: {message}");
                }
                else if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    break;
                }
            }
        }

        private static async Task<string> ReadUsername(WebSocket webSocket)
        {
            var buffer = new byte[1024];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var username = Encoding.UTF8.GetString(buffer, 0, result.Count);
            return !string.IsNullOrWhiteSpace(username) ? username.Trim() : string.Empty;
        }
    }
}