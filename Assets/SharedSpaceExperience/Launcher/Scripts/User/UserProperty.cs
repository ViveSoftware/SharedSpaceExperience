using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience
{
    public class UserProperty : NetworkBehaviour
    {

        [Header("User Properties")]
        public NetworkVariable<FixedString32Bytes> userName = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<FixedString32Bytes> ip = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<bool> isHost = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<bool> isAligned = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<bool> isReady = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        [Header("Tracked Pose")]
        public Transform head;
        public Transform rightController;
        public Transform leftController;

        [Header("User Default Model")]
        [SerializeField]
        private GameObject headModel;
        [SerializeField]
        private GameObject rightControllerModel;
        [SerializeField]
        private GameObject leftControllerModel;

        public override void OnNetworkSpawn()
        {
            // register callbacks
            userName.OnValueChanged += OnNameChanged;
            ip.OnValueChanged += OnIPChanged;
            isAligned.OnValueChanged += OnIsAlignedChanged;
            isReady.OnValueChanged += OnIsReadyChanged;

            // add to user manager
            UserManager.UserProperties.Add(OwnerClientId, this);
            UserManager.OnUserCountChanged?.Invoke();

            if (IsOwner)
            {
                // initialize user properties
                userName.Value = UserManager.Instance.localUserProperty.userName;
                NetworkController networkController = FindObjectOfType<NetworkController>();
                ip.Value = networkController.ip;
                if (IsServer) isHost.Value = true;

                RoomProperty.Instance.OnRealign += OnRealign;

                // invoke callback
                UserManager.OnLocalUserSpawned?.Invoke();
            }
            // update model
            UpdateUserModelVisibility();

            // invoke callback
            UserManager.OnUserConnected?.Invoke(OwnerClientId);

        }

        public override void OnNetworkDespawn()
        {
            // deregister callbacks
            userName.OnValueChanged -= OnNameChanged;
            ip.OnValueChanged -= OnIPChanged;
            isAligned.OnValueChanged -= OnIsAlignedChanged;
            isReady.OnValueChanged -= OnIsReadyChanged;

            if (IsOwner)
            {
                RoomProperty.Instance.OnRealign -= OnRealign;
                UserManager.OnLocalUserDespawned?.Invoke();
            }

            // invoke callback
            UserManager.OnUserDisconnected?.Invoke(OwnerClientId);

            // remove from list
            UserManager.UserProperties.Remove(OwnerClientId);
            UserManager.OnUserCountChanged?.Invoke();
        }

        private void OnNameChanged(FixedString32Bytes previous, FixedString32Bytes current)
        {
            UserManager.OnUserNameChanged?.Invoke(OwnerClientId, current);
        }

        private void OnIPChanged(FixedString32Bytes previous, FixedString32Bytes current)
        {
            UserManager.OnUserIPChanged?.Invoke(OwnerClientId, current);
        }

        private void OnIsAlignedChanged(bool previous, bool current)
        {
            if (IsOwner)
            {
                // update all user models if local aligned changed
                UserManager.Instance.UpdateAllUserModelVisibility();
            }
            else
            {
                // update the user models whose aligned changed
                UpdateUserModelVisibility();
            }

            UserManager.OnUserIsAlignedChanged?.Invoke(OwnerClientId, current);
        }

        private void OnIsReadyChanged(bool previous, bool current)
        {
            UserManager.OnUserIsReadyChanged?.Invoke(OwnerClientId, current);
        }

        private void OnRealign()
        {
            isAligned.Value = false;
        }

        [ServerRpc]
        public void SetIsReadyServerRpc(bool ready)
        {
            isReady.Value = ready;
        }

        public void UpdateUserModelVisibility()
        {
            // block if local user has not spawn
            if (!UserManager.TryGetLocalUser(out UserProperty localUser)) return;

            // determine visibility
            bool show = UserManager.debugShowUserDefaultModel || (
                UserManager.showUserDefaultModel &&
                (UserManager.previewUserDefaultModel || localUser.isAligned.Value) &&
                isAligned.Value
            );

            // set visibility
            headModel.SetActive(show);
            rightControllerModel.SetActive(show);
            leftControllerModel.SetActive(show);

            Logger.Log(
                "Update model visibility:" +
                "\nowner: " + OwnerClientId +
                "\nowner aligned: " + isAligned.Value +
                "\nlocal aligned: " + localUser.isAligned.Value +
                "\ndebug: " + UserManager.debugShowUserDefaultModel +
                "\npreview: " + UserManager.previewUserDefaultModel +
                "\nlogic: " + UserManager.showUserDefaultModel +
                "\nvisible: " + show
            );
        }

        private void Update()
        {
            if (IsOwner)
            {
                // update pose
                if (UserManager.Instance.headSource != null)
                {
                    head.transform.SetPositionAndRotation(
                        UserManager.Instance.headSource.transform.position,
                        UserManager.Instance.headSource.transform.rotation
                    );
                }

                if (UserManager.Instance.rightControllerSource != null)
                {
                    rightController.transform.SetPositionAndRotation(
                        UserManager.Instance.rightControllerSource.transform.position,
                        UserManager.Instance.rightControllerSource.transform.rotation
                    );
                }

                if (UserManager.Instance.leftControllerSource != null)
                {
                    leftController.transform.SetPositionAndRotation(
                        UserManager.Instance.leftControllerSource.transform.position,
                        UserManager.Instance.leftControllerSource.transform.rotation
                    );
                }
            }

        }
    }
}
