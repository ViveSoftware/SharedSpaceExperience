using UnityEngine;

using Photon.Pun;

namespace SharedSpaceExperience
{
    public class SceneManager : MonoBehaviour
    {

        public Transform networkPlayers;
        public Transform networkObjects;

        public void AddPlayer(Transform obj)
        {
            obj.SetParent(networkPlayers);
        }

        public void AddObject(Transform obj)
        {
            obj.SetParent(networkObjects);
        }

        public void CleanObjects()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SelfDestroy[] objs = networkObjects.GetComponentsInChildren<SelfDestroy>();
                foreach (SelfDestroy obj in objs)
                {
                    obj.DestroyRightNow();
                }
            }
        }

    }
}