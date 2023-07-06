using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace SharedSpaceExperience
{
    public class PlayerController : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
    {
        [SerializeField]
        private HealthManager healthManager;

        public Shooter shooter;
        [SerializeField]
        private Shield shield;
        [SerializeField]
        private BulletPreview normalBulletPreview;
        [SerializeField]
        private BulletPreview chargeBulletPreview;

        [SerializeField]
        private GameObject leftControllerModel;
        [SerializeField]
        private GameObject rightControllerModel;

        // player appearence style
        public int role
        {
            get { return PhotonUtils.GetPlayerProperty<int>(photonView.Controller, PlayerManager.ROLE_KEY); }
            set { PhotonUtils.SetPlayerProperty(photonView.Controller, PlayerManager.ROLE_KEY, value); }
        }
        // has player aligned the coordinate
        public bool isAligned
        {
            get { return PhotonUtils.GetPlayerProperty<bool>(photonView.Controller, PlayerManager.ALIGN_KEY); }
            set { PhotonUtils.SetPlayerProperty(photonView.Controller, PlayerManager.ALIGN_KEY, value); }
        }
        // has player ready to play
        public bool isReady
        {
            get { return PhotonUtils.GetPlayerProperty<bool>(photonView.Controller, PlayerManager.READY_KEY); }
            set { PhotonUtils.SetPlayerProperty(photonView.Controller, PlayerManager.READY_KEY, value); }
        }
        // player health
        public int health
        {
            get { return PhotonUtils.GetPlayerProperty<int>(photonView.Controller, PlayerManager.HEALTH_KEY); }
            set { PhotonUtils.SetPlayerProperty(photonView.Controller, PlayerManager.HEALTH_KEY, value); }
        }

        // health bricks
        public bool[] healthBricks
        {
            get { return PhotonUtils.GetPlayerProperty<bool[]>(photonView.Controller, PlayerManager.HEALTH_BRICKS_KEY); }
            set { PhotonUtils.SetPlayerProperty(photonView.Controller, PlayerManager.HEALTH_BRICKS_KEY, value); }
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            // add to scene
            SceneManager sceneManager = GameObject.FindObjectOfType<SceneManager>();
            if (sceneManager != null)
            {
                sceneManager.AddPlayer(transform);
            }
        }

        private void Start()
        {
            shooter = GetComponent<Shooter>();
            shooter.enabled = false;

            // set preview material
            int style = role;
            Logger.Log("[PlayerController] role: " + style);
            if (style >= 0)
            {
                if (shield != null)
                {
                    shield.ownerID = style;
                    shield.SetStyle(style);
                }
                normalBulletPreview.SetStyle(style);
                chargeBulletPreview.SetStyle(style);
                // update health bricks
                healthManager.OnHealthUpdate();
            }
        }

        public bool OnDamaged(int brickIndex)
        {
            // compute damage by local player
            if (!photonView.IsMine) return false;

            bool[] bricks = healthBricks;
            if (!bricks[brickIndex]) return false;

            bricks[brickIndex] = false;
            Hashtable properties = new Hashtable{
                {PlayerManager.HEALTH_KEY, health - 1},
                {PlayerManager.HEALTH_BRICKS_KEY, bricks}
            };
            Logger.Log("[PlayerController] on damaged. health brick: " + brickIndex);
            PhotonUtils.SetPlayerProperty(photonView.Controller, properties);

            return true;
        }

        public void SetAbilitiesActive(bool active)
        {
            if (!photonView.IsMine || role < 0) return;
            if (!active)
            {
                shooter.StopShooting();
            }
            shooter.enabled = active;
            shield.SetActive(active);
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {

            if (changedProps.ContainsKey(PlayerManager.ALIGN_KEY))
            {
                // show controller when the space between local and remote players is aligned
                ShowControllerModels(isAligned && PlayerManager.IsLocalPlayerAligned());
            }

            // update all players health bricks
            if (changedProps.ContainsKey(PlayerManager.HEALTH_KEY) && targetPlayer == photonView.Controller)
            {
                // update health bricks
                Logger.Log("[PlayerController] health update");
                healthManager.OnHealthUpdate();
            }
        }

        public void ShowControllerModels(bool show)
        {
            leftControllerModel.SetActive(show);
            rightControllerModel.SetActive(show);
        }

    }
}