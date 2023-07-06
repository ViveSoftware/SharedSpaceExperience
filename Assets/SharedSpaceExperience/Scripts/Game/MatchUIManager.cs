using UnityEngine;
using UnityEngine.EventSystems;
using Wave.Essence.InputModule;

using Photon.Pun;

namespace SharedSpaceExperience
{
    public class MatchUIManager : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private MatchManager matchManager;

        [Header("UI Controllers")]
        [SerializeField]
        private GameObject sceneCanvas;
        [SerializeField]
        private HostInitUI hostInitUI;
        [SerializeField]
        private AlignUI alignUI;
        [SerializeField]
        private WaitingUI waitingUI;
        [SerializeField]
        private HUDController hudController;
        [SerializeField]
        VideoController videoController;
        [SerializeField]
        private GameObject errorPanel;

        // controller model
        [Header("Controller Model")]
        [SerializeField]
        private GameObject[] controllers;
        [SerializeField]
        private PhysicsRaycaster[] raycasters;
        [SerializeField]
        private ControllerInputModule controllerInputModule;


        public void ShowInitUI()
        {
            sceneCanvas.SetActive(true);

            hostInitUI.SetActive(PhotonNetwork.IsMasterClient);
            waitingUI.SetActive(!PhotonNetwork.IsMasterClient);
            hudController.SetActive(false);
            errorPanel.SetActive(false);
        }

        public void ShowAlignUI()
        {
            bool isLocalPlayerReady = PlayerManager.IsLocalPlayerReady();

            sceneCanvas.SetActive(true);

            hostInitUI.SetActive(false);
            waitingUI.SetActive(isLocalPlayerReady);
            alignUI.SetActive(!isLocalPlayerReady);
            hudController.SetActive(false);
            errorPanel.SetActive(false);
        }

        public void RoundStart(double clipEndTime)
        {
            // update playing UI depend on player role
            hudController.SetRole();

            sceneCanvas.SetActive(false);

            waitingUI.SetActive(false);
            hudController.SetActive(true);
            errorPanel.SetActive(false);

            // play start video
            videoController.PlayStartVideo(clipEndTime);
        }

        public void RoundEnd(int winner)
        {
            sceneCanvas.SetActive(true);

            waitingUI.SetActive(false);
            hudController.SetActive(true);
            errorPanel.SetActive(false);

            if (MatchManager.MAX_ROUND == 1)
            {
                // skip round end animation if there is only one round
                PlayerManager.SetLocalPlayerReady(true);
            }
            else
            {
                // play round end animation
                videoController.PlayRoundEndVideo(winner, () =>
                {
                    // update billboard
                    int round = matchManager.roundResult.Length - 1;
                    hudController.UpdateBillboard(round, winner);

                    // set ready
                    PlayerManager.SetLocalPlayerReady(true);
                });
            }

        }

        public void ShowEnding(int result)
        {
            sceneCanvas.SetActive(false);

            waitingUI.SetActive(false);
            hudController.SetActive(true);
            errorPanel.SetActive(false);

            // play round end animation
            videoController.PlayMatchEndVideo(result, () =>
                {
                    // return to lobby
                    matchManager.ReturnToLobby();
                }
            );
        }

        public void ShowErrorUI()
        {
            sceneCanvas.SetActive(true);

            waitingUI.SetActive(false);
            errorPanel.SetActive(true);
        }


        public void OnDamaged()
        {
            hudController.OnDamaged();
        }

        public void OnClickedReturnButton()
        {
            Logger.Log("[MatchUI] return");
            matchManager.ReturnToLobby();
        }

        public void ShowControllers(bool show)
        {
            // show/hide controller modes
            foreach (GameObject controller in controllers)
            {
                controller.SetActive(show);
            }

            // enable/disable raycasters
            controllerInputModule.enabled = show;

        }

    }
}
