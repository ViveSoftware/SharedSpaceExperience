#if UNITY_EDITOR || !UNITY_ANDROID
#define PC_DEBUG
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Wave.Native;
using Wave.Essence.Events;
using Wave.Essence.ScenePerception;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience
{
    public class SpatialManager : MonoBehaviour, IAlignMethod
    {
        private const string ALIGN_ANCHOR_NAME = "AlignAnchor";
        private const string PERSISTED_ANCHOR_NAME_PREFIX = "Persisted";
        private const string ALIGN_PERSISTED_ANCHOR_NAME = PERSISTED_ANCHOR_NAME_PREFIX + ALIGN_ANCHOR_NAME;
        private readonly int PERSISTED_ANCHOR_NAME_PREFIX_LEN = PERSISTED_ANCHOR_NAME_PREFIX.Length;

#if PC_DEBUG
        private const string FILE_NAME = "pc_debug_spatial_anchor.dat";
#endif

        private bool isSceneRunning = false;
        private bool isScenePerceptionRunning = false;
        private bool isPaused = false;

        [SerializeField]
        private ScenePerceptionManager scenePerceptionManager;

        [SerializeField]
        private AlignManager alignManager;


        [SerializeField]
        private SpatialAnchor alignAnchor;
        [SerializeField]
        private Transform anchorSpawner;

        [SerializeField]
        private float checkRelocatePeriod = 0.5f;

        public Action OnAligned;

        private Coroutine alignCoroutine = null;


        private void OnEnable()
        {
            // hide anchor
            alignAnchor.ShowAnchor(false);

            StartCoroutine(WaitForManagers());
        }

        private IEnumerator WaitForManagers()
        {
            yield return new WaitUntil(() => SystemManager.Instance);
            scenePerceptionManager.TrackingOrigin = SystemManager.Instance.waveRig;
            anchorSpawner = SystemManager.Instance.rightController;
        }

        private void OnDisable()
        {
            StopSpatialAnchorService();
        }

        void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                if (isPaused)
                {
                    Logger.Log("resume spatial anchor service");
                    StartSpatialAnchorService();
                    isPaused = false;
                }
            }
            else
            {
                if (isSceneRunning)
                {
                    StopSpatialAnchorService();
                    isPaused = true;
                    Logger.Log("paused spatial anchor service");
                }
            }
        }

        public bool StartSpatialAnchorService()
        {
            if ((Interop.WVR_GetSupportedFeatures() & (ulong)WVR_SupportedFeature.WVR_SupportedFeature_ScenePerception) != 0)
            {
                WVR_Result wvrResult = scenePerceptionManager.StartScene();
                isSceneRunning = wvrResult == WVR_Result.WVR_Success;
                Logger.Log("start scene.");

            }

            if (isSceneRunning && !isScenePerceptionRunning)
            {
                WVR_Result wvrResult = scenePerceptionManager.StartScenePerception(WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane);
                isScenePerceptionRunning = wvrResult == WVR_Result.WVR_Success;
                Logger.Log("start scene perception.");
            }

            if (isScenePerceptionRunning)
            {
                // clear align anchors
                DestroyAlignAnchor();

                SystemEvent.Listen(WVR_EventType.WVR_EventType_SpatialAnchor_Changed, OnSpatialAnchorUpdated, true);
                SystemEvent.Listen(WVR_EventType.WVR_EventType_PersistedSpatialAnchor_Changed, OnPersistedAnchorUpdated, true);
                Logger.Log("start spatial anchor service.");
                return true;
            }

            Logger.Log("Scene Perception is not available on the current device.");
            return false;
        }

        public void StopSpatialAnchorService()
        {
            // stop coroutine
            if (alignCoroutine != null)
            {
                StopCoroutine(alignCoroutine);
                alignCoroutine = null;
            }

            if (isSceneRunning)
            {
                if (isScenePerceptionRunning)
                {
                    // only distroy align anchor
                    DestroyAlignAnchor();

                    // deregister event
                    SystemEvent.Remove(WVR_EventType.WVR_EventType_SpatialAnchor_Changed, OnSpatialAnchorUpdated);
                    SystemEvent.Remove(WVR_EventType.WVR_EventType_PersistedSpatialAnchor_Changed, OnPersistedAnchorUpdated);

                    // stop scene perception
                    WVR_Result wvrResult = scenePerceptionManager.StopScenePerception(WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane);
                    isScenePerceptionRunning = wvrResult != WVR_Result.WVR_Success;

                    Logger.Log("stop scene perception: " + !isScenePerceptionRunning);

                }

                // stop scene
                scenePerceptionManager.StopScene();
                isSceneRunning = false;
                Logger.Log("stop scene");
            }

            // hide anchor
            alignAnchor.ShowAnchor(false);
        }

        private string GetCorresponedSpatialAnchorName(string persistedAnchorName)
        {
            if (!persistedAnchorName.StartsWith(PERSISTED_ANCHOR_NAME_PREFIX))
            {
                Logger.LogError("Unsupported persisted anchor name format: " + persistedAnchorName);
                return null;
            }

            return persistedAnchorName[PERSISTED_ANCHOR_NAME_PREFIX_LEN..];
        }

        private HashSet<string> GetSpatialAnchorNames()
        {
            HashSet<string> spatialAnchorNames = new();

            // get spatial anchor UUIDs
            WVR_Result wvrResult = scenePerceptionManager.GetSpatialAnchors(out ulong[] uuids);
            if (wvrResult != WVR_Result.WVR_Success)
            {
                Logger.LogError("Failed to load spatial anchors");
                return null;
            }

            WVR_PoseOriginModel wvrOriginModel = ScenePerceptionManager.GetCurrentPoseOriginModel();
            foreach (ulong uuid in uuids)
            {
                // get anchor name
                wvrResult = scenePerceptionManager.GetSpatialAnchorState(
                    uuid,
                    wvrOriginModel,
                    out WVR_SpatialAnchorState anchorState
                );
                if (wvrResult != WVR_Result.WVR_Success)
                {
                    Logger.LogError("Failed to get spatial anchor state: " + uuid);
                    continue;
                }

                // add to set
                string anchorName = anchorState.anchorName.ToString();
                if (!spatialAnchorNames.Add(anchorName))
                {
                    // check duplicated anchor name
                    Logger.LogWarning($"Found spatial anchors with same name: {anchorName} ({uuid})");
                }

                Logger.Log("Found spatial anchor: " + anchorName);
            }

            return spatialAnchorNames;
        }

        private bool DoesPersistedAnchorExists(string persistedAnchorName)
        {
            return DoesPersistedAnchorExists(persistedAnchorName, out bool exists) && exists;
        }

        private bool DoesPersistedAnchorExists(string persistedAnchorName, out bool exists)
        {
            exists = false;
            if (!isScenePerceptionRunning) return false;

            // retrieve persisted anchors
            WVR_Result wvrResult = scenePerceptionManager.GetPersistedSpatialAnchorNames(out string[] names);
            if (wvrResult != WVR_Result.WVR_Success)
            {
                Logger.LogError("Failed to load persisted spatial anchors");
                return false;
            }

            // check if target anchor exists
            foreach (string presistedName in names)
            {
                if (presistedName == persistedAnchorName)
                {
                    exists = true;
                    break;
                }
            }

            return true;
        }

        public bool CreateAnchor(string spatialAnchorName, bool persisted = false)
        {
            if (!isScenePerceptionRunning) return false;

            // check whether the anchor name has been used
            HashSet<string> spatialAnchorNames = GetSpatialAnchorNames();
            if (spatialAnchorNames.Contains(spatialAnchorName))
            {
                Logger.LogError("Failed to create spatial anchor: anchor name has been used.");
                return false;
            }

            // check whether the persist anchor name has been used
            string persistedAnchorName = PERSISTED_ANCHOR_NAME_PREFIX + spatialAnchorName;
            if (persisted)
            {
                if (DoesPersistedAnchorExists(persistedAnchorName))
                {
                    Logger.LogError("Failed to create persisted anchor: anchor name has been used.");
                    return false;
                }
            }

            // create spatial anchor
            WVR_Result wvrResult = scenePerceptionManager.CreateSpatialAnchor(
                spatialAnchorName.ToCharArray(),
                anchorSpawner.position,
                anchorSpawner.rotation,
                ScenePerceptionManager.GetCurrentPoseOriginModel(),
                out ulong uuid,
                true
            );
            if (wvrResult != WVR_Result.WVR_Success)
            {
                Logger.LogError("Failed to create spatial anchor");
                return false;
            }
            Logger.Log($"Create spatial anchor: {spatialAnchorName} ({uuid})");

            // create persist anchor from spatial anchor
            if (persisted)
            {
                wvrResult = scenePerceptionManager.PersistSpatialAnchor(persistedAnchorName, uuid);
                if (wvrResult != WVR_Result.WVR_Success)
                {
                    Logger.LogError("Failed to create persisted anchor");
                    return false;
                }
                Logger.Log("Create persisted anchor: " + persistedAnchorName);
            }

            return true;
        }

        public bool DestroyAnchor(ulong anchorUUID)
        {
            if (!isScenePerceptionRunning) return false;

            // check if spatial anchor exists
            WVR_Result wvrResult = scenePerceptionManager.GetSpatialAnchorState(
                anchorUUID,
                ScenePerceptionManager.GetCurrentPoseOriginModel(),
                out WVR_SpatialAnchorState anchorState
            );
            if (wvrResult != WVR_Result.WVR_Success)
            {
                Logger.LogError("Failed to destroy persisted anchor: spatial anchor not found");
                return false;
            }

            // destroy spatial anchor
            string spatialAnchorName = anchorState.anchorName.ToString();
            wvrResult = scenePerceptionManager.DestroySpatialAnchor(anchorUUID);
            if (wvrResult != WVR_Result.WVR_Success)
            {
                Logger.LogError("Failed to destroy spatial anchor");
                return false;
            }
            Logger.Log($"Destroy spatial anchor: {spatialAnchorName} ({anchorUUID})");

            // check if persisted anchor exists
            string persistedAnchorName = PERSISTED_ANCHOR_NAME_PREFIX + spatialAnchorName;
            if (!DoesPersistedAnchorExists(persistedAnchorName))
            {
                Logger.LogWarning("Failed to destroy align anchor: align anchor not exists");
                return false;
            }

            // destroy persisted anchor
            wvrResult = scenePerceptionManager.UnpersistSpatialAnchor(persistedAnchorName);
            if (wvrResult != WVR_Result.WVR_Success)
            {
                Logger.LogError("Failed to destroy persisted anchor");
                return false;
            }
            Logger.Log("Destroy persisted anchor: " + persistedAnchorName);

            return true;
        }

        public bool CreateAlignAnchor()
        {
            return CreateAnchor(ALIGN_ANCHOR_NAME, true);
        }

        public bool DestroyAlignAnchor()
        {
            return DoesPersistedAnchorExists(ALIGN_PERSISTED_ANCHOR_NAME) &&
                DestroyAnchor(alignAnchor.GetAnchorUUID());
        }

        public bool ExportAlignData(out AlignData alignData)
        {
            alignData = null;
#if !PC_DEBUG

            // check if align anchor exists
            if (!DoesPersistedAnchorExists(ALIGN_PERSISTED_ANCHOR_NAME))
            {
                Logger.LogError("Align anchor not exists");
                return false;
            }

            // export persisted anchor
            WVR_Result wvrResult = scenePerceptionManager.ExportPersistedSpatialAnchor(ALIGN_PERSISTED_ANCHOR_NAME, out byte[] data);
            if (wvrResult != WVR_Result.WVR_Success)
            {
                Logger.LogError("Failed to export spatial anchor");
                return false;
            }

            // pack align data
            alignData = new()
            {
                method = AlignManager.AlignMethod.SpatialAnchor,
                refPos = alignAnchor.transform.localPosition,
                refRot = alignAnchor.transform.localRotation,
                refData = data
            };
#else
            // For PC debug
            // load prestored anchor
            if (!LocalStorage.Load(FILE_NAME, out byte[] data))
            {
                Logger.LogError("Failed to load spatial anchor");
                return false;
            }
            alignData = AlignData.FromBytes(data);
#endif
            Logger.Log("Export spatial anchor");
            return true;
        }

        public bool ImportAlignData(AlignData alignData)
        {
#if !PC_DEBUG
            // import persisted anchor
            // OnPersistedAnchorUpdated will be invoked when the persisted anchor is imported (generated).
            WVR_Result wvrResult = scenePerceptionManager.ImportPersistedSpatialAnchor(alignData.refData);
            if (wvrResult != WVR_Result.WVR_Success)
            {
                Logger.Log("Failed to import spatial anchor");
                return false;
            }
#else
            // For PC debug
            // save received anchor
            LocalStorage.SaveAsync(FILE_NAME, alignData.ToBytes());

            // consider aligned
            OnAligned?.Invoke();
#endif
            Logger.Log("Import anchor");

            return true;
        }

        private void OnPersistedAnchorUpdated(WVR_Event_t wvrEvent)
        {
            // Check for newly imported persisted anchor and create spatial anchor

            // Import persist anchor will not generate correspond spatial anchor automatically
            // We need to use CreateSpatialAnchorFromPersistenceName to create the spatial anchor

            // Currently the only way to check whether a persisted anchor has a correspond spatial anchor is by their name,
            // which has to be maintained manually by developers

            Logger.Log("OnPersistedAnchorUpdated");

            // get exist spatial anchor names
            HashSet<string> spatialAnchorNames = GetSpatialAnchorNames();
            if (spatialAnchorNames == null) return;

            // find newly imported persisted anchors
            WVR_Result wvrResult = scenePerceptionManager.GetPersistedSpatialAnchorNames(out string[] names);
            if (wvrResult != WVR_Result.WVR_Success)
            {
                Logger.LogError("Failed to load persisted spatial anchors");
                return;
            }

            foreach (string persistedName in names)
            {
                // get corresponed spatial anchor name
                string spatialAnchorName = GetCorresponedSpatialAnchorName(persistedName);
                if (spatialAnchorName == null) continue;

                // check if the correspond spatial anchor already exists
                if (spatialAnchorNames.Contains(spatialAnchorName)) continue;

                // create spatial for newly imported persisted anchor
                wvrResult = scenePerceptionManager.CreateSpatialAnchorFromPersistenceName(
                    persistedName,
                    spatialAnchorName,
                    out ulong uuid
                );
                if (wvrResult != WVR_Result.WVR_Success)
                {
                    Logger.LogError("Failed to create spatial anchor from persisted anchor: " + persistedName);
                    continue;
                }
                Logger.Log($"Create spatial anchor from newly imported persisted anchor: {spatialAnchorName} ({uuid})");

                // start align if the anchor is align anchor
                if (persistedName == ALIGN_PERSISTED_ANCHOR_NAME)
                {
                    if (alignCoroutine == null)
                    {
                        // start coroutine for anchor relocation
                        alignCoroutine = StartCoroutine(Align(uuid));
                    }
                    else
                    {
                        Logger.LogError("Is already aligning");
                    }
                }
            }

        }

        private void OnSpatialAnchorUpdated(WVR_Event_t wvrEvent)
        {
            // Update spatial anchor game objects

            // In this case, since we use only one anchor for align,
            // we will only update that anchor

            Logger.Log("OnSpatialAnchorUpdated");

            bool isAlignAnchorExists = false;

            // get spatial anchor UUIDs
            WVR_Result wvrResult = scenePerceptionManager.GetSpatialAnchors(out ulong[] uuids);
            if (wvrResult != WVR_Result.WVR_Success)
            {
                Logger.LogError("Failed to load spatial anchors");
                return;
            }

            // go through all spatial anchors
            WVR_PoseOriginModel wvrOriginModel = ScenePerceptionManager.GetCurrentPoseOriginModel();
            Pose trackingOriginPose = scenePerceptionManager.GetTrackingOriginPose();
            foreach (ulong uuid in uuids)
            {
                // get anchor state
                wvrResult = scenePerceptionManager.GetSpatialAnchorState(
                    uuid,
                    wvrOriginModel,
                    out SpatialAnchorTrackingState anchorState,
                    out Pose pose,
                    out string anchorName,
                    trackingOriginPose
                );
                if (wvrResult != WVR_Result.WVR_Success)
                {
                    Logger.LogError("Failed to get spatial anchor state: " + uuid);
                    continue;
                }

                // log anchor info
                LogAnchor(uuid, anchorName, anchorState, pose);

                // update align anchor game object
                if (anchorName == ALIGN_ANCHOR_NAME)
                {
                    if (isAlignAnchorExists)
                    {
                        Logger.LogError("Found multiple align anchors");
                        continue;
                    }
                    isAlignAnchorExists = true;

                    // update align anchor object
                    alignAnchor.SetAnchorInfo(uuid, anchorName);
                    alignAnchor.SetPose(pose.position, pose.rotation);
                    alignAnchor.ShowAnchor(anchorState == SpatialAnchorTrackingState.Tracking);
                }
            }

            if (!isAlignAnchorExists)
            {
                // hide align anchor object
                alignAnchor.ShowAnchor(false);
            }
        }

        private IEnumerator Align(ulong uuid)
        {
            // wait for align anchor relocate
            string anchorName;
            Pose pose;
            WVR_PoseOriginModel wvrOriginModel = ScenePerceptionManager.GetCurrentPoseOriginModel();
            Pose trackingOriginPose = scenePerceptionManager.GetTrackingOriginPose();
            while (true)
            {
                // get align anchor state and pose
                WVR_Result wvrResult = scenePerceptionManager.GetSpatialAnchorState(
                    uuid,
                    wvrOriginModel,
                    out SpatialAnchorTrackingState anchorState,
                    out pose,
                    out anchorName,
                    trackingOriginPose
                );
                if (wvrResult == WVR_Result.WVR_Success)
                {
                    // log anchor
                    Logger.Log("Relocating...");
                    LogAnchor(uuid, anchorName, anchorState, pose);

                    // check if relocated
                    if (anchorState == SpatialAnchorTrackingState.Tracking) break;
                }
                else
                {
                    Logger.LogError("Failed to get spatial anchor state: " + uuid);
                }

                yield return new WaitForSeconds(checkRelocatePeriod);
            }
            if (anchorName != ALIGN_ANCHOR_NAME)
            {
                Logger.LogWarning($"Align anchor {anchorName} has different name from {ALIGN_ANCHOR_NAME}");
            }

            // show anchor object
            alignAnchor.SetAnchorInfo(uuid, anchorName);
            alignAnchor.SetPose(pose.position, pose.rotation);
            alignAnchor.ShowAnchor(true);

            // do align
            alignManager.Align(
                alignAnchor.transform.position,
                alignAnchor.transform.rotation
            );
            Logger.Log("Aligned");
            OnAligned?.Invoke();

            alignCoroutine = null;
        }

        private void LogAnchor(ulong uuid, string name, SpatialAnchorTrackingState state, Pose pose)
        {
            Logger.Log(
                "==== spatial anchor: " + uuid +
                "\n\tanchorName: " + name +
                "\n\tanchorState: " + state +
                "\n\tposition: " + pose.position +
                "\n\trotation: " + pose.rotation
            );
        }
    }
}