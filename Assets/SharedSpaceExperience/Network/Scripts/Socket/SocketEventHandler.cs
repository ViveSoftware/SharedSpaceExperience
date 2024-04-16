using System.Collections.Generic;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience
{
    public class SocketEventHandler
    {
        public delegate void Handler(StreamManager stream, SocketDataPack pack);
        private readonly Dictionary<ulong, Handler> eventTable = new();

        public bool Register(ulong eventID, Handler handler)
        {
            if (eventTable.ContainsKey(eventID)) return false;

            eventTable.Add(eventID, handler);
            return true;
        }

        public bool Deregister(ulong eventID)
        {
            return eventTable.Remove(eventID);
        }

        public void HandleEvent(StreamManager stream, SocketDataPack pack)
        {
            if (!eventTable.ContainsKey(pack.dataType))
            {
                Logger.LogError("Unknown event: " + pack.dataType);
                return;
            }
            eventTable[pack.dataType]?.Invoke(stream, pack);
        }
    }
}