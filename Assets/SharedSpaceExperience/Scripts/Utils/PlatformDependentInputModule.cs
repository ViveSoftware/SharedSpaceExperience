using UnityEngine;
using UnityEngine.InputSystem.UI;

public class PlatformDependentInputModule : MonoBehaviour
{
    public InputSystemUIInputModule inputSystemUIInputModule;

    void OnEnable()
    {
        // Use input system UI input module only in unity editor
        bool isInEditor = false;
#if UNITY_EDITOR
        isInEditor = true;
#endif
        inputSystemUIInputModule.enabled = isInEditor;
    }

}
