using UnityEngine;
using Photon.Pun;

namespace SharedSpaceExperience
{
    public class SyncPose : MonoBehaviour
    {

        // tracked object source
        private Transform headSource;
        private Transform leftControllerSource;
        private Transform rightControllerSource;

        // target object to apply pose to
        [SerializeField]
        private Transform head;
        [SerializeField]
        private Transform leftController;
        [SerializeField]
        private Transform rightController;

        private PhotonView photonView;

        void Start()
        {
            photonView = GetComponent<PhotonView>();
        }

        public void SetSources(Transform head_in, Transform leftCR_in, Transform rightCR_in)
        {
            headSource = head_in;
            leftControllerSource = leftCR_in;
            rightControllerSource = rightCR_in;
        }

        void Update()
        {
            if (photonView.IsMine)
            {
                // update pose
                UpdatePose(head, headSource);
                UpdatePose(leftController, leftControllerSource);
                UpdatePose(rightController, rightControllerSource);
            }
        }

        void UpdatePose(Transform target, Transform source)
        {
            target.position = source.position;
            target.rotation = source.rotation;
        }
    }
}