using UnityEngine;
using Unity.Netcode;

namespace SharedSpaceExperience
{
    public class AppUser : NetworkBehaviour
    {
        private UserProperty userProp;

        [SerializeField]
        private Transform head;
        [SerializeField]
        private Transform rightController;
        [SerializeField]
        private Transform leftController;

        public override void OnNetworkSpawn()
        {
            // get user prop
            userProp = UserManager.UserProperties[OwnerClientId];
        }

        private void Update()
        {
            // copy pose from user
            if (IsOwner)
            {
                if (userProp == null) return;
                if (head != null)
                {
                    head.SetPositionAndRotation(
                        userProp.head.position,
                        userProp.head.rotation
                    );
                }

                if (rightController != null)
                {
                    rightController.SetPositionAndRotation(
                        userProp.rightController.position,
                        userProp.rightController.rotation
                    );
                }

                if (leftController != null)
                {
                    leftController.SetPositionAndRotation(
                        userProp.leftController.position,
                        userProp.leftController.rotation
                    );
                }
            }
        }
    }
}
