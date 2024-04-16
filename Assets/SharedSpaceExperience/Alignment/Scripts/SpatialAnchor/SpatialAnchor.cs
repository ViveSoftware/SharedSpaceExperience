using UnityEngine;

namespace SharedSpaceExperience
{
    public class SpatialAnchor : MonoBehaviour
    {
        [SerializeField]
        private ulong uuid;
        [SerializeField]
        private string anchorName;


        [SerializeField]
        private GameObject model;

        private void OnEnable()
        {
            ShowAnchor(false);
        }

        public ulong GetAnchorUUID()
        {
            return uuid;
        }
        public string GetAnchorName()
        {
            return anchorName;
        }

        public void SetAnchorInfo(ulong uuid, string name)
        {
            this.uuid = uuid;
            anchorName = name;
        }

        public void SetPose(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
        }

        public void ShowAnchor(bool show)
        {
            if (model == null) return;
            model.SetActive(show);
        }
    }
}