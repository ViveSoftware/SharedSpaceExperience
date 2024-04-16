using System;

namespace SharedSpaceExperience
{
    public class SocketCallbacks
    {
        public Action OnConnected;
        public Action OnDisconnected;
        public Action OnStopped;

        public Action<int> OnError;

        public Action<StreamManager, int, int> OnReceivingData;
        public Action<StreamManager, SocketDataPack> OnReceiveData;
    }
}