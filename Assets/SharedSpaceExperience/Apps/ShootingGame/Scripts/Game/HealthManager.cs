using UnityEngine;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.Example
{
    public class HealthManager : MonoBehaviour
    {
        [SerializeField]
        private PlayerProperty player;

        [SerializeField]
        private Transform head;

        [SerializeField]
        private HealthBrick[] bricks;

        public bool isVisible = false;
        public bool isAbilityActive = false;

        [SerializeField]
        private float distanceToHead;

        [SerializeField]
        private float height;

        public void ResetHealth()
        {
            foreach (HealthBrick brick in bricks)
            {
                brick.isHealthy = true;
            }
            UpdateHealth();
        }

        /* Visiblity */
        public void UpdateVisibility()
        {
            isVisible = player.isVisible;
            foreach (HealthBrick brick in bricks)
            {
                brick.UpdateVisibility();
            }
        }

        /* Ability */
        public void UpdateAbilityActive()
        {
            isAbilityActive = player.isAbilityActive && isVisible;
            foreach (HealthBrick brick in bricks)
            {
                brick.UpdateAbilityActive();
            }
        }

        public void UpdateHealth()
        {
            if (!player.IsServer) return;

            // check current health
            int health = 0;
            foreach (HealthBrick brick in bricks)
            {
                if (brick.isHealthy) ++health;
            }
            Logger.Log("update: " + health);
            // update player health
            player.health.Value = health;

        }

        private void Update()
        {
            // update pose
            // Warning: This pose is sync through parent's network transform
            Vector3 playerForward = Vector3.Cross(Vector3.Cross(Vector3.up, head.forward).normalized, Vector3.up).normalized;
            transform.SetPositionAndRotation(
                distanceToHead * playerForward + head.position + new Vector3(0, height, 0),
                Quaternion.LookRotation(playerForward, Vector3.up)
            );
        }
    }
}