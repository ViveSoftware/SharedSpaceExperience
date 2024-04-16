using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience
{
    public class AppTemplateUI : MonoBehaviour
    {

        [SerializeField]
        private AppManager appManager;


        [SerializeField]
        private TMP_Text message;

        [SerializeField]
        private Button backButton;

        [SerializeField]
        private string waitLoadedMessage;
        [SerializeField]
        private string onLoadedMessage;

        private void OnEnable()
        {
            backButton.interactable = true;
            message.text = waitLoadedMessage;

            appManager.OnSceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            appManager.OnSceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded()
        {
            message.text = onLoadedMessage;
            Logger.Log("OnSceneLoaded");
        }

        public void OnClickBack()
        {
            backButton.interactable = false;
            appManager.LeaveApp();
        }

    }
}