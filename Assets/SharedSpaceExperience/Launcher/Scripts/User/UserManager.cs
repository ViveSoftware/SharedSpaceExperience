using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience
{
    public class UserManager : MonoBehaviour
    {
        public static UserManager Instance { get; private set; }
        public LocalUserProperty localUserProperty;

        public static Dictionary<ulong, UserProperty> UserProperties = new();

        public static Action<ulong, FixedString32Bytes> OnUserNameChanged;
        public static Action<ulong, FixedString32Bytes> OnUserIPChanged;
        public static Action<ulong, bool> OnUserIsAlignedChanged;
        public static Action<ulong, bool> OnUserIsReadyChanged;

        public static Action OnLocalUserSpawned;
        public static Action OnLocalUserDespawned;
        public static Action<ulong> OnUserConnected;
        public static Action<ulong> OnUserDisconnected;
        public static Action OnUserCountChanged;

        public static bool debugShowUserDefaultModel = false; // for debug: show all models no matter is aligned or not
        public static bool previewUserDefaultModel = false; // for align check: show before local set aligned
        public static bool showUserDefaultModel = false; // for game logic: only show when aligned

        [HideInInspector]
        public Transform headSource;
        [HideInInspector]
        public Transform rightControllerSource;
        [HideInInspector]
        public Transform leftControllerSource;

        private void OnEnable()
        {
            // singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this) Destroy(gameObject);

            StartCoroutine(WaitForManagers());
        }

        private IEnumerator WaitForManagers()
        {
            yield return new WaitUntil(() => SystemManager.Instance);
            headSource = SystemManager.Instance.head;
            rightControllerSource = SystemManager.Instance.rightController;
            leftControllerSource = SystemManager.Instance.leftController;
        }

        public static UserProperty GetLocalUser()
        {
            return UserProperties.ContainsKey(NetworkManager.Singleton.LocalClientId) ?
                UserProperties[NetworkManager.Singleton.LocalClientId] : null;
        }

        public static bool TryGetLocalUser(out UserProperty localUser)
        {
            if (NetworkManager.Singleton == null)
            {
                localUser = null;
                return false;
            }
            return UserProperties.TryGetValue(NetworkManager.Singleton.LocalClientId, out localUser);
        }

        public void ResetAllUserIsReady(bool includeHost = true)
        {
            // only invoked by server
            if (!NetworkController.Instance.isServer) return;
            foreach (ulong uid in UserProperties.Keys)
            {
                if (includeHost || uid != NetworkManager.Singleton.LocalClientId)
                {
                    UserProperties[uid].isReady.Value = false;
                }
            }
        }

        public bool IsAllUserReady()
        {
            foreach (ulong uid in UserProperties.Keys)
            {
                if (!UserProperties[uid].isReady.Value) return false;
            }

            return true;
        }

        public void UpdateAllUserModelVisibility()
        {
            foreach (UserProperty user in UserProperties.Values)
            {
                user.UpdateUserModelVisibility();
            }
        }

        public void ShowUserDefaultModel(bool show)
        {
            Logger.Log(show.ToString());
            showUserDefaultModel = show;
            UpdateAllUserModelVisibility();
        }

        public void PreviewUserDefaultModel(bool show)
        {
            Logger.Log(show.ToString());
            previewUserDefaultModel = show;
            UpdateAllUserModelVisibility();
        }

        public void ForceShowUserDefaultModel(bool show)
        {
            Logger.Log(show.ToString());
            debugShowUserDefaultModel = show;
            UpdateAllUserModelVisibility();
        }

    }
}
