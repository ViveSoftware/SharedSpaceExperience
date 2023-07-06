using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform playerHMD;

    public float distance;
    public float height;

    private void Update()
    {
        Vector3 playerForward = Vector3.Cross(Vector3.Cross(Vector3.up, playerHMD.forward).normalized, Vector3.up).normalized;
        transform.position = distance * playerForward + new Vector3(playerHMD.position.x, height, playerHMD.position.z);
        transform.rotation = Quaternion.LookRotation(playerForward, Vector3.up);
    }
}
