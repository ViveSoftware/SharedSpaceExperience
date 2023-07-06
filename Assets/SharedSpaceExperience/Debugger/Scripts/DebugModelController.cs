using UnityEngine;

public class DebugModelController : MonoBehaviour
{
    [SerializeField]
    private GameObject model;

    public void SetActive(bool active)
    {
        model.SetActive(active);
    }
}
