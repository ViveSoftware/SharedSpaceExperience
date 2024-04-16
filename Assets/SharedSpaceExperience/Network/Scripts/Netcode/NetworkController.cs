using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Net;
using System.Net.Sockets;
using System;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience
{
    public enum NetworkAccess
    {
        LOCALHOST,
        INTRANET,
        INTERNET
    }

    [RequireComponent(typeof(NetworkManager), typeof(UnityTransport))]
    public class NetworkController : MonoBehaviour
    {
        public static NetworkController Instance { get; private set; }

        private UnityTransport transport;
        [SerializeField]
        private HostDiscrovery discrovery = null;

        public NetworkAccess networkAccess = NetworkAccess.LOCALHOST;
        public string ip = "127.0.0.1";
        public ushort port = 7777;
        public string serverIP = "127.0.0.1";

        public bool isActive => NetworkManager.Singleton.IsListening;
        public bool isConnected => NetworkManager.Singleton.IsConnectedClient;
        public bool isServer => NetworkManager.Singleton.IsServer;
        public bool isClient => NetworkManager.Singleton.IsClient;
        public bool isHost => NetworkManager.Singleton.IsHost;
        public bool isDiscovering => discrovery != null && discrovery.IsRunning;

        private bool selfDisconnect = false;

        public Action OnConnected;
        public Action<bool> OnDisconnected;

        private bool approveConnection = true;
        private string declineReason = "";

        public void Awake()
        {
            transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        }

        private void OnEnable()
        {
            // singleton
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientStopped += OnClientStopped;
        }

        private void OnDisable()
        {
            // singleton
            if (Instance == this) Instance = null;

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
            }
        }

        public bool StartHost(NetworkAccess access = NetworkAccess.LOCALHOST)
        {
            // set server network access 
            networkAccess = access;
            switch (networkAccess)
            {
                case NetworkAccess.LOCALHOST:
                    ip = "127.0.0.1";
                    break;
                case NetworkAccess.INTRANET:
                    ip = GetLocalIP();
                    break;
                case NetworkAccess.INTERNET:
                    ip = "0.0.0.0";
                    break;
            }
            return StartNetwork(true, ip, port);
        }

        public bool ConnectToServer(string serverIP, ushort serverPort)
        {
            ip = GetLocalIP();
            port = serverPort;
            return StartNetwork(false, serverIP, serverPort);
        }

        public bool StartNetwork(bool startAsHost, string serverIP, ushort serverPort)
        {
            // check if network already start
            if (isActive) return false;

            // setting network
            this.serverIP = serverIP;
            transport.SetConnectionData(serverIP, serverPort);

            // start network
            bool success;
            if (startAsHost)
            {
                NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
                success = NetworkManager.Singleton.StartHost();
            }
            else success = NetworkManager.Singleton.StartClient();

            Logger.Log(
                $"Network start: {success}\n" +
                $"Local IP: {ip}\n Server: {serverIP}:{serverPort}\n" +
                $"Is Server: {isServer}\nIs Client: {isClient}\nIs Host: {isHost}"
            );

            return success;
        }

        public void SetApprovalCheck(bool approved, string reason = "")
        {
            approveConnection = approved;
            declineReason = reason;
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            Logger.Log($"approval: {approveConnection}");
            response.Approved = approveConnection;
            response.CreatePlayerObject = approveConnection;
            response.Reason = declineReason;
        }

        public void StopNetwork(bool stopDsicovery = true, bool selfDisconnect = true)
        {
            this.selfDisconnect = selfDisconnect;

            // stop network discovery
            if (stopDsicovery) StopDiscovery();

            // stop connection
            NetworkManager.Singleton.Shutdown();
        }

        /* Network discovery */
        public void SetDiscoveryServerName(string name)
        {
            if (discrovery != null) discrovery.serverName = name;
        }

        public void StartDiscovery()
        {
            if (discrovery == null || discrovery.IsRunning) return;
            if (isServer)
            {
                discrovery.StartServer();
            }
            else
            {
                discrovery.StartClient();
            }
        }

        public void StopDiscovery()
        {
            if (isDiscovering)
            {
                discrovery.StopDiscovery();
            }
        }

        public bool SearchHost()
        {
            if (isDiscovering)
            {
                discrovery.ClientBroadcast(new DiscoveryBroadcastData());
                return true;
            }

            Logger.LogError("Network discovery is not enabled");
            return false;
        }

        private string GetLocalIP()
        {
            IPHostEntry host;
            string localIP = "0.0.0.0";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }

            return localIP;
        }

        public static bool IsValidIPv4(string ip)
        {
            return ip.Split(".").Length == 4 && IPAddress.TryParse(ip, out _);
        }

        private void OnClientConnected(ulong uid)
        {
            if (uid == NetworkManager.Singleton.LocalClientId) OnConnected?.Invoke();
        }

        private void OnClientStopped(bool what)
        {
            OnDisconnected?.Invoke(selfDisconnect);
            selfDisconnect = false;
        }

        public bool LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (!(isConnected && isServer && !string.IsNullOrEmpty(sceneName))) return false;

            // load app scene
            SceneEventProgressStatus status = NetworkManager.Singleton.SceneManager.LoadScene(sceneName, mode);
            if (status == SceneEventProgressStatus.Started)
            {
                Logger.Log($"Load scene: {sceneName}");
                return true;
            }
            else
            {
                Logger.LogError($"Failed to load scene: {sceneName}.\nStatus: {status}");
                return false;
            }
        }
    }
}