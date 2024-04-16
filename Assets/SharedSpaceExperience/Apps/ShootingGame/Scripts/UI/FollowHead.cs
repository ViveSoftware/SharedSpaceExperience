using System.Collections;
using UnityEngine;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.Example
{
    public class FollowHead : MonoBehaviour
    {
        [SerializeField]
        private Transform head;

        [SerializeField]
        private float distance;

        private void OnEnable()
        {
            StartCoroutine(WaitForManagers());
        }

        private IEnumerator WaitForManagers()
        {
            yield return new WaitUntil(() => SystemManager.Instance);
            head = SystemManager.Instance.head;
        }

        private void Update()
        {
            if (head == null) return;
            Vector3 forward = Vector3.Cross(Vector3.Cross(Vector3.up, head.forward).normalized, Vector3.up).normalized;
            transform.position = distance * forward + head.position;

            if (forward == Vector3.zero) return;
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }
    }
}