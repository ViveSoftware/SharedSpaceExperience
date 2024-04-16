using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using TMPro;

using Logger = Debugger.Logger;
using System.Collections;

namespace SharedSpaceExperience.UI
{
    public class UserNameTag : MonoBehaviour
    {
        [SerializeField]
        private UserProperty user;
        private Transform head;

        [SerializeField]
        private TMP_Text userName;
        [SerializeField]
        private Image arrow;


        [SerializeField]
        private Color readyColor;
        [SerializeField]
        private Color notReadyColor;

        private void OnEnable()
        {
            // hide from local user
            if (user.IsLocalPlayer)
            {
                gameObject.SetActive(false);
                return;
            }

            // register callbacks
            user.userName.OnValueChanged += OnUserNameChanged;
            user.isReady.OnValueChanged += OnIsReadyChanged;

            // update UI
            UpdateName();
            UpdateArrow();

            StartCoroutine(WaitForManagers());
        }

        private IEnumerator WaitForManagers()
        {
            yield return new WaitUntil(() => SystemManager.Instance);
            head = SystemManager.Instance.head;
        }

        private void OnDisable()
        {
            // deregister callbacks
            user.userName.OnValueChanged -= OnUserNameChanged;
            user.isReady.OnValueChanged -= OnIsReadyChanged;
        }

        private void OnUserNameChanged(FixedString32Bytes previous, FixedString32Bytes current)
        {
            UpdateName();
        }

        private void OnIsReadyChanged(bool previous, bool current)
        {
            UpdateArrow();
        }

        private void UpdateName()
        {
            userName.text = user.userName.Value.ToString();
        }

        private void UpdateArrow()
        {
            arrow.color = user.isReady.Value ? readyColor : notReadyColor;
        }

        private void Update()
        {
            transform.position = transform.parent.position + new Vector3(0, 0.25f, 0);

            if (head == null) return;
            Vector3 forward = transform.position - head.position;
            forward.y = 0;
            forward = Vector3.Normalize(forward);

            if (forward == Vector3.zero) return;
            transform.rotation = Quaternion.LookRotation(forward);
        }
    }
}