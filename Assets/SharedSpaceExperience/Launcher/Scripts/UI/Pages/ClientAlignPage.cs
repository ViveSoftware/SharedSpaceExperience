using TMPro;
using UnityEngine;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.UI
{
    public class ClientAlignPage : MonoBehaviour
    {
        [SerializeField]
        private AlignManager alignManager;

        [SerializeField]
        private PageManager pageManager;

        [SerializeField]
        private GameObject messageSubPage;
        [SerializeField]
        private GameObject trackebleMarkerSubPage;
        [SerializeField]
        private GameObject spatialAnchorSubPage;

        [SerializeField]
        private ProgressBar progressBar;

        [SerializeField]
        private TMP_Text message;
        [SerializeField]
        private string waitHostMessage;
        [SerializeField]
        private string loadingMessage;

        private bool isLoaded = false;
        private bool prevIsLoaded = false;

        private void OnEnable()
        {
            // register callback
            RoomProperty.Instance.alignMethod.OnValueChanged += OnAlignMethodChanged;

            // reset UI
            ResetSubPage();

        }

        private void OnDisable()
        {
            // disable subpages to prevent realign cause freezing
            trackebleMarkerSubPage.SetActive(false);
            spatialAnchorSubPage.SetActive(false);

            // deregister callbacks
            RoomProperty.Instance.alignMethod.OnValueChanged -= OnAlignMethodChanged;
            alignManager.OnLoadingAlignData -= progressBar.SetProgress;
            alignManager.OnAlignDataLoaded -= OnAlignDataLoaded;
        }

        private void Update()
        {
            // update subpage
            if (prevIsLoaded != isLoaded)
            {
                if (isLoaded) UpdateSubPages();
                prevIsLoaded = isLoaded;
            }
        }

        private void OnAlignMethodChanged(AlignManager.AlignMethod previous, AlignManager.AlignMethod current)
        {
            // Note: this will interrupt client align process if host update align method
            ResetSubPage();
        }

        private void ResetSubPage()
        {
            // reset 
            isLoaded = false;
            prevIsLoaded = false;
            progressBar.SetProgress(0, 0);

            // deregister callback if has registered
            alignManager.OnLoadingAlignData -= progressBar.SetProgress;
            alignManager.OnAlignDataLoaded -= OnAlignDataLoaded;

            // disable align method subpages
            trackebleMarkerSubPage.SetActive(false);
            spatialAnchorSubPage.SetActive(false);

            // reset alignment
            alignManager.ResetAlignment();

            // show user model if aligned
            UserManager.Instance.ShowUserDefaultModel(true);

            // host aligned, change sub page
            Logger.Log("align method: " + RoomProperty.Instance.alignMethod.Value);
            switch (RoomProperty.Instance.alignMethod.Value)
            {
                case AlignManager.AlignMethod.Skipped:
                    // directly set is aligned
                    UserManager.GetLocalUser().isAligned.Value = true;
                    break;

                case AlignManager.AlignMethod.NotAligned:
                    // update UI
                    message.text = waitHostMessage;
                    progressBar.gameObject.SetActive(false);
                    messageSubPage.SetActive(true);
                    break;

                default:
                    // update UI
                    message.text = loadingMessage;
                    progressBar.SetProgress(0, 0);
                    progressBar.gameObject.SetActive(true);
                    messageSubPage.SetActive(true);
                    alignManager.OnLoadingAlignData += progressBar.SetProgress;
                    alignManager.OnAlignDataLoaded += OnAlignDataLoaded;

                    // request align data
                    alignManager.RequestAlignData();
                    break;
            }
        }

        private void OnAlignDataLoaded(bool success)
        {
            if (!success)
            {
                Logger.Log("Request align data again");
                alignManager.RequestAlignData();
                return;
            }

            // deregister callback
            alignManager.OnLoadingAlignData -= progressBar.SetProgress;
            alignManager.OnAlignDataLoaded -= OnAlignDataLoaded;

            // set flag
            isLoaded = true;
        }

        private void UpdateSubPages()
        {
            Logger.Log("UpdateSubPages: " + RoomProperty.Instance.alignMethod.Value);

            // change subpage
            messageSubPage.SetActive(false);
            switch (RoomProperty.Instance.alignMethod.Value)
            {
                case AlignManager.AlignMethod.TrackableMarker:
                    trackebleMarkerSubPage.SetActive(true);
                    break;

                case AlignManager.AlignMethod.SpatialAnchor:
                    spatialAnchorSubPage.SetActive(true);
                    break;

                case AlignManager.AlignMethod.Skipped:
                case AlignManager.AlignMethod.NotAligned:
                default:
                    messageSubPage.SetActive(true);
                    progressBar.gameObject.SetActive(false);
                    message.text = $"<color=#FF0000>Unsupported align method: {RoomProperty.Instance.alignMethod.Value}" +
                        "\n\nPlease make sure all the devices use the same version \nof this app.</color>";
                    Logger.LogError("Unsupported align method: " + RoomProperty.Instance.alignMethod.Value);
                    break;
            }
        }

        public void OnClickBackButton()
        {
            // disconnect from host
            NetworkController.Instance.StopNetwork();

            // change page
            pageManager.SetPage(PageManager.Pages.ClientNetworkPage);
        }
    }
}