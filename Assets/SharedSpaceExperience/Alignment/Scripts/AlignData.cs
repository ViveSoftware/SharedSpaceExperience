using System.Collections.Generic;
using UnityEngine;
using System;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience
{
    public class AlignData
    {
        public AlignManager.AlignMethod method;
        public Vector3 refPos;
        public Quaternion refRot;
        public byte[] refData;

        public static AlignData FromBytes(byte[] bytes)
        {
            AlignData alignData = new();
            if (!alignData.TryFromBytes(bytes)) return null;
            return alignData;
        }

        public bool TryFromBytes(byte[] bytes)
        {
            if (bytes.Length < 4 * 8) return false;

            try
            {
                int offset = 0;

                // align method
                method = (AlignManager.AlignMethod)BitConverter.ToInt32(bytes, offset);
                offset += 4;

                // reference position
                refPos = new(
                    BitConverter.ToSingle(bytes, offset),
                    BitConverter.ToSingle(bytes, offset + 4),
                    BitConverter.ToSingle(bytes, offset + 8)
                );
                offset += 12;

                // reference rotation
                refRot = new(
                    BitConverter.ToSingle(bytes, offset),
                    BitConverter.ToSingle(bytes, offset + 4),
                    BitConverter.ToSingle(bytes, offset + 8),
                    BitConverter.ToSingle(bytes, offset + 12)
                );
                offset += 16;

                // reference data
                int rest = bytes.Length - offset;
                refData = new byte[rest];
                Buffer.BlockCopy(bytes, offset, refData, 0, rest);
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to parse align data: " + e);
                return false;
            }

            return true;
        }

        public byte[] ToBytes()
        {
            List<byte[]> props = new(){
            // align method
            BitConverter.GetBytes((int)method),
            // reference position
            BitConverter.GetBytes(refPos.x),
            BitConverter.GetBytes(refPos.y),
            BitConverter.GetBytes(refPos.z),
            // reference rotation
            BitConverter.GetBytes(refRot.x),
            BitConverter.GetBytes(refRot.y),
            BitConverter.GetBytes(refRot.z),
            BitConverter.GetBytes(refRot.w),
            // reference data
            refData,
        };

            // compute data size
            int dataSize = 0;
            for (int i = 0; i < props.Count; ++i)
            {
                dataSize += props[i].Length;
            }

            // concate byte arrays
            byte[] bytes = new byte[dataSize];
            int offset = 0;
            for (int i = 0; i < props.Count; ++i)
            {
                Buffer.BlockCopy(props[i], 0, bytes, offset, props[i].Length);
                offset += props[i].Length;
            }

            Logger.Log("data size: " + dataSize);

            return bytes;
        }
    }
}