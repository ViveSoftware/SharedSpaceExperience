using System;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience
{
    public class MarkerUtils : MonoBehaviour
    {
        public static string UUIDToString(WVR_Uuid uuid)
        {
            return BitConverter.ToString(uuid.data);
        }

        // NOTE: to get pose in Unity space from raw WVR_ArucoMarker
        // please use trackableMarkerController.ApplyTrackingOriginCorrectionToMarkerPose()
        public static Vector3 GetPosition(WVR_ArucoMarker marker)
        {
            return new Vector3(
                marker.pose.position.v0,
                marker.pose.position.v1,
                marker.pose.position.v2
            );
        }

        public static Quaternion GetRotation(WVR_ArucoMarker marker)
        {
            return new Quaternion(
                marker.pose.rotation.x,
                marker.pose.rotation.y,
                marker.pose.rotation.z,
                marker.pose.rotation.w
            );
        }

        public static byte[] SerializeMarker(WVR_ArucoMarker marker)
        {
            // convert to byte arrays
            List<byte[]> props = new(){
                BitConverter.GetBytes(marker.uuid.data.Length),
                marker.uuid.data,
                BitConverter.GetBytes(marker.trackerId),
                BitConverter.GetBytes(marker.size),
                BitConverter.GetBytes((int)marker.state),
                BitConverter.GetBytes(marker.pose.position.v0),
                BitConverter.GetBytes(marker.pose.position.v1),
                BitConverter.GetBytes(marker.pose.position.v2),
                BitConverter.GetBytes(marker.pose.rotation.x),
                BitConverter.GetBytes(marker.pose.rotation.y),
                BitConverter.GetBytes(marker.pose.rotation.z),
                BitConverter.GetBytes(marker.pose.rotation.w),
            };
            if (marker.markerName.name != null)
            {
                props.Add(System.Text.Encoding.UTF8.GetBytes(marker.markerName.name));
            }

            // compute data size
            int dataSize = 0;
            for (int i = 0; i < props.Count; ++i)
            {
                dataSize += props[i].Length;
            }

            // concate byte arrays
            byte[] data = new byte[dataSize];
            int offset = 0;
            for (int i = 0; i < props.Count; ++i)
            {
                Buffer.BlockCopy(props[i], 0, data, offset, props[i].Length);
                offset += props[i].Length;
            }

            Logger.Log("data size: " + dataSize);

            return data;
        }

        public static bool DeserializeMarker(byte[] data, out WVR_ArucoMarker marker)
        {
            try
            {
                marker = new();

                int offset = 0;
                int uuidLen = BitConverter.ToInt32(data, offset);
                offset += 4;
                marker.uuid.data = new byte[uuidLen];
                Buffer.BlockCopy(data, offset, marker.uuid.data, 0, uuidLen);
                offset += uuidLen;

                marker.trackerId = BitConverter.ToUInt64(data, offset);
                offset += 8;
                marker.size = BitConverter.ToSingle(data, offset);
                offset += 4;
                marker.state = (WVR_MarkerTrackingState)BitConverter.ToInt32(data, offset);
                offset += 4;

                marker.pose.position.v0 = BitConverter.ToSingle(data, offset);
                offset += 4;
                marker.pose.position.v1 = BitConverter.ToSingle(data, offset);
                offset += 4;
                marker.pose.position.v2 = BitConverter.ToSingle(data, offset);
                offset += 4;

                marker.pose.rotation.x = BitConverter.ToSingle(data, offset);
                offset += 4;
                marker.pose.rotation.y = BitConverter.ToSingle(data, offset);
                offset += 4;
                marker.pose.rotation.z = BitConverter.ToSingle(data, offset);
                offset += 4;
                marker.pose.rotation.w = BitConverter.ToSingle(data, offset);
                offset += 4;

                int rest = data.Length - offset;
                if (rest > 0)
                {
                    marker.markerName.name = System.Text.Encoding.UTF8.GetString(data, offset, rest).ToCharArray();
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to deserialize marker: " + e);

                marker = new();
                return false;
            }

            return true;
        }

        public static string MarkerToLog(WVR_ArucoMarker marker)
        {
            if (marker.uuid.data == null) return "";

            string log = "\n";
            log += "UUID: " + UUIDToString(marker.uuid) + "\n";
            log += "Tracker ID: " + marker.trackerId + "\n";
            log += "State: " + marker.state.ToString() + "\n";
            log += "Size: " + marker.size + "\n";
            log += "Position: (" + marker.pose.position.v0 + ", " + marker.pose.position.v1 + ", " + marker.pose.position.v2 + ")\n";
            log += "Rotation: (" + marker.pose.rotation.x + ", " + marker.pose.rotation.y + ", " + marker.pose.rotation.z + ", " + marker.pose.rotation.w + ")\n";
            log += "Name: " + marker.markerName.name + "\n";

            return log;
        }
    }
}