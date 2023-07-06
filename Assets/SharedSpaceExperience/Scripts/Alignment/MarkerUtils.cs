using System;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;

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

        public static void OverwriteMarkerPose(Marker marker)
        {
            // overwrite marker pose with pose in unity space
            marker.data.pose.position.v0 = marker.transform.localPosition.x;
            marker.data.pose.position.v1 = marker.transform.localPosition.y;
            marker.data.pose.position.v2 = marker.transform.localPosition.z;
            marker.data.pose.rotation.x = marker.transform.localRotation.x;
            marker.data.pose.rotation.y = marker.transform.localRotation.y;
            marker.data.pose.rotation.z = marker.transform.localRotation.z;
            marker.data.pose.rotation.w = marker.transform.localRotation.w;
        }

        public static Byte[] SerializeMarker(WVR_ArucoMarker marker)
        {
            // convert to byte arrays
            List<Byte[]> props = new List<Byte[]>(){
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
            Byte[] data = new Byte[dataSize];
            int offset = 0;
            for (int i = 0; i < props.Count; ++i)
            {
                System.Buffer.BlockCopy(props[i], 0, data, offset, props[i].Length);
                offset += props[i].Length;
            }

            Logger.Log("[MarkerUtils] data size: " + dataSize);

            return data;
        }

        public static WVR_ArucoMarker DeserializeMarker(Byte[] data)
        {
            WVR_ArucoMarker marker = new WVR_ArucoMarker();

            int offset = 0;
            int uuidLen = BitConverter.ToInt32(data, offset);
            offset += 4;
            marker.uuid.data = new Byte[uuidLen];
            System.Buffer.BlockCopy(data, offset, marker.uuid.data, 0, uuidLen);
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

            return marker;
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