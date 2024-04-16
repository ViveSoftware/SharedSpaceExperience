using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SharedSpaceExperience.UI
{
    public class UserListHandPanel : MonoBehaviour
    {
        [SerializeField]
        private bool show = false;
        [SerializeField]
        private bool canShow = false;

        [SerializeField]
        private GameObject userListCanvas;

        [SerializeField]
        private InputAction showUserListAction;

        private void OnEnable()
        {
            // enable input action
            showUserListAction.Enable();
            showUserListAction.started += ShowUserList;

            StartCoroutine(WaitForManagers());
        }

        private IEnumerator WaitForManagers()
        {
            // register callbacks
            yield return new WaitUntil(() => NetworkController.Instance);
            canShow = NetworkController.Instance.isConnected;
            NetworkController.Instance.OnConnected += OnConnected;
            NetworkController.Instance.OnDisconnected += OnDisconnected;

            userListCanvas.SetActive(show && canShow);
        }

        private void OnDisable()
        {
            // disable input action
            showUserListAction.started -= ShowUserList;
            showUserListAction.Disable();

            // deregister callbacks
            if (NetworkController.Instance != null)
            {
                NetworkController.Instance.OnConnected -= OnConnected;
                NetworkController.Instance.OnDisconnected -= OnDisconnected;
            }
        }

        private void ShowUserList(InputAction.CallbackContext context)
        {
            show = !show;
            userListCanvas.SetActive(show && canShow);
        }

        private void OnConnected()
        {
            canShow = true;
            userListCanvas.SetActive(show && canShow);
        }

        private void OnDisconnected(bool selfDisconnect)
        {
            canShow = false;
            userListCanvas.SetActive(false);
        }
    }
}