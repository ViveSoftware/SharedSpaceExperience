using UnityEngine;

namespace SharedSpaceExperience
{
    public class SystemSpawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject systemPrefab;

        private void OnEnable()
        {
            if (SystemManager.Instance == null)
            {
                Instantiate(systemPrefab, Vector3.zero, Quaternion.identity);
            }
        }
    }
}