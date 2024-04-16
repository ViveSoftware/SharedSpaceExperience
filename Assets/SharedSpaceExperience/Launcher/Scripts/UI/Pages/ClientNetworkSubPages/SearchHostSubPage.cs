using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.UI
{
    public class SearchHostSubPage : MonoBehaviour
    {
        [SerializeField]
        private ClientNetworkPage page;

        [SerializeField]
        private Transform hostList;
        [SerializeField]
        private ToggleGroup hostsToggleGroup;
        [SerializeField]
        private GameObject hostPrefab;

        [SerializeField]
        private ButtonController refreshButton;

        private bool interactable = true;

        private void OnEnable()
        {
            interactable = true;
            HostDiscrovery.OnServerFound += OnServerFound;
            NetworkController.Instance.StartDiscovery();
            OnClickRefresh();
        }

        private void OnDisable()
        {
            HostDiscrovery.OnServerFound -= OnServerFound;
            if (NetworkController.Instance != null)
            {
                NetworkController.Instance.StopDiscovery();
            }
        }

        public void OnClickRefresh()
        {
            // clear list
            foreach (Transform child in hostList)
            {
                Destroy(child.gameObject);
            }

            // clear selected host
            page.UpdateHostInfo("");

            // search hosts
            NetworkController.Instance.SearchHost();
        }

        public void OnServerFound(string serverName, string serverIP, ushort serverPort)
        {
            // check if server already found
            foreach (HostItemController child in hostList.GetComponentsInChildren<HostItemController>())
            {
                if (child.hostIP == serverIP)
                {
                    Logger.LogWarning($"Receive duplicated server response: {serverIP}");
                    return;
                }
            }

            // create new host item
            GameObject host = Instantiate(hostPrefab, hostList);
            HostItemController hostItem = host.GetComponent<HostItemController>();

            hostItem.Init(page, hostsToggleGroup, serverName, serverIP);
            hostItem.SetInteractable(interactable);

            // update selected host
            hostsToggleGroup.ActiveToggles().FirstOrDefault().GetComponent<HostItemController>().OnValueChanged(true);
        }

        public void SetInteractable(bool interactable)
        {
            this.interactable = interactable;

            // set refresh button
            refreshButton.SetInteractable(interactable);

            // set host items
            foreach (HostItemController child in hostList.GetComponentsInChildren<HostItemController>())
            {
                child.SetInteractable(interactable);
            }
        }
    }
}