using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SharedSpaceExperience
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField]
        MatchManager matchManager;

        [SerializeField]
        private TMP_Text countDownText;
        [SerializeField]
        private Animator damagedAnimator;
        [SerializeField]
        private Image[] pointHolder;
        [SerializeField]
        private Sprite[] pointImages; // < n: player point, == n: draw point
        [SerializeField]
        private Image headerBackground;
        [SerializeField]
        private Sprite[] headerBackgrounds;

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        private void Update()
        {
            if (matchManager.matchState == MatchState.PLAYING)
            {
                UpdateCountDown();
            }
        }

        private void UpdateCountDown()
        {
            countDownText.text = ((int)Mathf.Ceil((float)matchManager.GetCurrentCountDown())).ToString();
        }

        public void SetRole()
        {
            headerBackground.sprite = headerBackgrounds[PlayerManager.GetLocalPlayerRole()];
        }

        public void OnDamaged()
        {
            damagedAnimator.Play("Damaged", -1, 0);
        }

        public void UpdateBillboard(int round, int winner)
        {
            pointHolder[round].sprite = pointImages[winner < 0 ? MatchManager.NUM_PLAYERS : winner];
        }
    }
}
