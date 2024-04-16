using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace SharedSpaceExperience.UI
{
    public class SwapSpriteToggle : MonoBehaviour
    {
        [SerializeField]
        private Button button;
        [SerializeField]
        private Image image;
        [SerializeField]
        private TMP_Text text;

        [SerializeField]
        private bool isOn; // local state
        [SerializeField]
        private bool changeLocalStateImmediately;

        [SerializeField]
        private Sprite onSprite;

        [SerializeField]
        private Sprite offSprite;

        [SerializeField]
        private string onText;
        [SerializeField]
        private string offText;

        public UnityEvent<bool> OnValueChanged;

        private void OnEnable()
        {
            UpdateAppearance();
            button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            // lock button
            button.interactable = false;

            // change local state immediately
            isOn = !isOn;
            if (changeLocalStateImmediately) UpdateAppearance();

            // send user input event to model
            OnValueChanged?.Invoke(isOn);
        }

        public void OnStateChanged(bool state)
        {
            // update local state based on model
            isOn = state;
            UpdateAppearance();

            // unlock button
            button.interactable = true;
        }

        private void UpdateAppearance()
        {
            if (image != null) image.sprite = isOn ? onSprite : offSprite;
            if (text != null) text.text = isOn ? onText : offText;
        }

        public bool IsOn()
        {
            return isOn;
        }

        public void SetInteractable(bool interactable)
        {
            button.interactable = interactable;
        }
    }
}