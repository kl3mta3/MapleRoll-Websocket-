using MapleRoll2.Connect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MapleRoll2.Net
{
    public class WebSocketClient
    {
        internal ClientWebSocket _clientSocket;
        private Uri _serverUri;
        //internal MainWindow _mainWindow;
        public event Action<string> OnMessageReceived;
        public event Action OnConnected;
        public event Action OnDisconnected;

        public WebSocketClient(string serverUrl)
        {
            _clientSocket = new ClientWebSocket();
            //_mainWindow = new MainWindow();
            _serverUri = new Uri(serverUrl);
        }

      

        public async Task DisconnectFromServer()
        {
            if (_clientSocket != null && _clientSocket.State == WebSocketState.Open)
            {
                 await NotifyServerBeforeDisconnect(MapleRollConnect.groupId, MapleRollConnect.uId);
                Console.WriteLine($"MapleRollConnect.groupId:{MapleRollConnect.groupId}, MapleRollConnect.uId:{MapleRollConnect.uId}");

                await _clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnecting", CancellationToken.None);
                Console.WriteLine("Disconnected from server.");
            }
        }

        public async Task NotifyServerBeforeDisconnect(string groupId, string userId)
        {
            if (_clientSocket != null && _clientSocket.State == WebSocketState.Open)
            {
                var message = new
                {
                    Type = "disconnect",
                    GroupID = groupId,
                    UID = userId,
                    
                };
                //string jsonMessage = JsonSerializer.Serialize(message);
                await SendMessageAsync(message);
                Console.WriteLine($"Notify server for Disconnect Message:{message}.");
            }
        }


        public async Task ConnectAsync()
        {
            Console.WriteLine("Attempting to connect to webserver");
            try
            {
                await _clientSocket.ConnectAsync(_serverUri, CancellationToken.None);
                OnConnected?.Invoke();
                Console.WriteLine("[{DateTime.Now}]: Connected to WebSocket server.");
                _ = Task.Run(ReceiveMessagesAsync);
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine($"[{DateTime.Now}]: Failed to start. Reason: {ex.Message}. Ensure the application is running with sufficient privileges.", 20);
               // _mainWindow.SendMessageToConsole($"[{DateTime.Now}]: Failed to start. Reason: {ex.Message}. Ensure the application is running with sufficient privileges.", 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}]:Failed to connect: {ex.Message}");
               // _mainWindow.SendMessageToConsole($"[{DateTime.Now}]: Failed to start. Reason: {ex.Message}.", 1);
            }
        }

        public async Task SendMessageAsync(object message)
        {
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);

            if (_clientSocket.State == WebSocketState.Open)
            {
                await _clientSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                Console.WriteLine($"Sent Message Json: {json}");
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[4096];
            while (_clientSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await _clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        OnDisconnected?.Invoke();
                        Console.WriteLine("Connection closed.");
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    OnMessageReceived?.Invoke(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving message: {ex.Message}");
                    break;
                }
            }
        }
    }
}
