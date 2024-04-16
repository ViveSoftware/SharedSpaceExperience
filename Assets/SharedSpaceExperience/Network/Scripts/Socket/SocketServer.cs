using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience
{
    public class SocketServer
    {
        private bool isActive = false;

        private TcpListener server = null;
        private Thread socketThread;
        public SocketCallbacks callbacks = null;
        private readonly Dictionary<TcpClient, StreamManager> clients = new();

        public bool StartServer(string ip, int port, SocketCallbacks callbacks = null)
        {
            if (isActive) return false;

            this.callbacks = callbacks ?? new();

            Logger.Log($"Server: {ip}:{port}");
            server = new TcpListener(IPAddress.Parse(ip), port);
            server.Start();

            // start socket thread
            socketThread = new Thread(ServerThreadAsync)
            {
                IsBackground = true
            };
            socketThread.Start();

            return true;
        }

        private async void ServerThreadAsync()
        {
            isActive = true;
            Logger.Log("Thread start");

            while (isActive)
            {
                try
                {
                    // wait for connection
                    Logger.Log("Wait for connection");
                    TcpClient client = await server.AcceptTcpClientAsync();

                    if (client.Connected)
                    {
                        callbacks.OnConnected.Invoke();

                        // handle client
                        HandleClientAsync(client);
                    }
                }
                catch (SocketException e)
                {
                    Logger.LogError("Failed to connect to client");
                    Logger.LogError("" + e, false);
                    callbacks.OnError(e.ErrorCode);

                    if (e.ErrorCode == 10022)
                    {
                        // Invalid argument
                        // May happen when being disconnected from AP
                        isActive = false;
                        break;
                    }
                }
                catch (ObjectDisposedException e)
                {
                    Logger.LogWarning("Socket is diposed due to termination");
                    Logger.LogWarning("" + e, false);
                }
                catch (Exception e)
                {
                    Logger.LogError("Something failed when connecting to client");
                    Logger.LogError("" + e, false);
                }
            }
            Logger.Log("Thread end");

            callbacks.OnStopped.Invoke();

            isActive = false;
        }

        private async void HandleClientAsync(TcpClient client)
        {
            try
            {
                StreamManager streamManager = new(client.GetStream(), callbacks);
                clients.Add(client, streamManager);

                // start listen to client
                if (!await streamManager.StartStreamAsync())
                {
                    Logger.LogError("Stream manager failed to start listener");
                }
            }
            catch (SocketException e)
            {
                Logger.LogError("Connection failed: " + e.ErrorCode);
                Logger.LogError("" + e, false);

            }
            catch (Exception e)
            {
                Logger.LogError("Something failed when streaming with client");
                Logger.LogError("" + e, false);
            }
            finally
            {
                // disconnected
                Logger.Log("Disconnected");

                clients.Remove(client);

                // trigger callback
                callbacks.OnDisconnected.Invoke();
            }
        }

        public void DisconnectAllClients()
        {
            // stop all stream
            foreach (TcpClient client in clients.Keys)
            {
                clients[client]?.StopStream();
                client?.Close();
            }
            clients.Clear();
        }

        public void StopServer()
        {
            // break thread loop
            isActive = false;

            DisconnectAllClients();

            server.Stop();
        }
    }
}