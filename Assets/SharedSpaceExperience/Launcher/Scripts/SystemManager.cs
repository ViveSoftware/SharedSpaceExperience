using UnityEngine;

namespace SharedSpaceExperience
{
    public class SystemManager : MonoBehaviour
    {
        public static SystemManager Instance { get; private set; }

        public Transform waveRig;
        public Transform head;
        public Transform rightController;
        public Transform leftController;

        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this) Destroy(gameObject);
        }

        private void OnDisable()
        {
            if (Instance == this) Instance = null;
        }
    }
}