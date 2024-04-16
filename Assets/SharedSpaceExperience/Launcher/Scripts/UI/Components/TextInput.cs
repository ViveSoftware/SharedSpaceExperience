using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SharedSpaceExperience.UI
{
    public class TextInput : MonoBehaviour
    {
        [SerializeField]
        private bool clearOnSelect = false;
        public bool isNumeric;
        public bool isIPInput;
        public TMP_InputField inputField;

        [SerializeField]
        public Keyboard keyboard;

        private bool isSelected = false;

        public void OnSelected()
        {
            // prevent select mulitple times
            if (isSelected) return;
            isSelected = true;

            if (clearOnSelect)
            {
                inputField.text = "";
            }
            keyboard.ShowKeyboard(this);
        }

        public void Deselect()
        {
            StartCoroutine(TryDeselect());
        }

        private IEnumerator TryDeselect()
        {
            // delayed deselect
            while (EventSystem.current.currentSelectedGameObject == gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);

                yield return new WaitForSeconds(0.5f);
            }

            isSelected = false;
        }
    }
}