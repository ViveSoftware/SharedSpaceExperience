using UnityEngine;
using Unity.Netcode;

namespace SharedSpaceExperience.Example
{
    public class BulletPreview : NetworkBehaviour
    {
        [SerializeField]
        private ModelStyle model;
        private NetworkVariable<bool> isVisible = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private float size = 1;

        public override void OnNetworkSpawn()
        {
            isVisible.OnValueChanged += OnVisibilityChanged;
        }

        public override void OnNetworkDespawn()
        {
            isVisible.OnValueChanged -= OnVisibilityChanged;
        }

        public void SetSize(float size)
        {
            if (!IsOwner || this.size == size) return;

            this.size = size;
            transform.localScale = this.size * Vector3.one;
        }


        /* Visibility */
        public void UpdateVisibility(bool visible)
        {
            if (!IsOwner) return;
            if (isVisible.Value != visible) isVisible.Value = visible;
        }

        public void OnVisibilityChanged(bool previous, bool current)
        {
            model.SetVisible(isVisible.Value);
        }
    }
}
