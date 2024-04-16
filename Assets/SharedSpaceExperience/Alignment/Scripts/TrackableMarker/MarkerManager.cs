using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;
using Wave.Essence.TrackableMarker;
using System;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience
{
    public class MarkerManager : MonoBehaviour, IAlignMethod
    {
        private bool isMarkerServiceRunning = false;
        private bool isMarkerObserverRunning = false;
        private bool isPaused = false;

        public TrackableMarkerController trackableMarkerController;

        [SerializeField]
        private AlignManager alignManager;

        [SerializeField]
        private Transform markersParent;

        [SerializeField]
        private GameObject markerPrefab;

        private WVR_MarkerObserverState observerState;
        private const WVR_MarkerObserverTarget OBSERVER_TARGET = WVR_MarkerObserverTarget.WVR_MarkerObserverTarget_Aruco;

        private readonly Dictionary<string, Marker> markers = new();
        public Marker selectedMarker = null;
        public Action<Marker> OnDetectNewMarker;
        public Action OnSelectedMarkerUpdated;

        // target marker
        private bool filter = false;
        private WVR_ArucoMarker targetMarker;

        private void OnEnable()
        {
            StartCoroutine(WaitForManagers());
        }
        private IEnumerator WaitForManagers()
        {
            yield return new WaitUntil(() => SystemManager.Instance);
            trackableMarkerController.trackingOrigin = SystemManager.Instance.waveRig.gameObject;
        }

        private void OnDisable()
        {
            StopMarkerService();
        }

        void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                if (isPaused)
                {
                    Logger.Log("resume detection");
                    StartMarkerService();
                    isPaused = false;
                }
            }
            else
            {
                if (isMarkerServiceRunning)
                {
                    StopMarkerService();
                    isPaused = true;
                    Logger.Log("paused detection");
                }
            }
        }

        public bool StartMarkerService()
        {
            // start marker service
            if ((Interop.WVR_GetSupportedFeatures() &
                 (ulong)WVR_SupportedFeature.WVR_SupportedFeature_Marker) != 0)
            {
                WVR_Result result = trackableMarkerController.StartMarkerService();
                isMarkerServiceRunning = result == WVR_Result.WVR_Success;
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
            return StartMarkerDetection();
        }

        public bool StartMarkerDetection()
        {
            // start marker detection
            if (isMarkerObserverRunning)
            {
                // clear all trackable markers
                trackableMarkerController.ClearTrackableMarkers();

                // start detection
                WVR_Result result = trackableMarkerController.StartMarkerDetection(
                    OBSERVER_TARGET
                );

                if (result == WVR_Result.WVR_Success)
                {
                    Logger.Log("start detect marker");
                    return true;
                }
            }
            return false;
        }

        public void StopMarkerService()
        {
            // stop detection
            StopMarkerDetection();

            // stop marker observer
            if (isMarkerServiceRunning && isMarkerObserverRunning)
            {
                WVR_Result result = trackableMarkerController.StopMarkerObserver(
                    OBSERVER_TARGET
                );
                // whether stop the observer successfully
                isMarkerObserverRunning = result != WVR_Result.WVR_Success;

                if (!isMarkerObserverRunning)
                {
                    Logger.Log("stop marker observer");
                }

            }

            // stop marker service
            if (isMarkerServiceRunning)
            {
                trackableMarkerController.StopMarkerService();
                isMarkerServiceRunning = false;
                Logger.Log("stop marker service");
            }

            selectedMarker = null;
        }

        public void StopMarkerDetection()
        {
            trackableMarkerController.GetMarkerObserverState(OBSERVER_TARGET, out observerState);
            if (observerState == WVR_MarkerObserverState.WVR_MarkerObserverState_Detecting)
            {
                WVR_Result result = trackableMarkerController.StopMarkerDetection(
                    OBSERVER_TARGET
                );

                if (result == WVR_Result.WVR_Success)
                {
                    Logger.Log("stop detect marker");
                }
            }
        }

        public void UnsetTargetMarker()
        {
            filter = false;
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

        public bool CreateTrackableMarker(WVR_Uuid uuid)
        {
            if (isMarkerServiceRunning && isMarkerObserverRunning)
            {
                string id = MarkerUtils.UUIDToString(uuid);
                char[] markerNameArray = id.ToCharArray();

                WVR_Result result = trackableMarkerController.CreateTrackableMarker(uuid, markerNameArray);

                if (result == WVR_Result.WVR_Success)
                {
                    Logger.Log("Create trackable marker: " + uuid);
                    return true;
                }
            }
            return false;
        }

        public bool ExportAlignData(out AlignData alignData)
        {
            alignData = null;
            if (selectedMarker == null) return false;

            alignData = new()
            {
                method = AlignManager.AlignMethod.TrackableMarker,
                refPos = selectedMarker.transform.localPosition,
                refRot = selectedMarker.transform.localRotation,
                refData = MarkerUtils.SerializeMarker(selectedMarker.data)
            };
            Logger.Log($"Export marker: {selectedMarker.data.trackerId}, size: {selectedMarker.data.size}");

            return true;
        }

        public bool ImportAlignData(AlignData alignData)
        {
            // set target marker
            filter = MarkerUtils.DeserializeMarker(alignData.refData, out targetMarker);
            if (filter)
            {
                Logger.Log($"Import marker: {targetMarker.trackerId}, size: {targetMarker.size}");
            }
            return filter;
        }

        public bool Align(Marker clientMarker = null)
        {
            if (clientMarker == null) clientMarker = selectedMarker;
            if (clientMarker == null || !filter) return false;

            alignManager.Align(
                clientMarker.transform.position,
                clientMarker.transform.rotation
            );

            return true;
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
                        // Logger.Log("Detect old marker: " + id, false);
                    }
                    else
                    {
                        // create new marker
                        Marker marker = Instantiate(markerPrefab, markersParent).GetComponent<Marker>();
                        marker.Init(this, aruco);
                        markers[id] = marker;
                        OnDetectNewMarker?.Invoke(marker);
                        Logger.Log("Detect new marker: " + id, false);
                    }

                    // marker exists
                    markers[id].exist = true;
                    markers[id].ShowMarker(!filter || IsTargetMarker(markers[id]));
                    markers[id].SetInteractable(!filter);
                }

                // check marker existence
                List<string> keys = new(markers.Keys);
                foreach (string uuid in keys)
                {
                    if (!markers[uuid].exist)
                    {
                        Destroy(markers[uuid].gameObject);
                        markers.Remove(uuid);
                        Logger.Log("Remove nonexist maker: " + uuid);
                    }
                }
            }
        }
    }
}