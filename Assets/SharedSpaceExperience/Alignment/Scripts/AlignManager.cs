using System.Collections;
using UnityEngine;
using Unity.Netcode;
using System;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience
{
    public class AlignManager : MonoBehaviour
    {
        private const string FILE_NAME = "align_data.dat";

        public enum AlignMethod
        {
            NotAligned,
            Skipped,
            TrackableMarker,
            SpatialAnchor,
        }

        [SerializeField]
        private Transform waveRig;

        [SerializeField]
        [Tooltip("Assume all users share the same y-axis.")]
        private bool correctYAxis = true;
        [SerializeField]
        [Tooltip("Assume all users have the same floor height.")]
        private bool correctHeight = true;

        private SocketEventHandler eventHandler = new();

        private AlignData alignData = null;

        public Action<int, int> OnLoadingAlignData;
        public Action<bool> OnAlignDataLoaded;

        public Action OnResetTrackedSpace;
        public Action OnTrackedSpaceUpdated;

        enum AlignSocketEvent
        {
            RequestAlignDataEvent = 1,
            ReceiveAlignDataEvent,
        }

        private void OnEnable()
        {
            StartCoroutine(WaitForManagers());
        }

        private IEnumerator WaitForManagers()
        {
            yield return new WaitUntil(() => SocketManager.Instance);
            SocketManager.Instance.BeforeSocketStart += RegisterSocketCallbacks;
            SocketManager.Instance.BeforeSocketStop += DeregisterSocketCallbacks;

            // register when scene change
            if (SocketManager.Instance.isActive) RegisterSocketCallbacks();

            yield return new WaitUntil(() => SystemManager.Instance);
            waveRig = SystemManager.Instance.waveRig;

            yield return new WaitUntil(() => RoomProperty.Instance);
            RoomProperty.Instance.OnRealign += OnRealign;

            // Load from local storage
            if (NetworkController.Instance.isServer &&
                RoomProperty.Instance.alignMethod.Value > AlignMethod.Skipped &&
                alignData == null &&
                LocalStorage.Load(FILE_NAME, out byte[] data))
            {
                alignData = AlignData.FromBytes(data);
                Logger.Log("Load align data from local storage");
            }
        }

        private void OnDisable()
        {
            SocketManager.Instance.BeforeSocketStart -= RegisterSocketCallbacks;
            SocketManager.Instance.BeforeSocketStop -= DeregisterSocketCallbacks;

            // deregister when scene change
            if (SocketManager.Instance.isActive) DeregisterSocketCallbacks();

            RoomProperty.Instance.OnRealign -= OnRealign;
        }

        public void RegisterSocketCallbacks()
        {
            if (SocketManager.Instance.isServer)
            {
                // register server callback
                eventHandler.Register((ulong)AlignSocketEvent.RequestAlignDataEvent, RequestAlignDataHandler);
            }
            else
            {
                // register client callback
                eventHandler.Register((ulong)AlignSocketEvent.ReceiveAlignDataEvent, ReceiveAlignDataHandler);
            }

            SocketManager.Instance.callbacks.OnReceiveData += eventHandler.HandleEvent;
            SocketManager.Instance.callbacks.OnReceivingData += OnReceivingData;
        }

        public void DeregisterSocketCallbacks()
        {
            // deregister callbacks
            eventHandler.Deregister((ulong)AlignSocketEvent.RequestAlignDataEvent);
            eventHandler.Deregister((ulong)AlignSocketEvent.ReceiveAlignDataEvent);

            SocketManager.Instance.callbacks.OnReceiveData -= eventHandler.HandleEvent;
            SocketManager.Instance.callbacks.OnReceivingData -= OnReceivingData;
        }

        public void ResetAlignment()
        {
            // clear align data
            alignData = null;

            // reset room property
            if (RoomProperty.Instance != null && NetworkController.Instance.isServer)
            {
                RoomProperty.Instance.alignMethod.Value = AlignMethod.NotAligned;
            }

            // reset trackable pose
            ResetTrackedSpace();
        }

        public void ResetTrackedSpace()
        {
            OnResetTrackedSpace?.Invoke();

            // reset trackable pose
            SetTrackableSpacePose(Vector3.zero, Quaternion.identity);
        }

        public void SetAlignData(AlignData alignData)
        {
            this.alignData = alignData;

            // save to local storage
            LocalStorage.SaveAsync(FILE_NAME, this.alignData.ToBytes());
        }

        public AlignData GetAlignData()
        {
            return alignData;
        }

        public void RequestAlignData()
        {
            // client send request align data
            if (SocketManager.Instance.isServer) return;
            Logger.Log("Request RequestAlignData");

            SocketDataPack request = SocketDataPack.FromBytes(
                NetworkManager.Singleton.LocalClientId,
                (ulong)AlignSocketEvent.RequestAlignDataEvent,
                new byte[0]
            );
            SocketManager.Instance.client.SendDataAsync(request);

        }

        private async void RequestAlignDataHandler(StreamManager stream, SocketDataPack pack)
        {
            // server send align data
            if (!SocketManager.Instance.isServer) return;
            Logger.Log("Receive RequestAlignDataEvent");

            SocketDataPack response = SocketDataPack.FromBytes(
                NetworkManager.Singleton.LocalClientId,
                (ulong)AlignSocketEvent.ReceiveAlignDataEvent,
                alignData.ToBytes()
            );
            Logger.Log($"Data send: {response.dataSize}");

            await stream.SendDataAsync(response);
        }

        private void ReceiveAlignDataHandler(StreamManager stream, SocketDataPack pack)
        {
            // client receive align data
            if (SocketManager.Instance.isServer) return;
            Logger.Log("Receive ReceiveAlignDataEvent");
            Logger.Log($"Data received: {pack.dataSize}");

            alignData = AlignData.FromBytes(pack.data);
            if (alignData == null)
            {
                Logger.LogError("Failed to parse align data");
                OnAlignDataLoaded?.Invoke(false);
                return;
            }

            // check whether the alignment method is matched
            if (alignData.method != RoomProperty.Instance.alignMethod.Value)
            {
                Logger.LogError($"Mismatch align data. Expect {RoomProperty.Instance.alignMethod.Value} but receive {alignData.method}");
                OnAlignDataLoaded?.Invoke(false);
                return;
            }

            Logger.Log($"Parse align data: {alignData.method}, pos: {alignData.refPos}, rot: {alignData.refRot}");

            // invoke callback to update UI
            OnAlignDataLoaded?.Invoke(true);
        }

        private void OnRealign()
        {
            Logger.Log("Realign");

            if (SocketManager.Instance.isServer) return;
            // client reconnect to stop receiving align data
            SocketManager.Instance.Reconnect();
        }

        public void Align(Vector3 refPosClient, Quaternion refRotClient)
        {
            // refPosHost/refRotHost: reference pose in host space
            // refPosClient/refRotClient: reference pose in client space
            Vector3 refPosHost = alignData.refPos;
            Quaternion refRotHost = alignData.refRot;

            // compute host origin pose in client space, which will be the new origin of client
            Quaternion newOriginRot = refRotClient * Quaternion.Inverse(refRotHost);
            Vector3 newOriginPos = refPosClient - (newOriginRot * refPosHost);

            // assume host and client have the same up vector (y axis)
            // correct marker pose to make the computed host y axis be the same as the client
            if (correctYAxis)
            {
                // rotation to correct y axis
                Vector3 newOriginYAxis = newOriginRot * Vector3.up;
                Quaternion correctRot = Quaternion.FromToRotation(newOriginYAxis, Vector3.up);

                // correct y axis and recompute position
                newOriginRot = correctRot * newOriginRot;
                newOriginPos = refPosClient - (newOriginRot * refPosHost);
            }

            // assume the host and the client have the same floor height
            // this assumption depends on how well the room setup has been done
            if (correctHeight)
            {
                newOriginPos.y = 0;
            }

            // instead of moving the whole scene to the new origin
            // here we move the tracked devices in the opposite direction to get the same effect
            SetTrackableSpacePose(
                -(Quaternion.Inverse(newOriginRot) * newOriginPos),
                Quaternion.Inverse(newOriginRot)
            );
            // waveRig.position -= Quaternion.Inverse(newOriginRot) * newOriginPos;
            // waveRig.rotation *= Quaternion.Inverse(newOriginRot);
        }

        private void SetTrackableSpacePose(Vector3 position, Quaternion rotation)
        {
            waveRig.SetPositionAndRotation(position, rotation);
            transform.SetPositionAndRotation(position, rotation);

            OnTrackedSpaceUpdated?.Invoke();
        }

        private void OnReceivingData(StreamManager stream, int bytesReceived, int totalBytes)
        {
            // invoke callback to update UI
            OnLoadingAlignData?.Invoke(bytesReceived, totalBytes);
        }
    }
}