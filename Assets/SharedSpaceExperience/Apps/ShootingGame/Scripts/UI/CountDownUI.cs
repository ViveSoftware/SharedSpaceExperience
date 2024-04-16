using UnityEngine;
using TMPro;

namespace SharedSpaceExperience.Example
{
    public class CountDownUI : MonoBehaviour
    {
        [SerializeField]
        private GameManager gameManager;

        [SerializeField]
        private CountDown countDown;

        [SerializeField]
        private TMP_Text text;

        private int value;

        private void OnEnable()
        {
            text.text = gameManager.GAME_DURATION.ToString();
        }

        void Update()
        {
            if (gameManager.gameState.Value == GameManager.GameState.Playing)
            {
                int current = (int)countDown.GetCounterValue(0, gameManager.GAME_DURATION);
                if (current != value)
                {
                    value = current;
                    text.text = value.ToString();
                }
            }
        }
    }
}
