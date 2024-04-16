using UnityEngine;
using UnityEngine.UI;

namespace SharedSpaceExperience.UI
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleController : MonoBehaviour
    {
        private Toggle toggle;

        [SerializeField]
        private Image image;

        [SerializeField]
        private Sprite onSprite;

        [SerializeField]
        private Sprite offSprite;

        private void Awake()
        {
            toggle = GetComponent<Toggle>();
            offSprite = image.sprite;
        }
        private void OnEnable()
        {
            toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(bool value)
        {
            if (onSprite != null)
            {
                image.sprite = value ? onSprite : offSprite;
            }
        }
    }
}
