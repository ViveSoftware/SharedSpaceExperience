using UnityEngine;
using Wave.Native;
using SharedSpaceExperience;

public class DebugFakeMarker : MonoBehaviour
{
    [SerializeField]
    private MarkerManager markerManager;
    [SerializeField]
    private Marker marker;

    [SerializeField]
    private string uuid = "fake-marker";
    [SerializeField]
    private ulong trackerId;
    [SerializeField]
    private float scale;
    [SerializeField]
    private Vector3 position;
    [SerializeField]
    private Quaternion rotation;

    private WVR_ArucoMarker GenFakeMarker()
    {
        WVR_ArucoMarker aruco = new WVR_ArucoMarker();
        aruco.uuid.data = System.Text.Encoding.UTF8.GetBytes(uuid);
        aruco.trackerId = trackerId;
        aruco.size = scale;
        aruco.pose.position.v0 = position.x;
        aruco.pose.position.v1 = position.y;
        aruco.pose.position.v2 = position.z;
        aruco.pose.rotation.x = rotation.x;
        aruco.pose.rotation.y = rotation.y;
        aruco.pose.rotation.z = rotation.z;
        aruco.pose.rotation.w = rotation.w;

        return aruco;
    }

    void Start()
    {
        marker.Init(markerManager, GenFakeMarker());
    }

    void Update()
    {
        marker.UpdateMarker(GenFakeMarker());
    }
}
