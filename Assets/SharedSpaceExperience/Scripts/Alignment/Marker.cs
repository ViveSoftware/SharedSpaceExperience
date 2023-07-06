using UnityEngine;
using Wave.Native;
using Wave.Essence.TrackableMarker;
using TMPro;

namespace SharedSpaceExperience
{
    public class Marker : MonoBehaviour
    {
        public bool exist = true;
        private bool show = true;

        public WVR_ArucoMarker data;
        private MarkerManager markerManager;
        private TrackableMarkerController trackableMarkerController;

        [SerializeField]
        private Transform markerModel;

        [SerializeField]
        private TMP_Text text;
        private const string PREFIX = "Marker ID: ";


        public void Init(MarkerManager manager, WVR_ArucoMarker arucoMarker)
        {
            markerManager = manager;
            trackableMarkerController = markerManager.trackableMarkerController;

            data = arucoMarker;
            SetSize(data.size);
            SetPose(data.pose);
            text.text = PREFIX + data.trackerId;

            Logger.Log("[Marker] new marker: " + MarkerUtils.MarkerToLog(data));
        }

        public void UpdateMarker(WVR_ArucoMarker arucoMarker)
        {
            if (!TrackableMarkerController.IsUUIDEqual(data.uuid, arucoMarker.uuid)) return;

            if (data.size != arucoMarker.size)
            {
                SetSize(arucoMarker.size);
            }
            if (!WVRStructCompare.WVRPoseEqual(data.pose, arucoMarker.pose))
            {
                SetPose(arucoMarker.pose);
            }

            data = arucoMarker;
            text.text = PREFIX + data.trackerId;
        }

        private void SetSize(float size)
        {
            markerModel.localScale = new Vector3(size, size, size);
            data.size = size;

            Logger.Log("[Marker] " + data.trackerId + " size: " + size.ToString("F2"));
        }

        private void SetPose(WVR_Pose_t pose)
        {
            // parse pose
            trackableMarkerController.ApplyTrackingOriginCorrectionToMarkerPose(
                pose, out Vector3 position, out Quaternion rotation
            );

            // update pose
            transform.localPosition = position;
            transform.localRotation = rotation;

            Logger.Log("[Marker] " + data.trackerId + " position: " + position.ToString("F2") + ", rotation: " + rotation.ToString("F2"));

            data.pose = pose;

            // inform marker manager
            if (markerManager.selectedMarker == this)
            {
                markerManager.OnSelectedMarkerUpdated.Invoke();
            }
        }

        public void ShowMarker(bool show)
        {
            if (show != this.show)
            {
                this.show = show;
                markerModel.gameObject.SetActive(this.show);
            }
        }

        public void OnSelected()
        {
            // set selected marker
            markerManager.selectedMarker = this;
            markerManager.OnSelectedMarkerUpdated.Invoke();
        }
    }
}