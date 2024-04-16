using System.Collections;
using UnityEngine;
using Unity.Netcode;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.Example
{
    public class Bullet : NetworkBehaviour
    {
        [SerializeField]
        private GameObject hitBox;
        [SerializeField]
        private ModelStyle model;
        [SerializeField]
        private Transform trail;

        private bool hasCollided = false;
        private Coroutine destroyCoroutine;

        private const float TRAIL_LIFE_TIME = 2;

        public override void OnNetworkSpawn()
        {
            // set style
            if (PlayerManager.Instance.TryGetPlayer(OwnerClientId, out PlayerProperty player))
            {
                model.SetStyle(player.style.Value);
            }
        }

        public void Init(float lifeTime, float size, Vector3 velocity)
        {
            transform.localScale = size * Vector3.one;
            GetComponent<Rigidbody>().velocity = velocity;

            // self destroy
            if (IsServer)
            {
                destroyCoroutine = StartCoroutine(DelayDestroy(lifeTime));
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Logger.Log("bullet hit: " + other.name);

            // ignore owned objects
            if ((other.gameObject.CompareTag("Health") &&
                 other.GetComponentInParent<NetworkObject>()?.OwnerClientId == OwnerClientId) ||
                (other.gameObject.CompareTag("Shield") &&
                 other.GetComponentInParent<NetworkObject>()?.OwnerClientId == OwnerClientId)) return;

            // disable hitbox
            hitBox.SetActive(false);
            // detach trail
            trail.SetParent(null);

            // stop bullet
            if (IsServer)
            {
                GetComponent<Rigidbody>().isKinematic = true;
                GetComponent<Rigidbody>().position = transform.position;

                if (!hasCollided)
                {
                    hasCollided = true;

                    // stop delayed destroy
                    if (destroyCoroutine != null) StopCoroutine(destroyCoroutine);
                    // destroy right now
                    DestroyAnimationClientRpc();
                }
            }

        }

        private IEnumerator DelayDestroy(float timeout)
        {
            // only called by server
            yield return new WaitForSeconds(timeout);
            DestroyAnimationClientRpc();
        }

        [ClientRpc]
        private void DestroyAnimationClientRpc()
        {
            // detach trail
            trail.SetParent(transform.parent);
            Destroy(trail.gameObject, TRAIL_LIFE_TIME);

            // destroy bullet
            if (IsServer) NetworkObject.Despawn();
        }
    }
}