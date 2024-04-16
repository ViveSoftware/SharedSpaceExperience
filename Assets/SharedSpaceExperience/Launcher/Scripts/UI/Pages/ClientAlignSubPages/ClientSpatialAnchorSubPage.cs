using UnityEngine;
using TMPro;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.UI
{
    public class ClientSpatialAnchorSubPage : MonoBehaviour
    {

        [SerializeField]
        private AlignManager alignManager;
        [SerializeField]
        private SpatialManager spatialManager;

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

            // register callback
            spatialManager.OnAligned += OnAligned;

            // start spatial anchor service
            spatialManager.StartSpatialAnchorService();

            // import align data
            ImportAlignData();
        }

        private void OnDisable()
        {
            // disable preview user model
            UserManager.Instance.PreviewUserDefaultModel(false);

            // deregister callback
            spatialManager.OnAligned -= OnAligned;

            // stop spatial anchor service
            spatialManager.StopSpatialAnchorService();
        }

        private void OnAligned()
        {
            // update UI
            followHand.ResetPose();
            isAligned = true;
            UpdateUI();
        }

        public void OnClickRescan()
        {
            // reset tracked space
            alignManager.ResetTrackedSpace();

            // delete previous anchor
            spatialManager.DestroyAlignAnchor();

            // reimport align data
            ImportAlignData();

            // update UI
            isAligned = false;
            UpdateUI();
        }

        private void ImportAlignData()
        {
            if (!spatialManager.ImportAlignData(alignManager.GetAlignData()))
            {
                Logger.LogError("Failed to import align data");
                message.text = importFailedMessage;

                // deregister callback
                spatialManager.OnAligned -= OnAligned;
            }
        }

        public void OnClickComplete()
        {
            // disable UI
            rescanButton.SetInteractable(false);

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