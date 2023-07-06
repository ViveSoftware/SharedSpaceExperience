using UnityEngine;

using Photon.Pun;

namespace SharedSpaceExperience
{
    public class Shield : MonoBehaviourPun
    {
        public int ownerID = -1;

        [SerializeField]
        private ShieldModel model;

        private bool isActive = false;

        public void SetStyle(int i)
        {
            model.SetStyle(i);
        }

        public void SetActive(bool active)
        {
            if (photonView.IsMine && isActive != active)
            {
                photonView.RPC("SetShieldActiveRPC", RpcTarget.All, active);
            }
        }

        [PunRPC]
        public void SetShieldActiveRPC(bool active)
        {
            isActive = active;
            gameObject.SetActive(isActive);
        }

    }
}