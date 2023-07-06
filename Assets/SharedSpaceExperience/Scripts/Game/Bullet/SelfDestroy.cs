using System.Collections;
using UnityEngine;

using Photon.Pun;

namespace SharedSpaceExperience
{
    public class SelfDestroy : MonoBehaviourPun
    {
        protected Coroutine destroyCoroutine;

        public void SetTimeout(float timeout = 1)
        {
            destroyCoroutine = StartCoroutine(DelayDestroy(timeout));
        }

        public void DestroyRightNow()
        {
            StopCoroutine(destroyCoroutine);
            PhotonNetwork.Destroy(gameObject);
        }

        IEnumerator DelayDestroy(float timeout)
        {
            yield return new WaitForSeconds(timeout);

            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
