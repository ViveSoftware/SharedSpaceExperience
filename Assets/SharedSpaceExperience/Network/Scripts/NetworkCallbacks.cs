using UnityEngine;
using Unity.Netcode;
using System.Collections;

/*
This class demonstrates how to register network callbacks
It is recommend to register necessary callbacks in individual class rather then inherit this class
*/
namespace SharedSpaceExperience
{
    public class NetworkCallbacks : MonoBehaviour
    {
        protected void OnEnable()
        {
            StartCoroutine(WaitForManagers());
        }

        private IEnumerator WaitForManagers()
        {
            yield return new WaitUntil(() => NetworkManager.Singleton);
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnServerStopped += OnServerStopped;
            NetworkManager.Singleton.OnClientStarted += OnClientStartedHelper;
            NetworkManager.Singleton.OnClientStopped += OnClientStoppedHelper;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkManager.Singleton.OnTransportFailure += OnTransportFailed;

            HostDiscrovery.OnServerFound += OnServerFound;

            yield return new WaitUntil(() => SocketManager.Instance);
            SocketManager.Instance.callbacks.OnConnected += OnSocketConnected;
            SocketManager.Instance.callbacks.OnDisconnected += OnSocketDisconnected;
            SocketManager.Instance.callbacks.OnStopped += OnSocketStopped;
            SocketManager.Instance.callbacks.OnError += OnSocketError;
        }

        protected void OnDisable()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
                NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
                NetworkManager.Singleton.OnClientStarted -= OnClientStartedHelper;
                NetworkManager.Singleton.OnClientStopped -= OnClientStoppedHelper;
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
                NetworkManager.Singleton.OnTransportFailure -= OnTransportFailed;
            }

            HostDiscrovery.OnServerFound -= OnServerFound;

            if (SocketManager.Instance != null)
            {
                SocketManager.Instance.callbacks.OnConnected -= OnSocketConnected;
                SocketManager.Instance.callbacks.OnDisconnected -= OnSocketDisconnected;
                SocketManager.Instance.callbacks.OnStopped -= OnSocketStopped;
                SocketManager.Instance.callbacks.OnError -= OnSocketError;
            }
        }

        protected virtual void OnServerStarted()
        {
            // This callback is invoked when the local server is started and listening for incoming connections.
        }

        protected virtual void OnServerStopped(bool what)
        {
            // This callback is invoked when the local server stops.
        }

        private void OnClientStartedHelper()
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;

            OnClientStarted();
        }

        protected virtual void OnClientStarted()
        {
            // This callback is invoked when the local client is ready
        }

        private void OnClientStoppedHelper(bool what)
        {
            if (NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
            }

            OnClientStopped(what);
        }

        protected virtual void OnClientStopped(bool what)
        {
            // This callback is invoked when the local client stops
        }

        protected virtual void OnClientConnected(ulong clientId)
        {
            // This callback is invoked when a client connects. 
            // This callback is only ran on the server and on the local client that connects.
        }

        protected virtual void OnClientDisconnected(ulong clientId)
        {
            // This callback is invoked when a client disconnects. 
            // This callback is only ran on the server and on the local client that disconnects.

            // NOTE:
            // If the client is disconnected logically, 
            // the actor won't receive this event
        }

        protected virtual void OnTransportFailed()
        {
            // This callback is invoked if the NetworkTransport fails.
        }

        protected virtual void OnSceneEvent(SceneEvent sceneEvent)
        {
            // This callback is invoked if scene changes.
            // Check sceneEvent.SceneEventType to know what event is triggered

            // SceneEventType.Load: starts load scene
            // SceneEventType.LoadComplete: local scene loaded
            // SceneEventType.LoadEventCompleted: server and all clients scene loaded

            // SceneEventType.Unload: starts unload scene
            // SceneEventType.UnloadComplete: local scene unloaded
            // SceneEventType.UnloadEventCompleted: server and all clients scene unloaded

            // SceneEventType.Synchronize: starts synchronize scene
            // SceneEventType.SynchronizeComplete: client synchronized
            // SceneEventType.ReSynchronize: server ask client to resynchronized
        }

        protected virtual void OnServerFound(string serverName, string serverIP, ushort serverPort)
        {
            // This callback is invoked when network discovery found a server
        }

        protected virtual void OnSocketConnected()
        {
            // This callback is invoked when the socket connects
        }

        protected virtual void OnSocketDisconnected()
        {
            // This callback is invoked when the socket disconnected
        }

        protected virtual void OnSocketStopped()
        {
            // This callback is invoked when the socket port is closed
        }

        protected virtual void OnSocketError(int errorCode)
        {
            // This callback is invoked if the socket gets error
        }
    }
}