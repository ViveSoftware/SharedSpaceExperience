using UnityEngine;

namespace SharedSpaceExperience
{
    public class HealthBrick : MonoBehaviour
    {
        public int ownerID = -1;
        private int shieldIndex = -1;
        private bool enable = true;
        private HealthManager healthManager;

        [SerializeField]
        private Animator animator;

        public void Init(int playerID, int index, HealthManager manager)
        {
            ownerID = playerID;
            shieldIndex = index;
            healthManager = manager;
            enabled = true;
        }

        public void ResetBrick()
        {
            enable = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            Logger.Log("[HealthBrick] hit by " + other.name);

            // check if is hit by opponent's bullet
            if (!enable || other.gameObject?.tag != "Bullet" ||
                other.GetComponentInParent<Bullet>()?.ownerID == ownerID) return;

            // try send on damage event to server
            if (healthManager.OnDamaged(shieldIndex))
            {
                // temporary disable shield and wait for server update
                enable = false;
            }
        }


        public void PlayInvincibleAnimation()
        {
            animator.Play("Invincible", -1, 0);
        }

    }
}