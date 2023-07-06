using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace SharedSpaceExperience
{
    public class PlayerManager : MonoBehaviourPunCallbacks
    {
        public static PlayerController localPlayer = null;
        [SerializeField]
        private SceneManager sceneManager;
        [SerializeField]
        private GameObject playerPrefab;

        [Header("Tracked Source Object")]
        [SerializeField]
        private Transform head;
        [SerializeField]
        private Transform leftController;
        [SerializeField]
        private Transform rightController;

        /* Player Properties */
        // player property keys
        public const string ROLE_KEY = "role"; // >= 0: players, -1: spectators (currently not allowed)
        public const string ALIGN_KEY = "align";
        public const string READY_KEY = "ready";
        public const string HEALTH_KEY = "health";
        public const string HEALTH_BRICKS_KEY = "bricks";

        public static void InitLocalPlayerProperties(int playerID)
        {
            Debug.Log("[PlayerManager] init player: " + playerID);
            Hashtable properties = new Hashtable{
                {ROLE_KEY, playerID},
                {READY_KEY, false},
                {ALIGN_KEY, false},
                {HEALTH_KEY, 0},
                {HEALTH_BRICKS_KEY, new bool[4]{false, false, false, false}}
            };
            PhotonUtils.SetPlayerProperty(PhotonNetwork.LocalPlayer, properties);
        }

        public static bool IsPlayer(Player player)
        {
            return PhotonUtils.GetPlayerProperty<int>(player, ROLE_KEY) >= 0;
        }

        public static int GetPlayerRole(Player player)
        {
            // may request when player not initialized 
            bool success = PhotonUtils.TryGetPlayerProperty(player, ROLE_KEY, out object role);

            return success ? (int)role : -2;
        }

        public static void SetPlayerRole(Player player, int role)
        {
            // may request when player not initialized 
            PhotonUtils.SetPlayerProperty(player, ROLE_KEY, role);
        }

        public static bool IsPlayerReady(Player player)
        {
            // player is not spectator and has scanned and is ready
            if (!(PhotonUtils.HasPlayerProperty(player, ROLE_KEY) &&
                  PhotonUtils.HasPlayerProperty(player, READY_KEY)))
            {
                Logger.Log("[PlayerManager] player has no property role or ready");
                return false;
            }
            return PhotonUtils.GetPlayerProperty<int>(player, ROLE_KEY) < 0 ||
                PhotonUtils.GetPlayerProperty<bool>(player, READY_KEY);
        }

        public static bool IsPlayerAligned(Player player)
        {
            // player is aligned
            return PhotonUtils.GetPlayerProperty<int>(player, ROLE_KEY) < 0 ||
                PhotonUtils.GetPlayerProperty<bool>(player, ALIGN_KEY);
        }

        public static bool IsPlayerLose(Player player)
        {
            // player is spectator or has 0 health
            return PhotonUtils.GetPlayerProperty<int>(player, ROLE_KEY) < 0 ||
                PhotonUtils.GetPlayerProperty<int>(player, HEALTH_KEY) <= 0;
        }

        public static void ResetPlayer(Player player)
        {
            Hashtable properties = new Hashtable{
                {READY_KEY, false},
                {ALIGN_KEY, false},
                {HEALTH_KEY, 4},
                {HEALTH_BRICKS_KEY, new bool[4]{true, true, true, true}}
            };
            PhotonUtils.SetPlayerProperty(player, properties);
        }

        // Local player
        public static bool IsLocalPlayerReady()
        {
            return IsPlayerReady(PhotonNetwork.LocalPlayer);
        }

        public static bool IsLocalPlayerAligned()
        {
            return IsPlayerAligned(PhotonNetwork.LocalPlayer);
        }

        public static int GetLocalPlayerRole()
        {
            return GetPlayerRole(PhotonNetwork.LocalPlayer);
        }

        public static void SetLocalPlayerReady(bool ready)
        {
            PhotonUtils.SetPlayerProperty(PhotonNetwork.LocalPlayer, READY_KEY, ready);
        }

        public static void SetLocalPlayerAligned(bool aligned)
        {
            PhotonUtils.SetPlayerProperty(PhotonNetwork.LocalPlayer, ALIGN_KEY, aligned);
        }


        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            // create player object after player prop is set
            if (localPlayer == null && targetPlayer == PhotonNetwork.LocalPlayer)
            {
                GameObject playerObject = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
                localPlayer = playerObject.GetComponent<PlayerController>();

                playerObject.GetComponent<SyncPose>().SetSources(
                    head, leftController, rightController);

                Logger.Log("[PlayerManager] Joined and spawn player");
            }
        }

        public override void OnLeftRoom()
        {
            if (localPlayer != null)
            {
                PhotonNetwork.Destroy(localPlayer.gameObject);
                Logger.Log("[PlayerManager] Destroy local player");
            }
        }

        public void ShowAllControllerModels(bool show)
        {
            foreach (PlayerController player in sceneManager.GetComponentsInChildren<PlayerController>())
            {
                player.ShowControllerModels(show);
            }
        }
    }
}