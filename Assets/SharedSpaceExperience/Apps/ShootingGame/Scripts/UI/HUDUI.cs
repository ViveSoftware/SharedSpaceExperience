using UnityEngine;
using Unity.Netcode;

namespace SharedSpaceExperience.Example
{
    public class HUDUI : MonoBehaviour
    {
        [SerializeField]
        private GameManager gameManager;

        [SerializeField]
        private PlayerManager playerManager;

        [SerializeField]
        private GameObject gamePanel;
        [SerializeField]
        private VideoController videoController;
        [SerializeField]
        private Animator damagedAnimator;

        private Transform head;

        [SerializeField]
        private float distanceToHead;

        private void OnEnable()
        {
            head = UserManager.Instance.headSource;

            gameManager.OnGameStateChanged += UpdateUI;
            playerManager.OnLocalPlayerDamaged += OnDamaged;
        }

        private void OnDisable()
        {
            gameManager.OnGameStateChanged -= UpdateUI;
            playerManager.OnLocalPlayerDamaged -= OnDamaged;
        }

        private void Update()
        {
            transform.SetPositionAndRotation(
                distanceToHead * head.forward + head.position,
                head.rotation
            );
        }

        public void PlayStartVideo(double expectEndTime)
        {
            videoController.PlayStartVideo(expectEndTime);
        }

        public void PlayEndingVideo(long winner)
        {
            // videoController.StopVideo();
            if (winner == (long)NetworkManager.Singleton.LocalClientId)
            {
                videoController.PlayWinVideo();
            }
            else if (winner == -1 && playerManager.GetLocalPlayer()?.health.Value > 0)
            {
                videoController.PlayDrawVideo();
            }
            else
            {
                videoController.PlayLoseVideo();
            }
            videoController.OnVideoFinished += gameManager.OnEndingAnimationFinished;
        }

        private void OnDamaged()
        {
            damagedAnimator.Play("Damaged", -1, 0);
        }

        private void UpdateUI(GameManager.GameState gameState)
        {
            bool isInGame =
                gameState == GameManager.GameState.Starting ||
                gameState == GameManager.GameState.Playing ||
                gameState == GameManager.GameState.Ending;

            gamePanel.SetActive(isInGame);
        }
    }
}
