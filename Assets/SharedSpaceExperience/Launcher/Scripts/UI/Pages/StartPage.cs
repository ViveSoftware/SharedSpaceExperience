using UnityEngine;
using Unity.Netcode;
using TMPro;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.UI
{
    public class StartPage : MonoBehaviour
    {
        [SerializeField]
        private PageManager pageManager;

        [SerializeField]
        private TMP_InputField userNameInput;

        [SerializeField]
        private ButtonController hostButton;
        [SerializeField]
        private ButtonController joinButton;

        private void OnEnable()
        {
            // reset
            ResetUI();

            // get user name
            userNameInput.text = UserManager.Instance.localUserProperty.userName;
        }

        public void OnSubmitUserName(string userName)
        {
            UserManager.Instance.localUserProperty.userName = userName;
            UserManager.Instance.localUserProperty.Save();
        }

        public void OnClickHostButton()
        {
            // disable join button
            joinButton.SetInteractable(false);

            // set discovery server name
            NetworkController.Instance.SetDiscoveryServerName(
                UserManager.Instance.localUserProperty.userName
            );

            // register callbacks
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;

            // start hosting
            if (!NetworkController.Instance.StartHost(NetworkAccess.INTRANET))
            {
                OnStartServerFailed();
            };
        }

        public void OnClickJoinButton()
        {
            // disable host button
            hostButton.SetInteractable(false);

            // change page
            pageManager.SetPage(PageManager.Pages.ClientNetworkPage);

        }

        private void OnServerStarted()
        {
            // deregister callbacks
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;

            // change page
            pageManager.SetPage(PageManager.Pages.HostAlignPage);

        }

        private void OnStartServerFailed()
        {
            // deregister callbacks
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;

            // reset
            ResetUI();

            // TODO: show error message
            Logger.LogError("Failed to start server");
        }

        private void ResetUI()
        {
            // enable buttons
            hostButton.SetInteractable(true);
            joinButton.SetInteractable(true);
        }
    }
}