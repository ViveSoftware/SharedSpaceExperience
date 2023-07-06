using UnityEngine;
using UnityEngine.InputSystem;

public class DebugManager : MonoBehaviour
{
    [SerializeField]
    private bool isDebugMode = false;
    public InputAction debugAction;
    public GameObject[] DebugObjects;
    public DebugModelController[] models;

    private void OnEnable()
    {
        debugAction.Enable();
        debugAction.started += ToggleDebugMode;
    }

    private void OnDisable()
    {
        debugAction.started -= ToggleDebugMode;
        debugAction.Disable();
    }

    private void Start()
    {
        SetDebugMode(isDebugMode);
    }

    public void SetDebugMode(bool debugMode)
    {
        isDebugMode = debugMode;

        for (int i = 0; i < DebugObjects.Length; ++i)
        {
            DebugObjects[i].SetActive(isDebugMode);
        }

        models = GameObject.FindObjectsOfType<DebugModelController>();
        foreach (DebugModelController model in models)
        {
            model.SetActive(isDebugMode);
        }
    }

    public void ToggleDebugMode(InputAction.CallbackContext context)
    {
        SetDebugMode(!isDebugMode);
    }
}
