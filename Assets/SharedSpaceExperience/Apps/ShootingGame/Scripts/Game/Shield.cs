using UnityEngine;
using Unity.Netcode;

namespace SharedSpaceExperience.Example
{
    public class Shield : NetworkBehaviour
    {
        [SerializeField]
        private PlayerProperty player;
        [SerializeField]
        private GameObject hitBox;
        [SerializeField]
        private ModelStyle model;

        // maintained only by server
        [SerializeField]
        private bool isActive = false;
        [SerializeField]
        private NetworkVariable<bool> isVisible = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            isVisible.OnValueChanged += OnVisibilityChanged;
        }

        public override void OnNetworkDespawn()
        {
            isVisible.OnValueChanged -= OnVisibilityChanged;
        }


        /* Visibility */
        public void UpdateVisibility()
        {
            if (!IsServer) return;
            isVisible.Value = player.isVisible;
        }
        public void OnVisibilityChanged(bool previous, bool current)
        {
            model.SetVisible(isVisible.Value);
        }


        /* Ability */
        public void UpdateAbilityActive()
        {
            if (!IsServer) return;

            // combine with condition in this level
            isActive = player.isAbilityActive;
            hitBox.SetActive(isActive);
        }

    }
}
