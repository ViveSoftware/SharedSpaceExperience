using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public Transform target;

    private void Start()
    {
        target = Camera.main.transform;
    }

    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - target.position);
    }
}
