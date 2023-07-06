using UnityEngine;

using TMPro;

public class VersionDisplay : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text;

    void Start()
    {
        text.text = "v" + Application.version;
    }
}
