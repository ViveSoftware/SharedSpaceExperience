using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SharedSpaceExperience
{
    public class HostInitUI : MonoBehaviour
    {
        [SerializeField]
        private MatchManager matchManager;
        [SerializeField]
        private MarkerManager markerManager;

        [SerializeField]
        private GameObject confirmButton;
        [SerializeField]
        private TMP_Text message;

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);

            // start/stop marker detection
            if (active)
            {
                if (!markerManager.StartMarkerDetection())
                {
                    message.text = "Unable to detect markers";
                }

            }
            else
            {
                markerManager.StopMarkerDetection();
            }
        }

        public void OnMarkerSelected()
        {
            if (matchManager.matchState != MatchState.INITIALIZING) return;
            // show confirm button
            confirmButton.SetActive(markerManager.selectedMarker != null);
        }

        public void OnClickConfirmButton()
        {
            if (markerManager.selectedMarker == null ||
                matchManager.matchState != MatchState.INITIALIZING) return;

            // disable confirm button
            confirmButton.GetComponent<Button>().interactable = false;

            // stop marker detection
            Logger.Log("[show marker] ?");
            markerManager.ShowMarkers(false);
            markerManager.StopMarkerDetection();

            // inform match manager
            matchManager.OnHostInit();
        }
    }
}
