using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace SharedSpaceExperience
{
    public class PhotonDebugger : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private TMP_Text roomLogger;
        [SerializeField]
        private TMP_Text playerLogger;

        [SerializeField]
        private MatchManager matchManager;

        public Dictionary<int, string> playerInfo = new Dictionary<int, string>();

        private void Start()
        {
            // update properties
            // room properties
            UpdateRoomLogger();

            // player properties
            playerLogger.text = "";
            foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                playerInfo[player.ActorNumber] = player.ToStringFull();
                playerLogger.text += playerInfo[player.ActorNumber];
            }
        }

        public override void OnRoomPropertiesUpdate(Hashtable changedProps)
        {
            UpdateRoomLogger();
        }

        private void UpdateRoomLogger()
        {
            roomLogger.text = PhotonNetwork.CurrentRoom.ToStringFull();
            if (PhotonUtils.HasRoomProperty(MatchManager.ROUND_RESULT_KEY))
            {
                roomLogger.text += "\nround result: [";
                foreach (int winner in matchManager.roundResult)
                {
                    roomLogger.text += winner + ", ";
                }
                roomLogger.text += "]";
            }
            if (PhotonUtils.HasRoomProperty(MatchManager.MATCH_RESULT_KEY))
            {
                roomLogger.text += "\nmatch result: [";
                foreach (int winner in matchManager.matchResult)
                {
                    roomLogger.text += winner + ", ";
                }
                roomLogger.text += "]";
            }
            if (PhotonUtils.HasRoomProperty(MatchManager.MARKER_KEY))
            {
                roomLogger.text += "\nmarker: {";
                roomLogger.text += MarkerUtils.MarkerToLog(matchManager.marker);
                roomLogger.text += "}";
            }
        }

        private string playerInfoToString()
        {
            string tmp = "";
            foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                tmp += playerInfo[player.ActorNumber] + "\nhs: [";
                if (player.CustomProperties.ContainsKey(PlayerManager.HEALTH_BRICKS_KEY))
                {
                    foreach (bool shield in PhotonUtils.GetPlayerProperty<bool[]>(player, PlayerManager.HEALTH_BRICKS_KEY))
                    {
                        tmp += shield + ", ";
                    }
                    tmp += "]\n";
                }
            }
            return tmp;
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (targetPlayer.ActorNumber == -1) return;
            playerInfo[targetPlayer.ActorNumber] = targetPlayer.ToStringFull();
            playerLogger.text = playerInfoToString();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            playerInfo.Add(newPlayer.ActorNumber, newPlayer.ToStringFull());
            playerLogger.text = playerInfoToString();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            playerInfo.Remove(otherPlayer.ActorNumber);
            playerLogger.text = playerInfoToString();
        }
    }
}