using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Collections;

namespace SharedSpaceExperience.UI
{
    public class UserListItem : MonoBehaviour
    {
        [HideInInspector]
        public UserProperty property;

        [SerializeField]
        private TMP_Text userName;
        [SerializeField]
        private TMP_Text status;
        [SerializeField]
        private Image readyIcon;

        [SerializeField]
        private Sprite readySprite;
        [SerializeField]
        private Sprite notReadySprite;
        [SerializeField]
        private Color alignedColor;
        [SerializeField]
        private Color notAlignedColor;

        public void Init(UserProperty prop)
        {
            property = prop;

            // register network value callback
            property.userName.OnValueChanged += OnNameChanged;
            property.ip.OnValueChanged += OnNameChanged;
            property.isAligned.OnValueChanged += OnStatusChanged;
            property.isReady.OnValueChanged += OnStatusChanged;

            // init UI
            UpdateUserName();
            UpdateStatus();
        }

        private void OnDestroy()
        {
            // deregister network value callback
            property.userName.OnValueChanged -= OnNameChanged;
            property.ip.OnValueChanged -= OnNameChanged;
            property.isAligned.OnValueChanged -= OnStatusChanged;
            property.isReady.OnValueChanged -= OnStatusChanged;
        }

        private void OnNameChanged(FixedString32Bytes previous, FixedString32Bytes current)
        {
            UpdateUserName();
        }

        private void OnStatusChanged(bool previous, bool current)
        {
            UpdateStatus();
        }

        private void UpdateUserName()
        {
            userName.text = property.userName.Value.ToString();
            if (property.isHost.Value) userName.text += " <color=#8B8B8B>(Host)</color>";
            if (property.IsOwner) userName.text += " <color=#8B8B8B>(You)</color>";
            userName.text += $"\n({property.ip.Value})";
        }

        private void UpdateStatus()
        {
            if (property.isAligned.Value)
            {
                if (property.isReady.Value)
                {
                    status.text = "Ready";
                    status.color = alignedColor;
                    readyIcon.sprite = readySprite;
                }
                else
                {
                    status.text = "Aligned";
                    status.color = alignedColor;
                    readyIcon.sprite = notReadySprite;
                }
            }
            else
            {
                status.text = "Connected";
                status.color = notAlignedColor;
                readyIcon.sprite = notReadySprite;
            }
        }
    }
}