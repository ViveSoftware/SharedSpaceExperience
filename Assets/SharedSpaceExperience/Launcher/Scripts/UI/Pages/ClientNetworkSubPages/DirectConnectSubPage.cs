using UnityEngine;
using TMPro;

namespace SharedSpaceExperience.UI
{
    public class DirectConnectSubPage : MonoBehaviour
    {
        [SerializeField]
        private ClientNetworkPage page;

        [SerializeField]
        private TMP_InputField ipInput;


        private void OnEnable()
        {
            // load last successfully connected server from local storage
            ipInput.text = UserManager.Instance.localUserProperty.lastConnectedServer;

            page.UpdateHostInfo(ipInput.text);
        }

        public void OnIPInputChanged(string ip)
        {
            page.UpdateHostInfo(ip);
        }

        public void SetInteractable(bool interactable)
        {
            ipInput.interactable = interactable;
        }

    }
}