using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FuncHandle
{
    public class LanGameClient
    {
        private TcpClient client;
        private NetworkStream stream;
        private Thread receiveThread;
        private bool isConnected = false;

        public event Action<string> OnMessageReceived;
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string, int> OnScoreUpdated;
        public event Action<string> OnPlayerJoined;
        public event Action<string> OnPlayerLeft;

        public string ServerIP { get; private set; }
        public int Port { get; private set; }
        public string PlayerName { get; private set; }

        public LanGameClient(string playerName)
        {
            PlayerName = playerName;
        }

        // Kết nối đến server
        public bool Connect(string serverIP, int port = 5000)
        {
            try
            {
                ServerIP = serverIP;
                Port = port;

                client = new TcpClient();
                client.Connect(serverIP, port);
                stream = client.GetStream();
                isConnected = true;

                // Gửi tin nhắn tham gia
                SendMessage($"JOIN|{PlayerName}");

                // Bắt đầu nhận tin nhắn
                receiveThread = new Thread(ReceiveMessages);
                receiveThread.IsBackground = true;
                receiveThread.Start();

                OnConnected?.Invoke();
                OnMessageReceived?.Invoke($"Connected to server {serverIP}:{port}");
                return true;
            }
            catch (Exception ex)
            {
                OnMessageReceived?.Invoke($"Connection error: {ex.Message}");
                return false;
            }
        }

        // Ngắt kết nối
        public void Disconnect()
        {
            isConnected = false;
            
            try
            {
                stream?.Close();
                client?.Close();
            }
            catch { }

            OnDisconnected?.Invoke();
            OnMessageReceived?.Invoke("Disconnected from server");
        }

        // Nhận tin nhắn từ server
        private void ReceiveMessages()
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (isConnected && client.Connected)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ProcessMessage(message);
                }
            }
            catch (Exception ex)
            {
                if (isConnected)
                {
                    OnMessageReceived?.Invoke($"Receive error: {ex.Message}");
                }
            }
            finally
            {
                Disconnect();
            }
        }

        // Xử lý tin nhắn từ server
        private void ProcessMessage(string message)
        {
            string[] parts = message.Split('|');
            if (parts.Length < 1) return;

            string command = parts[0];

            switch (command)
            {
                case "PLAYER_JOINED":
                    // Server thông báo có player mới: PLAYER_JOINED|PlayerName
                    if (parts.Length >= 2)
                    {
                        string joinedPlayer = parts[1];
                        OnPlayerJoined?.Invoke(joinedPlayer);
                        OnMessageReceived?.Invoke($"{joinedPlayer} joined the game");
                    }
                    OnMessageReceived?.Invoke(message);
                    break;

                case "PLAYER_LEFT":
                    // Server thông báo player rời: PLAYER_LEFT|PlayerName
                    if (parts.Length >= 2)
                    {
                        string leftPlayer = parts[1];
                        OnPlayerLeft?.Invoke(leftPlayer);
                        OnMessageReceived?.Invoke($"{leftPlayer} left the game");
                    }
                    break;

                case "SCORE":
                    // Server gửi điểm: SCORE|PlayerName|Score
                    if (parts.Length >= 3)
                    {
                        string scorer = parts[1];
                        int score = int.Parse(parts[2]);
                        OnScoreUpdated?.Invoke(scorer, score);
                    }
                    break;

                case "CHAT":
                    // Server gửi tin nhắn chat: CHAT|PlayerName|Message
                    if (parts.Length >= 3)
                    {
                        string sender = parts[1];
                        string chatMsg = parts[2];
                        OnMessageReceived?.Invoke($"[{sender}]: {chatMsg}");
                    }
                    break;

                case "GAME_START":
                    // Server thông báo game bắt đầu
                    OnMessageReceived?.Invoke("Game started!");
                    break;

                case "GAME_END":
                    // Server thông báo game kết thúc
                    OnMessageReceived?.Invoke("Game ended!");
                    break;

                default:
                    OnMessageReceived?.Invoke(message);
                    break;
            }
        }

        // Gửi tin nhắn đến server
        public void SendMessage(string message)
        {
            try
            {
                if (isConnected && client.Connected)
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                OnMessageReceived?.Invoke($"Send error: {ex.Message}");
            }
        }

        // Gửi điểm
        public void SendScore(int score)
        {
            SendMessage($"SCORE|{PlayerName}|{score}");
        }

        // Gửi tin nhắn chat
        public void SendChat(string message)
        {
            SendMessage($"CHAT|{PlayerName}|{message}");
        }

        // Kiểm tra kết nối
        public bool IsConnected()
        {
            return isConnected && client != null && client.Connected;
        }
    }
}
