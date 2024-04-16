using UnityEngine;
using Unity.Netcode;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.UI
{
    public class ClientNetworkPage : MonoBehaviour
    {
        [SerializeField]
        private PageManager pageManager;

        [SerializeField]
        private SearchHostSubPage searchHostPage;
        [SerializeField]
        private DirectConnectSubPage directConnectPage;

        [SerializeField]
        private TabButtonController searchHostTabButton;
        [SerializeField]
        private TabButtonController directConnectTabButton;

        [SerializeField]
        private SwapSpriteToggle connectButton;

        [SerializeField]
        private string hostIP = "";

        [SerializeField]
        private bool isConnecting = false;

        private bool isConnected = false; // for change page


        private void OnEnable()
        {
            // reset button and update based on hostIP
            UpdateUI(true);
            UpdateHostInfo(hostIP);
        }

        public void UpdateHostInfo(string ip)
        {
            // block if is connecting
            if (isConnecting) return;

            // update ip
            hostIP = ip;

            // validate IP format
            connectButton.SetInteractable(NetworkController.IsValidIPv4(hostIP));
        }

        public void OnClickConnect(bool start)
        {
            if (start)
            {
                // disable UI
                UpdateUI(false);

                // register callbacks
                NetworkManager.Singleton.OnClientStarted += OnClientStarted;
                NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

                SocketManager.Instance.callbacks.OnConnected += OnClientConnectedCallback;

                // connect to server
                NetworkController.Instance.ConnectToServer(hostIP, NetworkController.Instance.port);
            }
            else
            {
                // register callbacks
                NetworkManager.Singleton.OnClientStopped += OnClientStopped;

                // stop client
                NetworkController.Instance.StopNetwork(false);
            }
        }

        private void OnClientStarted()
        {
            // deregister callback
            NetworkManager.Singleton.OnClientStarted -= OnClientStarted;

            // update button (allow cancel connection)
            connectButton.OnStateChanged(true);
        }

        private void OnClientStopped(bool what)
        {
            // deregister callbacks
            DeregisterNetworkCallbacks();

            // reset UI
            UpdateUI(true);
        }

        private void OnClientDisconnectCallback(ulong uid)
        {
            // deregister callbacks
            DeregisterNetworkCallbacks();

            // reset UI
            UpdateUI(true);

            if (!NetworkController.Instance.isServer && NetworkManager.Singleton.DisconnectReason != string.Empty)
            {
                Logger.LogError($"Connection declined by server: {NetworkManager.Singleton.DisconnectReason}");
            }
        }

        private void OnTransportFailure()
        {
            // deregister callbacks
            DeregisterNetworkCallbacks();

            // reset UI
            UpdateUI(true);
        }

        private void OnClientConnectedCallback()
        {
            // wait for both netcode and socket connected

            // deregister callbacks
            DeregisterNetworkCallbacks();

            // set flag
            isConnecting = false;
            isConnected = true;
        }

        private void DeregisterNetworkCallbacks()
        {
            NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
            NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
            NetworkManager.Singleton.OnTransportFailure -= OnTransportFailure;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;

            SocketManager.Instance.callbacks.OnConnected -= OnClientConnectedCallback;
        }


        public void OnClickBackButton()
        {
            // stop host
            NetworkController.Instance.StopNetwork();

            // change page
            pageManager.SetPage(PageManager.Pages.StartPage);
        }

        private void Update()
        {
            if (isConnected)
            {
                // On connected
                isConnected = false;

                // save last successfully connected host info
                UserManager.Instance.localUserProperty.lastConnectedServer = hostIP;
                UserManager.Instance.localUserProperty.Save();
                // change page
                pageManager.SetPage(PageManager.Pages.ClientAlignPage);
            }
        }

        private void UpdateUI(bool enable)
        {
            isConnecting = !enable;
            isConnected = false;
            connectButton.OnStateChanged(false);

            searchHostPage.SetInteractable(enable);
            directConnectPage.SetInteractable(enable);

            searchHostTabButton.SetInteractable(enable);
            directConnectTabButton.SetInteractable(enable);
        }
    }
}