using UnityEngine;
using Unity.Netcode;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.Example
{
    public class HealthBrick : NetworkBehaviour
    {
        [SerializeField]
        private HealthManager healthManager;

        [SerializeField]
        private GameObject hitBox;
        [SerializeField]
        private ModelStyle model;

        // maintained only by server
        public bool isHealthy = false;
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

        private void OnTriggerEnter(Collider other)
        {
            // check if is hit by other player's bullet
            if (!IsServer || !isActive || !other.gameObject.CompareTag("Bullet") ||
                other.GetComponentInParent<NetworkObject>()?.OwnerClientId == OwnerClientId) return;

            Logger.Log("hit by player: " + other.GetComponentInParent<NetworkObject>()?.OwnerClientId);

            // update health
            isHealthy = false;
            UpdateVisibility();
            UpdateAbilityActive();

            healthManager.UpdateHealth();
        }

        /* Visibility */
        public void UpdateVisibility()
        {
            if (!IsServer) return;

            // combine with condition in this level
            isVisible.Value = healthManager.isVisible && isHealthy;
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
            isActive = healthManager.isAbilityActive && isHealthy && isVisible.Value;
            hitBox.SetActive(isActive);
        }

    }
}