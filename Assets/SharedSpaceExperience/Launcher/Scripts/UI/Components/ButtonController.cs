using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SharedSpaceExperience.UI
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class ButtonController : MonoBehaviour
    {
        [SerializeField]
        private Button button;
        [SerializeField]
        private Image image;
        [SerializeField]
        private TMP_Text text;

        [SerializeField]
        private Sprite defaultSprite;
        [SerializeField]
        private Sprite disabledSprite;

        [SerializeField]
        private string defaultText;
        [SerializeField]
        private string disabledText;

        [SerializeField]
        [Tooltip("Disable button after click\nPositive: disabled until timeout\nZero: won't disable\nNegative: disabled until enable")]
        private float disableTimeout = -1;

        private void OnEnable()
        {
            SetInteractable(button.interactable);

            button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(OnClick);
        }

        public void SetInteractable(bool interactable)
        {
            if (interactable) CancelInvoke(nameof(DisableTimeout));

            button.interactable = interactable;
            if (image != null && disabledSprite != null)
            {
                image.sprite = interactable ? defaultSprite : disabledSprite;
            }

            if (text != null) text.text = interactable ? defaultText : disabledText;
        }

        private void OnClick()
        {
            // lock button
            if (disableTimeout != 0) SetInteractable(false);

            // disable timeout
            if (disableTimeout > 0) Invoke(nameof(DisableTimeout), disableTimeout);
        }

        public void OnStateChanged()
        {
            // unlock button
            SetInteractable(true);
        }

        private void DisableTimeout()
        {
            // enable if timeout
            SetInteractable(true);
        }
    }
}