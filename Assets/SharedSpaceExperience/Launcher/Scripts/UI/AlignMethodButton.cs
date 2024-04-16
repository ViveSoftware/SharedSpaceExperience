using UnityEngine;

namespace SharedSpaceExperience.UI
{
    public class AlignMethodButton : MonoBehaviour
    {
        [SerializeField]
        private AlignManager.AlignMethod method;

        [SerializeField]
        private HostAlignPage hostAlignPage;

        public void OnToggleChanged(bool value)
        {
            if (value) hostAlignPage.SetAlignMethod(method);
        }
    }
}