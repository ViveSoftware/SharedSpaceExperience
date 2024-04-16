using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.UI
{
    public class FollowHand : MonoBehaviour
    {
        private Transform head;
        private Transform hand;

        [SerializeField]
        private AlignManager alignManager;

        [SerializeField]
        private float distance;

        [SerializeField]
        private InputAction fixAction;

        private bool isFixed = true;

        private void OnEnable()
        {
            StartCoroutine(WaitForManagers());
            fixAction.Enable();
            fixAction.started += EnableMoveMode;
            fixAction.canceled += DisableMoveMode;

            alignManager.OnTrackedSpaceUpdated += ResetPose;
        }

        private IEnumerator WaitForManagers()
        {
            yield return new WaitUntil(() => SystemManager.Instance);
            head = SystemManager.Instance.head;
            hand = SystemManager.Instance.leftController;

            // wait until head transform update
            yield return new WaitUntil(() => UserManager.Instance);

            // init pose
            ResetPose();
        }

        private void OnDisable()
        {
            fixAction.started -= EnableMoveMode;
            fixAction.canceled -= DisableMoveMode;
            fixAction.Disable();

            alignManager.OnTrackedSpaceUpdated -= ResetPose;
        }

        private void EnableMoveMode(InputAction.CallbackContext context)
        {
            isFixed = false;
        }

        private void DisableMoveMode(InputAction.CallbackContext context)
        {
            isFixed = true;
        }

        private void Update()
        {
            if (isFixed || head == null || hand == null) return;

            Vector3 forward = hand.position - head.position;
            forward = Quaternion.AngleAxis(30, Vector3.up) * forward;
            UpdatePose(forward);
        }

        public void ResetPose()
        {
            Logger.Log($"system pose: {head.position} {head.rotation}");
            UpdatePose(head == null ? Vector3.forward : head.forward);
        }

        private void UpdatePose(Vector3 forward)
        {
            forward.y = 0;
            forward = Vector3.Normalize(forward);
            if (forward == Vector3.zero) forward = Vector3.forward;

            transform.SetPositionAndRotation(
                distance * forward + head.position,
                Quaternion.LookRotation(forward, Vector3.up)
            );
        }
    }
}