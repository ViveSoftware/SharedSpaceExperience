using UnityEngine;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.UI
{
    public class AppListItem : MonoBehaviour
    {
        [SerializeField]
        private HostAppsPage page;

        [SerializeField]
        private GameObject appInfoPrefab;

        private readonly AppInfo appInfo;

        private void OnEnable()
        {
            if (!appInfoPrefab.TryGetComponent(out AppInfo appInfo))
            {
                Logger.LogError("Failed to found App info");
            }
        }

        public void OnSelected()
        {
            page.OnSelectApp(appInfo);
        }
    }
}