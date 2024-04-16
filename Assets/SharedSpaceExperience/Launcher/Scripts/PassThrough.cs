using UnityEngine;
using Wave.Native;

namespace SharedSpaceExperience
{
    public class PassThrough : MonoBehaviour
    {
        private void OnDestroy()
        {
            Interop.WVR_ShowPassthroughUnderlay(false);
        }

        void Start()
        {
            Interop.WVR_ShowPassthroughUnderlay(true);
            Interop.WVR_SetPassthroughImageFocus(WVR_PassthroughImageFocus.Scale);
        }

        void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                Interop.WVR_ShowPassthroughUnderlay(true);
            }
        }
    }
}