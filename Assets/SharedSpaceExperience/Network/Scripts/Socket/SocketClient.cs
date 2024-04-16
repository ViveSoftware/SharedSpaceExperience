using System.Net.Sockets;
using System.Threading;
using System;
using System.Threading.Tasks;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience
{
    public class SocketClient
    {
        private bool isActive = false;
        private string serverIP = "127.0.0.1";
        private int serverPort = 11111;

        private TcpClient client = null;
        private Thread socketThread;
        public SocketCallbacks callbacks = null;
        private StreamManager streamManager = null;

        public bool StartClient(string ip, int port, SocketCallbacks callbacks = null)
        {
            if (isActive) return false;

            serverIP = ip;
            serverPort = port;
            this.callbacks = callbacks ?? new();

            // start socket thread
            socketThread = new Thread(ClientThreadAsync)
            {
                IsBackground = true
            };
            socketThread.Start();

            return true;
        }

        private async void ClientThreadAsync()
        {
            isActive = true;
            Logger.Log("Thread start");

            while (isActive)
            {
                bool hasConnected = false;
                try
                {
                    // wait for connection
                    Logger.Log($"Try connect to {serverIP}:{serverPort}");
                    // TcpClient can only connect once. Create new one for reconnection
                    client = new TcpClient { NoDelay = true };
                    await client.ConnectAsync(serverIP, serverPort);

                    if (client.Connected)
                    {
                        hasConnected = true;
                        callbacks.OnConnected.Invoke();

                        // handle server
                        streamManager = new(client.GetStream(), callbacks);

                        // start listen to server
                        if (!await streamManager.StartStreamAsync())
                        {
                            Logger.LogError("Stream manager failed to start listener");
                        }
                    }
                }
                catch (SocketException e)
                {
                    Logger.LogError("Failed to connect to server (SocketException)");
                    Logger.LogError("" + e, false);
                }
                catch (Exception e)
                {
                    Logger.LogError("Failed to connect to server (other Exception)");
                    Logger.LogError("" + e, false);
                }
                finally
                {
                    streamManager = null;
                    if (hasConnected)
                    {
                        // disconnected
                        Logger.Log("Disconnected");

                        // trigger call back
                        callbacks.OnDisconnected.Invoke();
                    }
                }
            }
            Logger.Log("Thread end");
            callbacks.OnStopped.Invoke();

            isActive = false;
        }

        public async Task<bool> SendDataAsync(SocketDataPack pack)
        {
            if (!isActive || client == null || !client.Connected || streamManager == null) return false;

            return await streamManager.SendDataAsync(pack);
        }

        public void StopClient()
        {
            // break thread loop
            isActive = false;

            streamManager?.StopStream();
            client.Close();
        }

        public bool Reconnect()
        {
            if (!isActive) return false;

            streamManager?.StopStream();
            client.Close();

            return true;
        }
    }
}