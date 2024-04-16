#if UNITY_EDITOR || !UNITY_ANDROID
#define PC_DEBUG
#endif

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.UI
{
    public class HostTrackableMarkerSubPage : MonoBehaviour
    {
        [SerializeField]
        private AlignManager alignManager;
        [SerializeField]
        private MarkerManager markerManager;
        [SerializeField]
        private ButtonController rescanButton;

        [SerializeField]
        private ButtonController completeButton;
        [SerializeField]
        private Button backButton;

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
        private string exportFailedMessage;

        [SerializeField]
        private bool isAligned = false;

        private bool isExportDone = false;
        private AlignData alignData = null;

        private void OnEnable()
        {
            // reset UI
            isAligned = false;
            UpdateUI();

            // start marker detection 
            markerManager.UnsetTargetMarker();
            markerManager.OnSelectedMarkerUpdated += OnSelectMarker;
            markerManager.StartMarkerService();
        }

        private void OnDisable()
        {
            StopCoroutine(ExportAlignData());

            // stop marker detection
            markerManager.OnSelectedMarkerUpdated -= OnSelectMarker;
            markerManager.StopMarkerService();

            // hide markers
            markerManager.ShowMarkers(false);
        }

        public void OnSelectMarker()
        {
            // stop marker detection
            markerManager.StopMarkerDetection();

            // update UI
            isAligned = true;
            UpdateUI();
        }

        public void OnClickRescan()
        {
            // restart marker detection
            markerManager.ShowMarkers(false);
            markerManager.StopMarkerService();
            markerManager.StartMarkerService();

            // reset UI
            isAligned = false;
            UpdateUI();
        }

        public void OnClickConfirm()
        {
            // disable UI
            markerManager.ShowMarkers(false);
            rescanButton.SetInteractable(false);
            backButton.interactable = false;

            // export align data asynchronously to prevent freezing
            StartCoroutine(ExportAlignData());
        }

        private async Task ExportAlignDataAsync()
        {
            await Task.Run(() =>
            {
                markerManager.ExportAlignData(out alignData);
                isExportDone = true;
            });
        }

        private IEnumerator ExportAlignData()
        {
            alignData = null;
            isExportDone = false;

#if !PC_DEBUG
            ExportAlignDataAsync();
#else
            markerManager.ExportAlignData(out alignData);
            isExportDone = true;
#endif
            // wait for export
            yield return new WaitUntil(() => isExportDone);


            if (alignData != null)
            {
                // save align data
                alignManager.SetAlignData(alignData);

                // set host aligned and ready
                UserManager.GetLocalUser().isAligned.Value = true;
                UserManager.GetLocalUser().isReady.Value = true;

                // set align method
                RoomProperty.Instance.alignMethod.Value = AlignManager.AlignMethod.TrackableMarker;
            }
            else
            {
                // enable UI
                rescanButton.SetInteractable(true);
                backButton.interactable = true;
                message.text = exportFailedMessage;
                Logger.LogError("Failed to export align data");
            }
        }

        private void UpdateUI()
        {
            completeButton.gameObject.SetActive(isAligned);
            completeButton.SetInteractable(isAligned);
            rescanButton.gameObject.SetActive(isAligned);
            rescanButton.SetInteractable(isAligned);
            backButton.interactable = true;

            message.text = isAligned ? alignedMessage : defaultMessage;
        }
    }
}