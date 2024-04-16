using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.UI
{
    public class PageManager : MonoBehaviour
    {
        public enum Pages
        {
            StartPage,
            WarningPage,
            ClientNetworkPage,
            HostAlignPage,
            ClientAlignPage,
            HostAppsPage,
            ClientAppsPage,
        }

        [SerializeField]
        private GameObject startPage;
        [SerializeField]
        private GameObject clientNetworkPage;
        [SerializeField]
        private GameObject hostAlignPage;
        [SerializeField]
        private GameObject clientAlignPage;
        [SerializeField]
        private GameObject hostAppsPage;
        [SerializeField]
        private GameObject clientAppsPage;
        [SerializeField]
        private WarningPage warningPage;

        [SerializeField]
        private GameObject userListCanvas;

        private readonly Dictionary<Pages, GameObject> pages = new();

        private void OnEnable()
        {
            // add pages to list
            pages[Pages.StartPage] = startPage;
            pages[Pages.WarningPage] = warningPage.gameObject;
            pages[Pages.ClientNetworkPage] = clientNetworkPage;
            pages[Pages.HostAlignPage] = hostAlignPage;
            pages[Pages.ClientAlignPage] = clientAlignPage;
            pages[Pages.HostAppsPage] = hostAppsPage;
            pages[Pages.ClientAppsPage] = clientAppsPage;

            // register callbacks
            UserManager.OnLocalUserSpawned += OnLocalUserSpawned;
            UserManager.OnLocalUserDespawned += OnLocalUserDespawned;
            NetworkController.Instance.OnConnected += OnConnected;
            NetworkController.Instance.OnDisconnected += OnDisconnected;

            Logger.Log("On Enable");
            if (UserManager.TryGetLocalUser(out UserProperty localUser))
            {
                localUser.isAligned.OnValueChanged += OnIsAlignChanged;
                Logger.Log("registered");
            }
            else
            {
                Logger.LogWarning("Failed to get local user");
            }

            // show user list
            if (NetworkController.Instance.isConnected)
            {
                userListCanvas.SetActive(true);
            }
        }

        private void OnDisable()
        {
            // deregister callbacks
            UserManager.OnLocalUserSpawned -= OnLocalUserSpawned;
            UserManager.OnLocalUserDespawned -= OnLocalUserDespawned;

            if (NetworkController.Instance != null)
            {
                NetworkController.Instance.OnConnected -= OnConnected;
                NetworkController.Instance.OnDisconnected -= OnDisconnected;
            }

            if (UserManager.TryGetLocalUser(out UserProperty localUser) && localUser != null)
            {
                localUser.isAligned.OnValueChanged -= OnIsAlignChanged;
                Logger.Log("deregistered");
            }
        }

        private void Start()
        {
            // set page 
            SetPage(
                NetworkController.Instance.isActive ?
                    NetworkController.Instance.isServer ?
                        UserManager.GetLocalUser().isAligned.Value ?
                            Pages.HostAppsPage : Pages.HostAlignPage
                    : NetworkController.Instance.isConnected ?
                        UserManager.GetLocalUser().isAligned.Value ?
                            Pages.ClientAppsPage : Pages.ClientAlignPage
                    : Pages.ClientNetworkPage
                : Pages.StartPage
            );
        }

        public void SetPage(Pages page)
        {
            // disable all pages
            foreach (GameObject p in pages.Values)
            {
                p.SetActive(false);
            }

            if (pages[page] == null)
            {
                Logger.LogError("page not exists: " + page);
            }

            // enable the target page
            pages[page].SetActive(true);
            Logger.Log("set page: " + page);
        }

        public void OnLocalUserSpawned()
        {
            Logger.Log("OnLocalUserSpawned");
            if (UserManager.TryGetLocalUser(out UserProperty localUser))
            {
                localUser.isAligned.OnValueChanged += OnIsAlignChanged;
                Logger.Log("registered");
            }
        }

        public void OnLocalUserDespawned()
        {
            Logger.Log("OnLocalUserDespawned");
            if (UserManager.TryGetLocalUser(out UserProperty localUser) && localUser != null)
            {
                localUser.isAligned.OnValueChanged -= OnIsAlignChanged;
                Logger.Log("deregistered");
            }
        }

        public void OnIsAlignChanged(bool previous, bool current)
        {
            Logger.Log("align changed: " + current);

            // update page depends on whether local user has aligned
            if (NetworkController.Instance.isServer)
            {
                SetPage(current ? Pages.HostAppsPage : Pages.HostAlignPage);
            }
            else
            {
                // client
                if (current)
                {
                    SetPage(Pages.ClientAppsPage);
                }
                else
                {
                    // pop warning page
                    warningPage.SetWarning("Align data has been cleared", Pages.ClientAlignPage);
                    SetPage(Pages.WarningPage);
                }
            }
        }

        private void OnConnected()
        {
            // show user list
            userListCanvas.SetActive(true);
        }

        private void OnDisconnected(bool selfDisconnect)
        {
            // hide user list
            userListCanvas.SetActive(false);

            if (selfDisconnect) return;

            // pop warning page when being desconnected
            string warningText = "Network disconnected";
            if (NetworkManager.Singleton.DisconnectReason != string.Empty)
            {
                warningText += "\n" + NetworkManager.Singleton.DisconnectReason;
            }
            warningPage.SetWarning(warningText, Pages.StartPage);
            SetPage(Pages.WarningPage);
        }
    }
}