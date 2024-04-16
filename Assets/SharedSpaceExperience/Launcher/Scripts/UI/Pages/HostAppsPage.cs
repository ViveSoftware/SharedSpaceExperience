using TMPro;
using UnityEngine;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.UI
{
    public class HostAppsPage : MonoBehaviour
    {
        [SerializeField]
        private ConnectionController connectionController;

        [SerializeField]
        private AppInfo selectedApp = null;

        [SerializeField]
        private ButtonController startButton;
        [SerializeField]
        private TMP_Text message;
        [SerializeField]
        [Multiline]
        private string defaultMessage;
        [SerializeField]
        [Multiline]
        private string notEnoughUserMessage;
        [SerializeField]
        [Multiline]
        private string waitForReadyMessage;
        [SerializeField]
        [Multiline]
        private string readyMessage;

        private void OnEnable()
        {
            // register callbacks
            UserManager.OnUserCountChanged += UpdateUI;
            UserManager.OnUserIsReadyChanged += OnUserIsReadyChanged;

            UpdateUI();

            // show user model
            UserManager.Instance.ShowUserDefaultModel(true);
        }

        private void OnDisable()
        {
            // deregister callbacks
            UserManager.OnUserCountChanged -= UpdateUI;
            UserManager.OnUserIsReadyChanged -= OnUserIsReadyChanged;

            // hide user model
            UserManager.Instance.ShowUserDefaultModel(false);
        }

        private void OnUserIsReadyChanged(ulong uid, bool isReady)
        {
            UpdateUI();
        }

        public void OnSelectApp(AppInfo appSceneName)
        {
            if (string.IsNullOrEmpty(selectedApp.ENTRY_SCENE_NAME)) return;
            selectedApp = appSceneName;
            UpdateUI();
        }

        public void OnClickStartButton()
        {
            if (selectedApp == null) return;
            if (NetworkController.Instance.LoadScene(selectedApp.ENTRY_SCENE_NAME))
            {
                // reject new connection
                connectionController.DeclineConnection();
            }
            else
            {
                // unlock start button (allow cancel)
                Logger.LogError("Failed to load app scene: " + selectedApp.ENTRY_SCENE_NAME);
                startButton.SetInteractable(true);
            }
        }

        public void OnClickBackButton()
        {
            // back to align stage
            RoomProperty.Instance.alignMethod.Value = AlignManager.AlignMethod.NotAligned;
            // set all users to not ready and aligned
            UserManager.Instance.ResetAllUserIsReady();

            // RPC to inform all users (except host) that host is realigning 
            RoomProperty.Instance.RealignClientRPC();
        }

        private void UpdateUI()
        {
            // check is app determined
            if (selectedApp == null)
            {
                startButton.SetInteractable(false);
                message.text = defaultMessage;
                return;
            }

            // check is there enough user for the app
            if (UserManager.UserProperties.Count < selectedApp.MIN_USER_NUM)
            {
                startButton.SetInteractable(false);
                message.text = notEnoughUserMessage;
                return;
            }

            // check are all user ready 
            if (!UserManager.Instance.IsAllUserReady())
            {
                startButton.SetInteractable(false);
                message.text = waitForReadyMessage;
                return;
            }

            startButton.SetInteractable(true);
            message.text = readyMessage;
        }

    }
}