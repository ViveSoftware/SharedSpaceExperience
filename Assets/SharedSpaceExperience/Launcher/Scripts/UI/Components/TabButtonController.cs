using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SharedSpaceExperience.UI
{
    public class TabButtonController : MonoBehaviour
    {
        [SerializeField]
        private Toggle toggle;
        [SerializeField]
        private TMP_Text text;

        [SerializeField]
        private GameObject tabPage;

        private void OnEnable()
        {
            OnValueChanged(toggle.isOn);
        }

        public void OnValueChanged(bool value)
        {
            // mark under line if select
            if (value)
            {
                text.fontStyle |= FontStyles.Underline;
            }
            else
            {
                text.fontStyle &= ~FontStyles.Underline;
            }

            tabPage.SetActive(value);
        }

        public void SetInteractable(bool interactable)
        {
            toggle.interactable = interactable;
        }

        public bool IsOn()
        {
            return toggle.isOn;
        }
    }
}