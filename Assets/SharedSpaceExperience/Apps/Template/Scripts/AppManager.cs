using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Unity.Netcode;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience
{
    public class AppManager : MonoBehaviour
    {
        public static AppManager Instance { get; private set; }

        private const string LAUNCHER_SCENE_NAME = "Launcher";

        [SerializeField]
        protected string APP_SCENE_NAME = "AppTemplate";

        [SerializeField]
        protected GameObject appUserPrefab;

        public Action OnSceneLoaded;

        private void OnEnable()
        {
            // singleton
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(this);

            // for PC debug
            if (NetworkManager.Singleton == null)
            {
                SceneManager.LoadScene(LAUNCHER_SCENE_NAME);
                return;
            }

            // register callback
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        }

        private void OnDisable()
        {
            // singleton
            if (Instance == this) Instance = null;

            // deregister callback
            if (NetworkManager.Singleton?.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            }
        }

        private void OnLoadEventCompleted(
            string sceneName,
            LoadSceneMode loadSceneMode,
            List<ulong> clientsCompleted,
            List<ulong> clientsTimedOut
        )
        {
            if (sceneName == APP_SCENE_NAME)
            {
                OnSceneLoaded?.Invoke();
                SpawnAppUser();
            }
        }

        public void LeaveApp()
        {
            if (NetworkController.Instance.isServer)
            {
                // reset user is ready
                UserManager.Instance.ResetAllUserIsReady(false);

                // load launcher scene
                NetworkController.Instance.LoadScene(LAUNCHER_SCENE_NAME);
            }
            else
            {
                // disconnect? & load launcher scene
                NetworkController.Instance.StopNetwork();

                // load launcher scene
                SceneManager.LoadScene(LAUNCHER_SCENE_NAME);
            }
        }

        private void SpawnAppUser()
        {
            if (!NetworkController.Instance.isServer) return;
            foreach (ulong uid in UserManager.UserProperties.Keys)
            {
                UserProperty user = UserManager.UserProperties[uid];
                if (user == null)
                {
                    Logger.LogWarning("Failed to spawn app user for " + uid);
                }

                SpawnNetworkObject(uid, appUserPrefab);
            };
        }

        public GameObject SpawnNetworkObject(ulong owner, GameObject prefab, Transform parent = null)
        {
            return SpawnNetworkObject(owner, prefab, Vector3.zero, Quaternion.identity, parent);
        }

        public GameObject SpawnNetworkObject(
            ulong owner,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null
        )
        {
            // instantiate gameobject
            if (prefab == null) return null;
            GameObject obj = Instantiate(prefab, position, rotation, parent);

            // spawn network object
            if (obj.TryGetComponent<NetworkObject>(out var networkObj))
            {
                networkObj.SpawnWithOwnership(owner, true);
            }

            return obj;
        }
    }
}