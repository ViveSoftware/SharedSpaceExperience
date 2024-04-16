using UnityEngine;
using TMPro;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.UI
{
    public class ClientTrackableMarkerSubPage : MonoBehaviour
    {
        [SerializeField]
        private AlignManager alignManager;
        [SerializeField]
        private MarkerManager markerManager;

        [SerializeField]
        private FollowHand followHand;

        [SerializeField]
        private ButtonController rescanButton;
        [SerializeField]
        private ButtonController completeButton;

        [SerializeField]
        private TMP_Text message;

        [SerializeField]
        [Multiline]
        private string defaultMessage;
        [SerializeField]
        [Multiline]
        private string alignedMessage;
        [SerializeField]
        [Multiline]
        private string importFailedMessage;

        [SerializeField]
        private bool isAligned = false;

        private void OnEnable()
        {
            // reset UI
            isAligned = false;
            UpdateUI();

            // reset tracked space
            alignManager.ResetTrackedSpace();

            // start marker detection
            if (!markerManager.ImportAlignData(alignManager.GetAlignData()))
            {
                Logger.LogError("Failed to import align data");
                message.text = importFailedMessage;
                return;
            }
            markerManager.OnDetectNewMarker += OnDetectMarker;
            markerManager.OnSelectedMarkerUpdated += OnSelectMarker; // NOTE: For Fake Marker (Debug) Only
            markerManager.StartMarkerService();
        }

        private void OnDisable()
        {
            // disable preview user model
            UserManager.Instance.PreviewUserDefaultModel(false);

            // stop marker detection
            markerManager.OnDetectNewMarker -= OnDetectMarker;
            markerManager.OnSelectedMarkerUpdated -= OnSelectMarker; // NOTE: For Fake Marker (Debug) Only
            markerManager.StopMarkerService();

            // hide markers
            markerManager.ShowMarkers(false);
        }

        // Note: For Fake Marker (Debug) Only
        private void OnSelectMarker()
        {
            // do align
            if (!markerManager.Align())
            {
                Logger.LogError("Failed to align");
                return;
            }

            // update UI
            isAligned = true;
            UpdateUI();

            // stop marker detection for rescan
            markerManager.StopMarkerService();
        }

        private void OnDetectMarker(Marker marker)
        {
            // do align
            if (!markerManager.Align(marker))
            {
                Logger.LogError("Failed to align");
                return;
            }

            // update UI
            followHand.ResetPose();
            isAligned = true;
            UpdateUI();

            // stop marker detection for rescan
            markerManager.StopMarkerService();
        }

        public void OnClickRescan()
        {
            // reset tracked space
            alignManager.ResetTrackedSpace();

            // start marker detection to rescan?
            markerManager.ShowMarkers(false);
            markerManager.StartMarkerService();

            // update UI
            isAligned = false;
            UpdateUI();
        }

        public void OnClickComplete()
        {
            // disable UI
            rescanButton.SetInteractable(false);

            // hide markers
            markerManager.ShowMarkers(false);

            // set client aligned
            UserManager.GetLocalUser().isAligned.Value = true;
        }

        private void UpdateUI()
        {
            rescanButton.gameObject.SetActive(isAligned);
            rescanButton.SetInteractable(isAligned);
            completeButton.gameObject.SetActive(isAligned);
            completeButton.SetInteractable(isAligned);

            message.text = isAligned ? alignedMessage : defaultMessage;

            // preview user model to check whether aligned properly
            UserManager.Instance.PreviewUserDefaultModel(isAligned);
        }
    }
}