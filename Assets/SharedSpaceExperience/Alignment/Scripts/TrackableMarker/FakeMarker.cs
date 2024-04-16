#if UNITY_EDITOR || !UNITY_ANDROID
#define PC_DEBUG
#endif

using System.Collections;
using UnityEngine;
using Wave.Native;
using SharedSpaceExperience;

public class FakeMarker : MonoBehaviour
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

    private void OnEnable()
    {
        // the fake marker is only used for PC debug
#if PC_DEBUG
        StartCoroutine(WaitForManagers());
#else
        Destroy(gameObject);
#endif

    }

    private IEnumerator WaitForManagers()
    {
        yield return new WaitUntil(() => DebugManager.Instance);
        DebugManager.Instance.AddDebugObject(marker.gameObject);
    }

    private void OnDisable()
    {
        DebugManager.Instance.RemoveDebugObject(marker.gameObject);
    }

    private void Start()
    {
        marker.Init(markerManager, GenFakeMarker());
    }

    private void Update()
    {
        marker.UpdateMarker(GenFakeMarker());
    }

    private WVR_ArucoMarker GenFakeMarker()
    {
        WVR_ArucoMarker aruco = new();
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
}
