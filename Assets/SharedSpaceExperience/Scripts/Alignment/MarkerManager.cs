using System.Collections.Generic;
using UnityEngine;
using Wave.Native;
using Wave.Essence.TrackableMarker;
using UnityEngine.Events;

namespace SharedSpaceExperience
{
    public class MarkerManager : MonoBehaviour
    {
        private bool isMarkerServiceRunning = false;
        private bool isMarkerObserverRunning = false;
        private bool isPaused = false;

        public TrackableMarkerController trackableMarkerController;

        [SerializeField]
        private Transform markersParent;

        [SerializeField]
        private GameObject markerPrefab;

        private WVR_MarkerObserverState observerState;
        private const WVR_MarkerObserverTarget OBSERVER_TARGET = WVR_MarkerObserverTarget.WVR_MarkerObserverTarget_Aruco;

        private Dictionary<string, Marker> markers = new Dictionary<string, Marker>();
        public Marker selectedMarker = null;
        public UnityEvent OnSelectedMarkerUpdated;

        // target marker
        private bool filter = false;
        private WVR_ArucoMarker targetMarker;

        private void OnDisable()
        {
            StopMarkerDetection();
        }

        void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                if (isPaused)
                {
                    Debug.Log("[MarkerManager]: resume detection");
                    StartMarkerDetection();
                    isPaused = false;
                }
            }
            else
            {
                if (isMarkerServiceRunning)
                {
                    StopMarkerDetection();
                    isPaused = true;
                    Debug.Log("[MarkerManager]: paused detection");
                }
            }
        }

        public bool StartMarkerDetection()
        {
            // start marker service
            if ((Interop.WVR_GetSupportedFeatures() &
                 (ulong)WVR_SupportedFeature.WVR_SupportedFeature_Marker) != 0)
            {
                WVR_Result result = trackableMarkerController.StartMarkerService();
                isMarkerServiceRunning = (result == WVR_Result.WVR_Success);
            }
            // start marker observer
            if (isMarkerServiceRunning && !isMarkerObserverRunning)
            {
                WVR_Result result = trackableMarkerController.StartMarkerObserver(
                    OBSERVER_TARGET
                );
                isMarkerObserverRunning = result == WVR_Result.WVR_Success;
            }
            // start marker detection
            if (isMarkerObserverRunning)
            {
                // load existing markers
                LoadExistingMarkers();

                // start detection
                WVR_Result result = trackableMarkerController.StartMarkerDetection(
                    OBSERVER_TARGET
                );

                if (result == WVR_Result.WVR_Success)
                {
                    Logger.Log("[MarkerManager] start detect marker");
                    return true;
                }
            }
            return false;
        }

        public void StopMarkerDetection()
        {
            // stop detection
            trackableMarkerController.GetMarkerObserverState(OBSERVER_TARGET, out observerState);
            if (observerState == WVR_MarkerObserverState.WVR_MarkerObserverState_Detecting)
            {
                WVR_Result result = trackableMarkerController.StopMarkerDetection(
                    OBSERVER_TARGET
                );

                if (result == WVR_Result.WVR_Success)
                {
                    Logger.Log("[MarkerManager] stop detect marker");
                }
            }

            // stop marker observer
            if (isMarkerServiceRunning && isMarkerObserverRunning)
            {
                // save markers
                SaveMarkers();

                WVR_Result result = trackableMarkerController.StopMarkerObserver(
                    OBSERVER_TARGET
                );
                isMarkerObserverRunning = result == WVR_Result.WVR_Success;
            }

            // stop marker service
            if (isMarkerServiceRunning)
            {
                trackableMarkerController.StopMarkerService();
                isMarkerServiceRunning = false;
                Logger.Log("[MarkerManager] stop marker service");
            }
        }

        public void LoadExistingMarkers()
        {
            // To make the Aruco marker be detectable in the detection mode
            // we have to destroy the binding with trackable marker

            // retrieve exist trackable markers
            WVR_Result result = trackableMarkerController.GetTrackableMarkers(
                OBSERVER_TARGET, out WVR_Uuid[] trackableMarkerIdArray);

            // destroy trackable markers so the Aruco marker can be updated in detection mode
            if (result == WVR_Result.WVR_Success)
            {
                foreach (WVR_Uuid uuid in trackableMarkerIdArray)
                {
                    result = trackableMarkerController.DestroyTrackableMarker(uuid);

                    if (result == WVR_Result.WVR_Success)
                    {
                        Logger.Log("[MarkerManager] Delete trackable marker: " + MarkerUtils.UUIDToString(uuid));
                    }
                    else
                    {
                        Logger.Log("[MarkerManager] Failed to delete trackable marker: " + MarkerUtils.UUIDToString(uuid));
                    }
                }
            }
        }

        public void SaveMarkers()
        {
            // convert all the detected markers into trackable markers to store them
            foreach (string uuid in markers.Keys)
            {
                // Create trackable marker from the aruco marker
                // The purpose of this is to store the markers as persist data,
                // so that next time we can retrieve them.
                WVR_Result result = trackableMarkerController.CreateTrackableMarker(markers[uuid].data.uuid, uuid.ToCharArray());

                if (result == WVR_Result.WVR_Success)
                {
                    Logger.Log("[MarkerManager] Create trackable marker: " + uuid);
                }
                else
                {
                    Logger.Log("[MarkerManager] Failed to create trackable marker: " + uuid);
                }
            }
        }

        public void SetTargetMarker(WVR_ArucoMarker target)
        {
            filter = true;
            targetMarker = target;
        }

        private bool IsTargetMarker(Marker marker)
        {
            // currently identify markers by Aruco ID
            return marker.data.trackerId == targetMarker.trackerId;
        }

        public void ShowMarkers(bool show)
        {
            foreach (Marker marker in markers.Values)
            {
                if (filter)
                {
                    marker.ShowMarker(show && IsTargetMarker(marker));
                }
                else
                {
                    marker.ShowMarker(show);
                }
            }
        }

        private void Update()
        {
            if (!(isMarkerServiceRunning && isMarkerObserverRunning)) return;
            trackableMarkerController.GetMarkerObserverState(OBSERVER_TARGET, out observerState);
            if (observerState == WVR_MarkerObserverState.WVR_MarkerObserverState_Detecting)
            {
                // get Aruco markers
                WVR_Result result = trackableMarkerController.GetArucoMarkers(
                    TrackableMarkerController.GetCurrentPoseOriginModel(),
                    out WVR_ArucoMarker[] arucoMarkers
                );

                // reset marker existence
                foreach (Marker marker in markers.Values)
                {
                    marker.exist = false;
                }

                foreach (WVR_ArucoMarker aruco in arucoMarkers)
                {
                    string id = MarkerUtils.UUIDToString(aruco.uuid);

                    if (markers.ContainsKey(id))
                    {
                        // update marker
                        markers[id].UpdateMarker(aruco);
                    }
                    else
                    {
                        // create new marker
                        Marker marker = Instantiate(markerPrefab, markersParent).GetComponent<Marker>();
                        marker.Init(this, aruco);
                        if (filter)
                        {
                            marker.ShowMarker(IsTargetMarker(marker));
                        }
                        markers[id] = marker;

                        Logger.Log("[Align] Detect new marker: " + id);
                    }

                    // marker exists
                    markers[id].exist = true;
                }


                // check marker existence
                // NOTE: in current SDK, we are actually unable to known whether the marker is in missing state
                List<string> keys = new List<string>(markers.Keys);
                foreach (string uuid in keys)
                {
                    if (!markers[uuid].exist)
                    {
                        markers.Remove(uuid);
                    }
                }
            }
        }
    }
}