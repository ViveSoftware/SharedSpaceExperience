using UnityEngine;

namespace SharedSpaceExperience.UI
{
    public class NetworkDiscoveryToggle : MonoBehaviour
    {
        [SerializeField]
        private SwapSpriteToggle toggle;

        private void OnEnable()
        {
            OnValueChanged(true);
        }

        public void OnValueChanged(bool value)
        {
            if (value) NetworkController.Instance.StartDiscovery();
            else NetworkController.Instance.StopDiscovery();

            toggle.OnStateChanged(NetworkController.Instance.isDiscovering);
        }
    }
}