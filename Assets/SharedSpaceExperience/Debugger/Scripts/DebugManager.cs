#if UNITY_EDITOR || !UNITY_ANDROID
#define PC_DEBUG
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

using SharedSpaceExperience;

public class DebugManager : MonoBehaviour
{

    public static DebugManager Instance { get; private set; }

    [SerializeField]
    private InputSystemUIInputModule inputSystemUIInputModule;
    [SerializeField]
    private bool hideVRObjects = true;
    [SerializeField]
    private List<GameObject> vrObjects = new();
    [SerializeField]
    private List<GameObject> debugObjects = new();

    [SerializeField]
    private InputAction debugModeAction;

    private bool isDebugMode = false;

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this) Destroy(this);

#if PC_DEBUG
        if (hideVRObjects)
        {
            // hide VR object 
            foreach (GameObject obj in vrObjects)
            {
                obj.SetActive(false);
            }
        }
        // enable PC input system
        inputSystemUIInputModule.enabled = true;
#else
        inputSystemUIInputModule.enabled = false;
#endif

        debugModeAction.Enable();
        debugModeAction.started += ToggleDebugObjects;

        ShowDebugObjects();
    }

    private void OnDisable()
    {
        debugModeAction.started -= ToggleDebugObjects;
        debugModeAction.Disable();
    }

    private void ToggleDebugObjects(InputAction.CallbackContext context)
    {
        isDebugMode = !isDebugMode;
        ShowDebugObjects();

    }

    private void ShowDebugObjects()
    {
        foreach (GameObject obj in debugObjects)
        {
            obj.SetActive(isDebugMode);
        }

        // show user model
        if (UserManager.Instance != null)
        {
            UserManager.Instance.ForceShowUserDefaultModel(isDebugMode);
        }
    }

    public void AddDebugObject(GameObject obj)
    {
        debugObjects.Add(obj);
        obj.SetActive(isDebugMode);
    }

    public void RemoveDebugObject(GameObject obj)
    {
        debugObjects.Remove(obj);
    }

}
