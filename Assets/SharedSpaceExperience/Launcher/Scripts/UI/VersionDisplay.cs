using UnityEngine;

using TMPro;

namespace SharedSpaceExperience.UI
{
    public class VersionDisplay : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text text;

        void Start()
        {
            text.text = "v" + Application.version;
        }
    }
}
