using UnityEngine;

using Photon.Pun;

namespace SharedSpaceExperience
{

    public class BulletPreview : MonoBehaviourPun
    {
        [SerializeField]
        private ParticleSystem particle;

        [SerializeField]
        private BulletModel model;

        private bool isActive = false;
        private float prevSize = 1;

        public void SetStyle(int i)
        {
            model.SetStyle(i);
        }

        public void SetPreviewSize(float size)
        {
            if (photonView.IsMine && !prevSize.Equals(size))
            {
                prevSize = size;
                transform.localScale = size * Vector3.one;
            }
        }

        public void SetPreviewActive(bool active)
        {
            if (photonView.IsMine && isActive != active)
            {
                photonView.RPC("SetBulletPreviewActiveRPC", RpcTarget.All, active);
            }
        }

        [PunRPC]
        public void SetBulletPreviewActiveRPC(bool active)
        {
            isActive = active;
            if (isActive)
            {
                particle.gameObject.SetActive(true);
                particle.Play(true);
            }
            else
            {
                particle.gameObject.SetActive(false);
                // particle.Stop(true);
            }
        }

    }
}