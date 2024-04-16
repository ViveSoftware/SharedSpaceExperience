using UnityEngine;
using UnityEngine.UI;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.UI
{
    public class HostAlignPage : MonoBehaviour
    {
        [SerializeField]
        private AlignManager alignManager;
        [SerializeField]
        private PageManager pageManager;
        [SerializeField]
        private Toggle trackableMarkerToggle;

        [SerializeField]
        private GameObject alignMethodSubPage;
        [SerializeField]
        private GameObject trackableMarkerSubPage;
        [SerializeField]
        private GameObject spatialAnchorSubPage;

        [SerializeField]
        private ButtonController nextButton;

        [SerializeField]
        private ButtonController skipButton;

        [SerializeField]
        private AlignManager.AlignMethod selectedMethod = AlignManager.AlignMethod.TrackableMarker;

        private void OnEnable()
        {
            // reset UI
            ResetToAlignMethodSubPage();
        }

        public void ResetToAlignMethodSubPage()
        {
            // reset alignment
            alignManager.ResetAlignment();

            // reset toggle
            if (selectedMethod <= AlignManager.AlignMethod.Skipped)
            {
                selectedMethod = AlignManager.AlignMethod.TrackableMarker;
                trackableMarkerToggle.isOn = true;
            }

            // reset subpages
            alignMethodSubPage.SetActive(true);
            trackableMarkerSubPage.SetActive(false);
            spatialAnchorSubPage.SetActive(false);
            nextButton.SetInteractable(true);
            skipButton.SetInteractable(true);

            // show model since host is consider aligned
            UserManager.Instance.ShowUserDefaultModel(true);
        }

        public void SetAlignMethod(AlignManager.AlignMethod method)
        {
            selectedMethod = method;
        }

        public void OnClickNextButton()
        {
            // get selected aligned method
            // change sub page
            switch (selectedMethod)
            {
                case AlignManager.AlignMethod.TrackableMarker:
                    trackableMarkerSubPage.SetActive(true);
                    break;
                case AlignManager.AlignMethod.SpatialAnchor:
                    spatialAnchorSubPage.SetActive(true);
                    break;
                default:
                    Logger.Log("Unsupport align method: " + selectedMethod);
                    return;
            }

            alignMethodSubPage.SetActive(false);
        }

        public void OnClickSkipButton()
        {
            UserManager.GetLocalUser().isAligned.Value = true;
            UserManager.GetLocalUser().isReady.Value = true;

            // set align method to skipped
            RoomProperty.Instance.alignMethod.Value = AlignManager.AlignMethod.Skipped;
        }

        public void OnClickBackButton()
        {
            // stop host
            NetworkController.Instance.StopNetwork();

            // change page
            pageManager.SetPage(PageManager.Pages.StartPage);
        }
    }
}