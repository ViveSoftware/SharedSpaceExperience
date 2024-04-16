using UnityEngine;
using UnityEngine.UI;
using Wave.Native;
using Wave.Essence.TrackableMarker;
using TMPro;

using Logger = Debugger.Logger;

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
        private Button button;

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

            Logger.Log("new marker: " + MarkerUtils.MarkerToLog(data));
        }

        public void UpdateMarker(WVR_ArucoMarker arucoMarker)
        {
            if (data.uuid != arucoMarker.uuid) return;

            if (data.size != arucoMarker.size)
            {
                SetSize(arucoMarker.size);
            }
            if (data.pose != arucoMarker.pose)
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

            Logger.Log($"{data.trackerId} size: {size:F2}");
        }

        private void SetPose(WVR_Pose_t pose)
        {
            // parse pose
            trackableMarkerController.ApplyTrackingOriginCorrectionToMarkerPose(
                pose, out Vector3 position, out Quaternion rotation
            );

            // update pose
            transform.SetPositionAndRotation(position, rotation);
            Logger.Log($"{data.trackerId} position: {position:F2}, rotation: {rotation:F2}");

            data.pose = pose;

            // inform marker manager
            if (markerManager.selectedMarker == this)
            {
                markerManager.OnSelectedMarkerUpdated?.Invoke();
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

        public void SetInteractable(bool interactable)
        {
            button.interactable = interactable;
        }

        public void OnSelected()
        {
            // set selected marker
            markerManager.selectedMarker = this;
            markerManager.OnSelectedMarkerUpdated?.Invoke();
        }
    }
}