using UnityEngine;
using TMPro;

namespace SharedSpaceExperience
{
    public class WaitingUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text message;
        [SerializeField]
        private MatchManager matchManager;

        private string waitHostMessage = "Waiting for host...";
        private string waitClientMessage = "Waiting for the other player...";

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);

            if (active)
            {
                if (matchManager.matchState == MatchState.INITIALIZING)
                {
                    message.text = waitHostMessage;
                }
                else
                {
                    message.text = waitClientMessage;
                }
            }
        }
    }
}