using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketsSample.Controllers
{
    public class WebSocketChatManager
    {
        private ConcurrentDictionary<WebSocket, string> _sockets = new ConcurrentDictionary<WebSocket, string>();

        public async Task AddWebSocket(WebSocket webSocket, string username)
        {
            _sockets.TryAdd(webSocket, username);
            await SendMessageToAll($"{username} joined the chat");
        }

        public async Task RemoveWebSocket(WebSocket webSocket)
        {
            if (_sockets.TryRemove(webSocket, out var username))
            {
                await SendMessageToAll($"{username} left the chat");
            }
        }

        public async Task SendMessageToAll(string message)
        {
            foreach (var socket in _sockets.Keys)
            {
                if (socket.State == WebSocketState.Open)
                {
                    var byteMessage = Encoding.UTF8.GetBytes(message);
                    await socket.SendAsync(new ArraySegment<byte>(byteMessage, 0, byteMessage.Length), WebSocketMessageType.Text, true, default);
                }
            }
        }
    }
}
