using Unity.Netcode;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience
{
    public class NetworkDebug : NetworkCallbacks
    {
        protected override void OnServerStarted()
        {
            Logger.Log("server start");
        }

        protected override void OnServerStopped(bool what)
        {
            Logger.Log("server stop: " + what);
        }

        protected override void OnClientStarted()
        {
            Logger.Log("client start");
        }

        protected override void OnClientStopped(bool what)
        {
            Logger.Log("client stop: " + what);
        }

        protected override void OnClientConnected(ulong clientId)
        {
            Logger.Log("client connected: " + clientId);
        }

        protected override void OnClientDisconnected(ulong clientId)
        {
            Logger.Log("client disconnected: " + clientId);
        }

        protected override void OnTransportFailed()
        {
            Logger.Log("transport failed");
        }

        protected override void OnSceneEvent(SceneEvent sceneEvent)
        {
            Logger.Log($"Scene event type: {sceneEvent.SceneEventType}, scene name: {sceneEvent.SceneName}");
        }

        protected override void OnServerFound(string serverName, string serverIP, ushort serverPort)
        {
            Logger.Log($"found server: {serverName} ({serverIP}:{serverPort})");
        }

        protected override void OnSocketConnected()
        {
            Logger.Log("socket connected");
        }

        protected override void OnSocketDisconnected()
        {
            Logger.Log("socket disconnected");
        }

        protected override void OnSocketStopped()
        {
            Logger.Log("socket stopped");
        }

        protected override void OnSocketError(int errorCode)
        {
            Logger.Log("socket error: " + errorCode);
        }
    }
}