#if UNITY_EDITOR || !UNITY_ANDROID
#define PC_DEBUG
#endif

using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.UI
{
    public class HostSpatialAnchorSubPage : MonoBehaviour
    {
        [SerializeField]
        private AlignManager alignManager;
        [SerializeField]
        private SpatialManager spatialManager;
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

        public InputAction createAnchorInput;

        private void OnEnable()
        {
#if !PC_DEBUG
            isAligned = false;
#else
            // assume to be aligned in PC debug mode
            isAligned = true;
#endif
            // reset UI
            UpdateUI();

            // start spatial anchor service
            spatialManager.StartSpatialAnchorService();
        }

        private void OnDisable()
        {
            StopCoroutine(ExportAlignData());

            // deregister callback
            createAnchorInput.started -= CreateAnchor;
            createAnchorInput.Disable();

            // stop spatial anchor service
            spatialManager.StopSpatialAnchorService();
        }

        private void CreateAnchor(InputAction.CallbackContext context)
        {
            // create anchor
            if (!spatialManager.CreateAlignAnchor()) return;

            // update UI
            isAligned = true;
            UpdateUI();
        }

        public void OnClickRescan()
        {
            // destroy previous anchor
            spatialManager.DestroyAlignAnchor();

            // reset UI
            isAligned = false;
            UpdateUI();
        }

        public void OnClickConfirm()
        {
            // disable UI
            rescanButton.SetInteractable(false);
            backButton.interactable = false;

            // export align data asynchronously to prevent freezing
            StartCoroutine(ExportAlignData());
        }

        private async Task ExportAlignDataAsync()
        {
            await Task.Run(() =>
            {
                spatialManager.ExportAlignData(out alignData);
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
            spatialManager.ExportAlignData(out alignData);
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
                RoomProperty.Instance.alignMethod.Value = AlignManager.AlignMethod.SpatialAnchor;
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

            if (!isAligned)
            {
                message.text = defaultMessage;
                createAnchorInput.Enable();
                createAnchorInput.started += CreateAnchor;
            }
            else
            {
                message.text = alignedMessage;
                createAnchorInput.started -= CreateAnchor;
                createAnchorInput.Disable();
            }
        }
    }
}