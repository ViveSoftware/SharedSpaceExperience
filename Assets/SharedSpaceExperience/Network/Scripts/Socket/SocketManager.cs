using System.Collections;
using UnityEngine;
using Unity.Netcode;
using System;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience
{
    public class SocketManager : MonoBehaviour
    {
        public static SocketManager Instance { get; private set; }

        public bool isServer;
        public bool isActive;

        [SerializeField]
        private int socketPort = 11111;
        [SerializeField]
        private ulong appID = 0;
        public SocketServer server = null;
        public SocketClient client = null;

        public SocketCallbacks callbacks = new();

        public Action BeforeSocketStart;
        public Action BeforeSocketStop;

        private void OnValidate()
        {
            // generate App ID
            if (appID == 0)
            {
                byte[] bytes = new byte[8];
                new System.Random().NextBytes(bytes);
                appID = BitConverter.ToUInt64(bytes, 0);

                // set to data pack
                SocketDataPack.CHECK_CODE = appID;
            }
        }

        private void Awake()
        {
            // register callbacks
            callbacks.OnError += OnSocketError;
        }

        private void OnEnable()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(this);

            // wait for NetworkManager.Singleton to initialize
            StartCoroutine(WaitForManagers());
        }

        private IEnumerator WaitForManagers()
        {
            yield return new WaitUntil(() => NetworkManager.Singleton);
            NetworkManager.Singleton.OnServerStarted += OnNetcodeServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += OnNetcodeClientConnected;
            NetworkManager.Singleton.OnServerStopped += OnNetcodeStopped;
            NetworkManager.Singleton.OnClientStopped += OnNetcodeStopped;
        }

        private void OnDisable()
        {
            // stop socket
            StopSocket();

            if (NetworkManager.Singleton == null) return;
            NetworkManager.Singleton.OnServerStarted -= OnNetcodeServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnNetcodeClientConnected;
            NetworkManager.Singleton.OnServerStopped -= OnNetcodeStopped;
            NetworkManager.Singleton.OnClientStopped -= OnNetcodeStopped;
        }

        private void OnApplicationQuit()
        {
            // stop socket
            StopSocket();
        }

        private void OnNetcodeServerStarted()
        {
            if (!NetworkController.Instance.isServer) return;
            isServer = true;

            // start socket as server
            server = new();
            BeforeSocketStart?.Invoke();
            server.StartServer(NetworkController.Instance.ip, socketPort, callbacks);

            Logger.Log("start socket server");

            isActive = true;
        }

        private void OnNetcodeClientConnected(ulong clientId)
        {
            if (NetworkController.Instance.isServer || clientId != NetworkManager.Singleton.LocalClientId) return;
            isServer = false;

            // start socket as client
            client = new();
            BeforeSocketStart?.Invoke();
            client.StartClient(NetworkController.Instance.serverIP, socketPort, callbacks);

            isActive = true;
        }

        private void OnNetcodeStopped(bool what)
        {
            // stop socket
            StopSocket();
        }

        private void StopSocket()
        {
            BeforeSocketStop?.Invoke();

            if (server != null)
            {
                server.StopServer();
                server = null;
            }

            if (client != null)
            {
                client.StopClient();
                client = null;
            }
        }

        private void OnSocketError(int errorCode)
        {
            if (errorCode == 10022)
            {
                // Could be the host disconnected from the AP
                if (isServer)
                {
                    // stop netcode
                    NetworkController.Instance.StopNetwork(true, false);
                }
            }
        }

        public bool Reconnect()
        {
            if (isServer || client == null) return false;
            return client.Reconnect();
        }
    }
}