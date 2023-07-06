using UnityEngine;

using Photon.Pun;

namespace SharedSpaceExperience
{
    public class Bullet : SelfDestroy, IPunInstantiateMagicCallback
    {
        public int ownerID = -1;
        public bool hasCollided = false;

        [SerializeField]
        private GameObject hitBox;
        [SerializeField]
        private Transform trail;
        [SerializeField]
        private BulletModel model;

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            ownerID = (int)info.photonView.InstantiationData[0];
            transform.localScale = (float)info.photonView.InstantiationData[1] * Vector3.one;
            if (ownerID >= 0) model.SetStyle(ownerID);

            // add to scene
            SceneManager sceneManager = GameObject.FindObjectOfType<SceneManager>();
            if (sceneManager != null)
            {
                sceneManager.AddObject(transform);
            }

        }

        private void OnTriggerEnter(Collider other)
        {
            Logger.Log("[Bullet] hit " + other.name);
            // ignore owned objects
            if ((other.gameObject.tag == "Health" &&
                 other.GetComponentInParent<HealthBrick>()?.ownerID == ownerID) ||
                (other.gameObject.tag == "Shield" &&
                 other.GetComponentInParent<Shield>()?.ownerID == ownerID)) return;

            // disable hitbox
            hitBox.SetActive(false);
            // detach trail
            trail.SetParent(null);

            // stop bullet
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.velocity = Vector3.zero;
            rigidbody.position = transform.position;

            if (photonView.IsMine && !hasCollided)
            {
                hasCollided = true;
                Debug.Log("[Bullet] collide " + other.name);

                // stop timer
                if (destroyCoroutine != null) StopCoroutine(destroyCoroutine);

                photonView.RPC("OnHitRPC", RpcTarget.All);
            }
        }

        [PunRPC]
        private void OnHitRPC()
        {
            // detach trail
            trail.SetParent(transform.parent);
            Destroy(trail.gameObject, 2);

            if (photonView.IsMine)
            {
                // self destroy
                SetTimeout(0.1f);
            }
        }

    }
}