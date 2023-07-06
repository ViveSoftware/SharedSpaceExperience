using UnityEngine;

namespace SharedSpaceExperience
{
    public class AlignmentManager : MonoBehaviour
    {
        [SerializeField]
        private Transform waveRig;
        [SerializeField]
        private MatchManager matchManager;
        [SerializeField]
        private MarkerManager markerManager;

        [SerializeField]
        [Tooltip("Assume all players share the same y-axis.")]
        private bool correctYAxis = true;
        [SerializeField]
        [Tooltip("Assume all players have the same floor height.")]
        private bool correctHeight = true;

        // [SerializeField]
        private Vector3 markerPosInHostSpace;
        // [SerializeField]
        private Quaternion markerRotInHostSpace;
        // [SerializeField]
        private Vector3 markerPosInClientSpace;
        // [SerializeField]
        private Quaternion markerRotInClientSpace;
        // [SerializeField]
        private Vector3 hostOriginPosInClientSpace;
        // [SerializeField]
        private Quaternion hostOriginRotInClientSpace;

        public void Align()
        {
            if (markerManager.selectedMarker == null)
            {
                Debug.LogError("[AlignmentManager] No selected marker");
                return;
            }

            // get the transformation between marker and origin 
            markerPosInHostSpace = MarkerUtils.GetPosition(matchManager.marker);
            markerRotInHostSpace = MarkerUtils.GetRotation(matchManager.marker);

            markerPosInClientSpace = markerManager.selectedMarker.transform.position;
            markerRotInClientSpace = markerManager.selectedMarker.transform.rotation;

            // compute host origin pose in client space, which will be used as new client origin
            hostOriginRotInClientSpace = markerRotInClientSpace * Quaternion.Inverse(markerRotInHostSpace);
            hostOriginPosInClientSpace = markerPosInClientSpace - (hostOriginRotInClientSpace * markerPosInHostSpace);

            // assume host and client have the same up vector (y axis)
            // correct marker pose to make the computed host y axis be the same as the client
            if (correctYAxis)
            {
                // adjust rotation
                Vector3 hostYAxisInClientSpace = hostOriginRotInClientSpace * Vector3.up;
                Quaternion adjustRot = Quaternion.FromToRotation(hostYAxisInClientSpace, Vector3.up);

                // rotate marker such that the recomputed host y axis matches client y axis
                markerRotInClientSpace = adjustRot * markerRotInClientSpace;
                // recompute host origin pose
                hostOriginRotInClientSpace = markerRotInClientSpace * Quaternion.Inverse(markerRotInHostSpace);
                hostOriginPosInClientSpace = markerPosInClientSpace - (hostOriginRotInClientSpace * markerPosInHostSpace);
            }
            // assume the host and the client have the same floor height
            // this assumption depends on how well the room setup have been done
            if (correctHeight)
            {
                hostOriginPosInClientSpace.y = 0;
            }

            // instead of moving the whole scene to the new origin
            // here we move the tracked devices in the opposite direction to get the same effect
            waveRig.position = waveRig.position - (Quaternion.Inverse(hostOriginRotInClientSpace) * hostOriginPosInClientSpace);
            waveRig.rotation = waveRig.rotation * Quaternion.Inverse(hostOriginRotInClientSpace);
        }

    }
}
