using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FuncHandle
{
    public class LanGameServer
    {
        private TcpListener server;
        private List<TcpClient> clients = new List<TcpClient>();
        private Dictionary<TcpClient, string> clientNames = new Dictionary<TcpClient, string>();
        private Dictionary<string, int> playerScores = new Dictionary<string, int>();
        private bool isRunning = false;
        private Thread listenThread;
        
        public event Action<string> OnMessageReceived;
        public event Action<string> OnPlayerConnected;
        public event Action<string> OnPlayerDisconnected;
        public event Action<string, int> OnScoreUpdated;

        public int Port { get; private set; }
        public int PlayerCount => clients.Count;

        public LanGameServer(int port = 5000)
        {
            Port = port;
        }

        // Khởi động server
        public void Start()
        {
            if (isRunning) return;

            try
            {
                server = new TcpListener(IPAddress.Any, Port);
                server.Start();
                isRunning = true;

                listenThread = new Thread(ListenForClients);
                listenThread.IsBackground = true;
                listenThread.Start();

                OnMessageReceived?.Invoke($"Server started on port {Port}");
            }
            catch (Exception ex)
            {
                OnMessageReceived?.Invoke($"Error starting server: {ex.Message}");
            }
        }

        // Dừng server
        public void Stop()
        {
            isRunning = false;
            
            foreach (var client in clients)
            {
                try
                {
                    client.Close();
                }
                catch { }
            }
            clients.Clear();
            clientNames.Clear();
            playerScores.Clear();

            server?.Stop();
            OnMessageReceived?.Invoke("Server stopped");
        }

        // Lắng nghe client kết nối
        private void ListenForClients()
        {
            while (isRunning)
            {
                try
                {
                    TcpClient client = server.AcceptTcpClient();
                    clients.Add(client);

                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                }
                catch (Exception ex)
                {
                    if (isRunning)
                    {
                        OnMessageReceived?.Invoke($"Error accepting client: {ex.Message}");
                    }
                }
            }
        }

        // Xử lý client
        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (isRunning && client.Connected)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ProcessMessage(client, message);
                }
            }
            catch (Exception ex)
            {
                OnMessageReceived?.Invoke($"Client error: {ex.Message}");
            }
            finally
            {
                DisconnectClient(client);
            }
        }

        // Xử lý tin nhắn từ client
        private void ProcessMessage(TcpClient client, string message)
        {
            string[] parts = message.Split('|');
            if (parts.Length < 2) return;

            string command = parts[0];
            string data = parts[1];

            switch (command)
            {
                case "JOIN":
                    // Client tham gia: JOIN|PlayerName
                    string playerName = data;
                    clientNames[client] = playerName;
                    playerScores[playerName] = 0;
                    OnPlayerConnected?.Invoke(playerName);
                    BroadcastMessage($"PLAYER_JOINED|{playerName}");
                    break;

                case "SCORE":
                    // Client gửi điểm: SCORE|PlayerName|Score
                    if (parts.Length >= 3)
                    {
                        string scorer = parts[1];
                        int score = int.Parse(parts[2]);
                        playerScores[scorer] = score;
                        OnScoreUpdated?.Invoke(scorer, score);
                        BroadcastMessage(message); // Gửi cho tất cả client
                    }
                    break;

                case "CHAT":
                    // Client gửi tin nhắn chat: CHAT|PlayerName|Message
                    if (parts.Length >= 3)
                    {
                        string sender = parts[1];
                        string chatMsg = parts[2];
                        BroadcastMessage($"CHAT|{sender}|{chatMsg}");
                    }
                    break;

                case "GAME_START":
                    // Host bắt đầu game
                    BroadcastMessage("GAME_START|");
                    break;

                case "GAME_END":
                    // Host kết thúc game
                    BroadcastMessage("GAME_END|");
                    break;
            }
        }

        // Gửi tin nhắn đến tất cả client
        public void BroadcastMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            
            foreach (var client in clients)
            {
                try
                {
                    if (client.Connected)
                    {
                        NetworkStream stream = client.GetStream();
                        stream.Write(data, 0, data.Length);
                    }
                }
                catch (Exception ex)
                {
                    OnMessageReceived?.Invoke($"Error broadcasting: {ex.Message}");
                }
            }
        }

        // Gửi tin nhắn đến client cụ thể
        public void SendToClient(TcpClient client, string message)
        {
            try
            {
                if (client.Connected)
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    NetworkStream stream = client.GetStream();
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                OnMessageReceived?.Invoke($"Error sending to client: {ex.Message}");
            }
        }

        // Ngắt kết nối client
        private void DisconnectClient(TcpClient client)
        {
            if (clientNames.ContainsKey(client))
            {
                string playerName = clientNames[client];
                OnPlayerDisconnected?.Invoke(playerName);
                BroadcastMessage($"PLAYER_LEFT|{playerName}");
                clientNames.Remove(client);
                playerScores.Remove(playerName);
            }

            clients.Remove(client);
            try { client.Close(); } catch { }
        }

        // Lấy danh sách điểm
        public Dictionary<string, int> GetScores()
        {
            return new Dictionary<string, int>(playerScores);
        }

        // Lấy IP của server
        public string GetServerIP()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "127.0.0.1";
        }
        public void BroadcastMessageToClients(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);

            foreach (var client in clients)
            {
                try
                {
                    if (client.Connected)
                    {
                        // Bỏ qua kết nối từ localhost (chính là Host tự connect)
                        string clientIP = ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                        if (clientIP == "127.0.0.1" || clientIP == "::1") continue;

                        NetworkStream stream = client.GetStream();
                        stream.Write(data, 0, data.Length);
                    }
                }
                catch (Exception ex)
                {
                    OnMessageReceived?.Invoke($"Error broadcasting to clients: {ex.Message}");
                }
            }
        }
    }
}
