using UnityEngine;

namespace SharedSpaceExperience
{
    public class ConnectionController : MonoBehaviour
    {
        [SerializeField]
        private string declineReason = "Server has started the application";

        private void OnEnable()
        {
            // make sure enable ConnectionApproval
            // NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;

            AllowConnection();
        }

        public void AllowConnection()
        {
            if (!NetworkController.Instance.isServer) return;

            // enable connection
            NetworkController.Instance.SetApprovalCheck(true);

            // enable network discovery
            NetworkController.Instance.StartDiscovery();
        }

        public void DeclineConnection()
        {
            if (!NetworkController.Instance.isServer) return;

            // decline connection
            NetworkController.Instance.SetApprovalCheck(false, declineReason);

            // stop network discovery
            NetworkController.Instance.StopDiscovery();
        }

    }
}