using System;

namespace SharedSpaceExperience
{
    public class SocketDataPack
    {
        public static ulong CHECK_CODE = 0x12345678;
        public static int HEADER_SIZE = 32;

        // header
        public ulong checkCode;
        public ulong senderID;
        public ulong dataType;
        public ulong dataSize;

        // data
        public byte[] data;

        public static SocketDataPack FromBytes(ulong clientID, ulong dataType, byte[] dataBytes)
        {
            return new SocketDataPack()
            {
                checkCode = CHECK_CODE,
                senderID = clientID,
                dataType = dataType,
                dataSize = (ulong)dataBytes.Length,
                data = dataBytes
            };
        }

        public static byte[] ToBytes(SocketDataPack pack)
        {
            byte[] bytes = new byte[HEADER_SIZE + pack.GetDataSize()];
            Array.Copy(BitConverter.GetBytes(pack.checkCode), 0, bytes, 0, 8);
            Array.Copy(BitConverter.GetBytes(pack.senderID), 0, bytes, 8, 8);
            Array.Copy(BitConverter.GetBytes(pack.dataType), 0, bytes, 16, 8);
            Array.Copy(BitConverter.GetBytes(pack.dataSize), 0, bytes, 24, 8);
            Array.Copy(pack.data, 0, bytes, HEADER_SIZE, pack.GetDataSize());

            return bytes;
        }

        public bool SetHeader(byte[] bytes)
        {
            checkCode = BitConverter.ToUInt64(bytes, 0);
            if (checkCode != CHECK_CODE) return false;
            senderID = BitConverter.ToUInt64(bytes, 8);
            dataType = BitConverter.ToUInt64(bytes, 16);
            dataSize = BitConverter.ToUInt64(bytes, 24);

            return true;
        }

        public bool SetData(byte[] bytes)
        {
            data = bytes;
            return bytes.Length == GetDataSize();
        }

        public int GetDataSize()
        {
            return (int)dataSize;
        }
    }
}