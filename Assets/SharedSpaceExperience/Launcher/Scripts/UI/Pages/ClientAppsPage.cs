using UnityEngine;
using TMPro;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.UI
{
    public class ClientAppsPage : MonoBehaviour
    {
        [SerializeField]
        private PageManager pageManager;

        [SerializeField]
        private SwapSpriteToggle toggle;
        [SerializeField]
        private TMP_Text message;
        [SerializeField]
        [Multiline]
        private string defaultMessage;
        [SerializeField]
        [Multiline]
        private string readyMessage;

        private void OnEnable()
        {
            UserManager.GetLocalUser().isReady.OnValueChanged += OnIsReadyChanged;

            // reset UI
            UpdateUI(UserManager.GetLocalUser().isReady.Value);

            // show user model if aligned
            UserManager.Instance.ShowUserDefaultModel(true);
        }

        private void OnDisable()
        {
            if (UserManager.TryGetLocalUser(out UserProperty localUser))
            {
                localUser.isReady.OnValueChanged -= OnIsReadyChanged;
            }

            // dont show user model if aligned
            UserManager.Instance.ShowUserDefaultModel(false);
        }

        public void OnClickReadyButton(bool ready)
        {
            // set ready
            Logger.Log(ready.ToString());
            UserManager.GetLocalUser().SetIsReadyServerRpc(ready);
        }

        private void OnIsReadyChanged(bool previous, bool current)
        {
            // update ready button
            UpdateUI(current);
        }

        public void OnClickBackButton()
        {
            // disconnect from host
            NetworkController.Instance.StopNetwork();

            // change page
            pageManager.SetPage(PageManager.Pages.ClientNetworkPage);
        }


        private void UpdateUI(bool isReady)
        {
            toggle.OnStateChanged(isReady);
            if (isReady)
            {
                message.text = readyMessage;
            }
            else
            {
                message.text = defaultMessage;
            }
        }
    }
}