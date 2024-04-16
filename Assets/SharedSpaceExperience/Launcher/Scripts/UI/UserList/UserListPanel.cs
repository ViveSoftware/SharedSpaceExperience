using UnityEngine;
using Unity.Netcode;

namespace SharedSpaceExperience.UI
{
    public class UserListPanel : MonoBehaviour
    {
        [SerializeField]
        private Transform userList;
        [SerializeField]
        private GameObject userItemPrefab;

        [SerializeField]
        private GameObject networkDiscoveryToggle;

        private void OnEnable()
        {
            // initialize list
            foreach (ulong uid in UserManager.UserProperties.Keys)
            {
                AddUserItem(uid);
            }

            // register callbacks
            UserManager.OnUserConnected += AddUserItem;
            UserManager.OnUserDisconnected += OnClientDisconnectCallback;

            // only show toggle for host
            if (networkDiscoveryToggle != null)
            {
                networkDiscoveryToggle.SetActive(NetworkController.Instance.isServer);
            }
        }

        private void OnDisable()
        {
            // deregister callbacks
            if (NetworkManager.Singleton != null)
            {
                UserManager.OnUserConnected -= AddUserItem;
                UserManager.OnUserDisconnected -= OnClientDisconnectCallback;
            }

            // clean list
            foreach (UserListItem item in GetComponentsInChildren<UserListItem>())
            {
                Destroy(item.gameObject);
            }
        }

        private void OnClientDisconnectCallback(ulong uid)
        {
            // Remove user item
            foreach (UserListItem item in GetComponentsInChildren<UserListItem>())
            {
                if (item.property.OwnerClientId == uid)
                {
                    Destroy(item.gameObject);
                    break;
                }
            }
        }

        private void AddUserItem(ulong uid)
        {
            // check if exists
            foreach (UserListItem userItem in GetComponentsInChildren<UserListItem>())
            {
                if (userItem.property.OwnerClientId == uid) return;
            }

            // create new item
            UserListItem item = Instantiate(userItemPrefab, userList).GetComponent<UserListItem>();
            item.Init(UserManager.UserProperties[uid]);

            // sort user list
            // 1. host
            // 2. local
            // 3. rest
            if (uid == NetworkManager.Singleton.LocalClientId)
            {
                item.transform.SetSiblingIndex(NetworkController.Instance.isServer ? 0 : 1);
            }
            else if (item.property.isHost.Value)
            {
                item.transform.SetSiblingIndex(0);
            }
            else
            {
                item.transform.SetAsLastSibling();
            }
        }
    }
}