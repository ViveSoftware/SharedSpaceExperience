using UnityEngine;

namespace SharedSpaceExperience
{
    public class HealthManager : MonoBehaviour
    {
        public PlayerController player;
        [SerializeField]
        private Transform head;

        [SerializeField]
        private GameObject[] healthBrickPrefabs;

        private HealthBrick[] healthBricks = new HealthBrick[MAX_HEALTH];

        private Vector3[] position = {
            new Vector3(0, 0.18f, 0),
            new Vector3(0.18f, 0, 0),
            new Vector3(-0.18f, 0, 0),
            new Vector3(0, -0.18f, 0)
        };

        private const float DAMAGE_CD = 1;
        public const int MAX_HEALTH = 4;

        private bool inited = false;
        private float nextDamageTime = 0;

        public float distance;
        public float height;

        private void Start()
        {
            Init();
        }

        private void Update()
        {
            // follow player
            Vector3 playerForward = Vector3.Cross(Vector3.Cross(Vector3.up, head.forward).normalized, Vector3.up).normalized;
            transform.position = distance * playerForward + head.position + new Vector3(0, height, 0);
            transform.rotation = Quaternion.LookRotation(playerForward, Vector3.up);

        }

        private void Init()
        {
            if (inited) return;
            int id = player.role;
            // create bricks
            for (int i = 0; i < MAX_HEALTH; ++i)
            {
                GameObject brick = Instantiate(healthBrickPrefabs[id], transform);
                brick.transform.localPosition = position[i];
                healthBricks[i] = brick.GetComponent<HealthBrick>();
                healthBricks[i].Init(id, i, this);
                healthBricks[i].gameObject.SetActive(false);
            }

            inited = true;
        }

        public bool OnDamaged(int index)
        {
            // CD block and try update to server
            Logger.Log("[HealthManager] on damage");
            if (Time.time < nextDamageTime || !player.OnDamaged(index)) return false;
            Logger.Log("[HealthManager] success");
            nextDamageTime = Time.time + DAMAGE_CD;

            return true;
        }

        public void OnHealthUpdate()
        {
            if (!inited) Init();
            for (int i = 0; i < MAX_HEALTH; ++i)
            {
                if (player.healthBricks[i])
                {
                    healthBricks[i].gameObject.SetActive(true);
                    healthBricks[i].ResetBrick();

                    // invincible animation
                    healthBricks[i].PlayInvincibleAnimation();
                }
                else
                {
                    healthBricks[i].gameObject.SetActive(false);
                }
            }
        }
    }
}