using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace SharedSpaceExperience.UI
{
    public class HostItemController : MonoBehaviour
    {
        private ClientNetworkPage page;

        [SerializeField]
        private Toggle toggle;

        [SerializeField]
        private TMP_Text text;

        private string hostName;
        public string hostIP;

        public void Init(ClientNetworkPage page, ToggleGroup group, string hostName, string hostIP)
        {
            toggle.group = group;

            this.page = page;
            this.hostName = hostName;
            this.hostIP = hostIP;

            text.text = $"{this.hostName} ({this.hostIP})";
        }

        public void OnValueChanged(bool value)
        {
            if (value)
            {
                // on selected
                // update selected host
                page.UpdateHostInfo(hostIP);
            }
        }

        public void SetInteractable(bool interactable)
        {
            toggle.interactable = interactable;
        }
    }
}