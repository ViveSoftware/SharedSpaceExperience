using UnityEngine;
using TMPro;

namespace SharedSpaceExperience.Example
{
    public class SceneUI : MonoBehaviour
    {
        [SerializeField]
        private AppManager appManager;
        [SerializeField]
        private GameManager gameManager;


        [SerializeField]
        private GameObject menuPanel;

        [SerializeField]
        private GameObject startButton;

        [SerializeField]
        private TMP_Text message;

        [SerializeField]
        private string waitForOthersMessage;
        [SerializeField]
        private string startMessage;
        [SerializeField]
        private string waitForStartMessage;
        [SerializeField]
        private string disconnectedMessage;
        [SerializeField]
        private string notEnoughPlayerMessage;

        private void OnEnable()
        {
            UpdateUI(gameManager.gameState.Value);
            gameManager.OnGameStateChanged += UpdateUI;
            NetworkController.Instance.OnDisconnected += OnDisconnected;
        }

        private void OnDisable()
        {
            gameManager.OnGameStateChanged -= UpdateUI;
            NetworkController.Instance.OnDisconnected -= OnDisconnected;
        }

        public void OnClickStart()
        {
            gameManager.StartGame();
        }

        public void OnClickLeave()
        {
            appManager.LeaveApp();
        }


        private void OnDisconnected(bool selfDisconnect)
        {
            if (!selfDisconnect)
            {
                menuPanel.SetActive(true);
                startButton.SetActive(false);
                message.text = $"<color=#FF0000>{disconnectedMessage}</color>";
            }
        }

        private void UpdateUI(GameManager.GameState gameState)
        {
            switch (gameState)
            {
                case GameManager.GameState.WaitForReady:
                    menuPanel.SetActive(true);
                    startButton.SetActive(false);
                    message.text = waitForOthersMessage;
                    break;
                case GameManager.GameState.WaitForStart:
                    menuPanel.SetActive(true);
                    startButton.SetActive(gameManager.IsServer);
                    message.text = gameManager.IsServer ? startMessage : waitForStartMessage;
                    break;
                case GameManager.GameState.Error:
                    menuPanel.SetActive(true);
                    startButton.SetActive(false);
                    message.text = $"<color=#FF0000>{notEnoughPlayerMessage}</color>";
                    break;
                default:
                    menuPanel.SetActive(false);
                    break;
            }
        }
    }
}
