using TMPro;
using UnityEngine;

namespace SharedSpaceExperience.UI
{
    public class WarningPage : MonoBehaviour
    {
        [SerializeField]
        private PageManager pageManager;

        [SerializeField]
        private TMP_Text message;

        [SerializeField]
        private PageManager.Pages returnPage;


        public void SetWarning(string warningMessage, PageManager.Pages returnPage)
        {
            message.text = warningMessage;
            this.returnPage = returnPage;
        }

        public void OnClickBackButton()
        {
            // change page
            pageManager.SetPage(returnPage);
        }
    }
}