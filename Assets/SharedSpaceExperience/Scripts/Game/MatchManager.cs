using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Wave.Native;

using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace SharedSpaceExperience
{
    public enum MatchState
    {
        INITIALIZING,   // wait for host select marker
        READY,          // wait for all player to be ready
        PLAYING,        // game play
        WAITING,        // wait for all player finish animation after each round
        ENDING,         // end of match 
        ERROR           // player leaves while playing
    }

    // only controlled by master client
    public class MatchManager : MonoBehaviourPunCallbacks
    {

        [SerializeField]
        private MatchUIManager ui;

        [SerializeField]
        private SceneManager sceneManager;

        [SerializeField]
        private SceneTransition sceneTransition;
        [SerializeField]
        private PlayerManager playerManager;

        [SerializeField]
        private MarkerManager markerManager;

        /* Game Parameters */
        public const int NUM_PLAYERS = 2;
        public const int MAX_ROUND = 1;   // 3
        private const int MAX_POINTS = 1; // 4
        private const int POINTS_WIN = 1; // 2
        private const int POINTS_DRAW = 1;
        private const double MAX_TIME = 99;
        private const double SYNC_DELAY = 2;
        // temporary hard code animation length
        private const double START_ANIMATION_LENGTH = 3 + SYNC_DELAY;

        /* Room Properties */
        // room property keys
        public const string STATE_KEY = "match_state";
        public const string MARKER_KEY = "marker";
        public const string TIMER_KEY = "start_time";
        public const string ROUND_RESULT_KEY = "round_result";
        public const string MATCH_RESULT_KEY = "match_result";

        // match state
        private MatchState _matchState = MatchState.INITIALIZING;
        public MatchState matchState
        {
            get { return _matchState; }
            set { PhotonUtils.SetRoomProperty(STATE_KEY, value); }
        }
        // marker
        private WVR_ArucoMarker _marker;
        public WVR_ArucoMarker marker
        {
            get { return _marker; }
            set { PhotonUtils.SetRoomProperty(MARKER_KEY, MarkerUtils.SerializeMarker(value)); }
        }
        // count down
        private double countDown = MAX_TIME;
        // timer start time
        private double _startTime = -1; // -1: before start, -2: pause timer
        public double startTime
        {
            get { return _startTime; }
            set { PhotonUtils.SetRoomProperty(TIMER_KEY, value); }
        }
        // round winner
        private int[] _roundResult = new int[0];
        public int[] roundResult
        {
            get { return _roundResult; }
            set { PhotonUtils.SetRoomProperty(ROUND_RESULT_KEY, value); }
        }
        // match result (0: lose, 1: draw, 2: win)
        public int[] matchResult
        {
            get { return PhotonUtils.GetRoomProperty<int[]>(MATCH_RESULT_KEY); }
            set { PhotonUtils.SetRoomProperty(MATCH_RESULT_KEY, value); }
        }

        public static RoomOptions GetInitialRoomOptions(bool isVisible)
        {
            // room option
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = NUM_PLAYERS;
            roomOptions.IsVisible = isVisible;
            roomOptions.IsOpen = true;

            // room properties
            roomOptions.CustomRoomProperties = new Hashtable{
                {STATE_KEY, MatchState.INITIALIZING},
                {TIMER_KEY, -1.0},
                {ROUND_RESULT_KEY, new int[0]},
                {MATCH_RESULT_KEY, new int[0]}
            };

            return roomOptions;
        }

        public static int AllocatePlayerID()
        {
            bool[] hasPlayer = new bool[NUM_PLAYERS];
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                // skip local player
                if (player == PhotonNetwork.LocalPlayer) continue;

                int id = PlayerManager.GetPlayerRole(player);
                if (id >= 0) hasPlayer[id] = true;
            }

            for (int i = 0; i < NUM_PLAYERS; ++i)
            {
                Debug.Log("[MatchManager] has player " + i + " " + hasPlayer[i]);
                if (!hasPlayer[i]) return i;
            }
            Debug.Log("[MatchManager] no available player id");
            return -1;
        }

        private void CheckPlayerRoles()
        {
            bool[] hasPlayer = new bool[NUM_PLAYERS];
            List<Player> duplicatedPlayer = new List<Player>();
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                int id = PlayerManager.GetPlayerRole(player);
                if (id >= 0)
                {
                    if (hasPlayer[id])
                    {
                        // player role reused
                        Logger.Log("[MatchManager] player role reused " + id);
                        duplicatedPlayer.Add(player);
                    }
                    hasPlayer[id] = true;
                }
            }

            int i = 0;
            foreach (Player player in duplicatedPlayer)
            {
                // reassign id if duplicated
                while (hasPlayer[i] && i < NUM_PLAYERS) ++i;
                Logger.Log("[MatchManager] reassign role " + i);
                PlayerManager.SetPlayerRole(player, i < NUM_PLAYERS ? i : -1);
            }
        }

        private void CheckPlayers(out bool enoughPlayer, out bool allReady)
        {
            int numPlayer = 0;
            int numReadyPlayer = 0;
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (PlayerManager.IsPlayer(player))
                {
                    numPlayer++;
                    if (PlayerManager.IsPlayerReady(player)) numReadyPlayer++;
                }
            }
            enoughPlayer = numPlayer == NUM_PLAYERS;
            allReady = numReadyPlayer == NUM_PLAYERS;
        }

        private void Awake()
        {
            // init local player properties
            int id = MatchManager.AllocatePlayerID();
            PlayerManager.InitLocalPlayerProperties(id);
            Logger.Log("[MatchManager] Spawn player " + id);
        }

        private void Start()
        {
            // initialize properties
            if (PhotonNetwork.IsMasterClient)
            {
                _roundResult = new int[0];
                matchResult = new int[0];
            }

            // get room properties
            if (PhotonUtils.HasRoomProperty(STATE_KEY))
            {
                _matchState = PhotonUtils.GetRoomProperty<MatchState>(STATE_KEY);
            }
            if (PhotonUtils.HasRoomProperty(MARKER_KEY))
            {
                _marker = MarkerUtils.DeserializeMarker(PhotonUtils.GetRoomProperty<Byte[]>(MARKER_KEY));
                markerManager.SetTargetMarker(_marker);
                Logger.Log("[MatchManager] Get target marker: " + MarkerUtils.MarkerToLog(marker));
            }
            if (PhotonUtils.HasRoomProperty(TIMER_KEY))
            {
                _startTime = PhotonUtils.GetRoomProperty<double>(TIMER_KEY);
            }
            if (PhotonUtils.HasRoomProperty(ROUND_RESULT_KEY))
            {
                _roundResult = PhotonUtils.GetRoomProperty<int[]>(ROUND_RESULT_KEY);
            }
            // update UI
            OnStateChanged();
        }

        private void Update()
        {
            CheckTimeout();
        }

        public void OnHostInit()
        {
            // only called by master client
            // update user property
            PlayerManager.SetLocalPlayerAligned(true);
            PlayerManager.SetLocalPlayerReady(true);
            // update room property
            if (markerManager.selectedMarker != null)
            {
                MarkerUtils.OverwriteMarkerPose(markerManager.selectedMarker);
                marker = markerManager.selectedMarker.data;
            }
        }

        public void OnAligned()
        {
            // update user property
            PlayerManager.SetLocalPlayerAligned(true);
        }

        public void OnReady()
        {
            // update user property
            PlayerManager.SetLocalPlayerReady(true);
        }

        private void StartRound()
        {
            // only called by master client
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable{
            {STATE_KEY, MatchState.PLAYING},
            {TIMER_KEY, PhotonNetwork.Time + START_ANIMATION_LENGTH}
        });

            // reset scene
            sceneManager.CleanObjects();

            // reset all player to full health but not ready
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (PlayerManager.IsPlayer(player))
                {
                    PlayerManager.ResetPlayer(player);
                }
            }
        }

        private IEnumerator StartRoundAfterAnimation()
        {
            float delay = (float)(startTime - PhotonNetwork.Time);
            Logger.Log("[MatchManager] delay " + delay + " start " + startTime);
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            // enable players' ability
            PlayerManager.localPlayer.SetAbilitiesActive(true);
        }

        private void StopTimer()
        {
            // NOTE: only called by master client
            _startTime = -2;
            startTime = -2;
        }

        public double GetCurrentCountDown()
        {
            if (startTime.Equals(-1)) return MAX_TIME;
            if (startTime > -2) countDown = MAX_TIME - (PhotonNetwork.Time - startTime);
            return countDown < 0 ? 0 : (countDown > MAX_TIME ? MAX_TIME : countDown);
        }

        private void CheckTimeout()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (matchState == MatchState.PLAYING && GetCurrentCountDown() == 0 && startTime >= 0)
            {
                // timeout and draw
                EndRound(-1);
            }
        }

        private void TryEndRound()
        {
            // NOTE: only called by master client
            // fight until only one player remain
            int winner = -1;
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (!PlayerManager.IsPlayerLose(player))
                {
                    if (winner == -1) winner = PhotonUtils.GetPlayerProperty<int>(player, PlayerManager.ROLE_KEY);
                    else return;
                }
            }
            EndRound(winner);
        }

        private void EndRound(int winner)
        {
            // only called by master client
            // winner: -1: draw, >= 0: winner player id

            /* Compute round result*/
            // stop timer
            StopTimer();

            // update result
            int[] result = new int[roundResult.Length + 1];
            for (int i = 0; i < roundResult.Length; ++i)
            {
                result[i] = roundResult[i];
            }
            result[roundResult.Length] = winner;
            roundResult = result;

            /* Check if the game is over */

            // compute points
            int drawPoints = 0;
            int[] points = new int[NUM_PLAYERS];
            foreach (int id in result)
            {
                if (id == -1) drawPoints += POINTS_DRAW;
                else points[id] += POINTS_WIN;
            }

            int maxPoints = 0;
            bool isDraw = false;
            Logger.Log("[MatchManager] round over");
            for (int i = 0; i < NUM_PLAYERS; ++i)
            {
                points[i] += drawPoints;
                Logger.Log("[MatchManager] player " + i + ": " + points[i]);
                if (maxPoints < points[i])
                {
                    maxPoints = points[i];
                    isDraw = false;
                }
                else if (maxPoints == points[i]) isDraw = true;
            }

            if (maxPoints >= MAX_POINTS || result.Length == MAX_ROUND)
            {
                // game over
                Logger.Log("[MatchManager] game over");
                // update match result to stop game
                // 0: lose, 1: draw, 2: win
                int winResult = isDraw ? 1 : 2;
                int[] results = new int[NUM_PLAYERS];
                for (int i = 0; i < NUM_PLAYERS; ++i)
                {
                    if (points[i] == maxPoints) results[i] = winResult;
                }
                matchResult = results;

                // lock room
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }

            // set match state
            matchState = MatchState.WAITING;
        }

        private void OnStateChanged()
        {
            switch (matchState)
            {
                case MatchState.INITIALIZING:
                    // update scan UI and do scan
                    ui.ShowInitUI();
                    break;

                case MatchState.READY:
                    // show align UI
                    ui.ShowAlignUI();
                    break;

                case MatchState.PLAYING:
                    // hide controller model 
                    playerManager.ShowAllControllerModels(false);
                    // play start animation
                    ui.RoundStart(startTime);
                    // hide controllers
                    ui.ShowControllers(false);
                    // enable player ability
                    StartCoroutine(StartRoundAfterAnimation());
                    break;

                case MatchState.WAITING:
                    // disable player ability
                    PlayerManager.localPlayer.SetAbilitiesActive(false);
                    break;

                case MatchState.ENDING:
                    // play ending animation
                    int id = PlayerManager.GetLocalPlayerRole();
                    if (id >= 0)
                    {
                        ui.ShowEnding(matchResult[id]);
                    }
                    break;

                case MatchState.ERROR:
                    // disable player ability
                    PlayerManager.localPlayer.SetAbilitiesActive(false);
                    // show error UI
                    ui.ShowControllers(true);
                    ui.ShowErrorUI();
                    break;
            }
        }

        public override void OnRoomPropertiesUpdate(Hashtable changedProps)
        {
            if (changedProps.ContainsKey(TIMER_KEY))
            {
                _startTime = PhotonUtils.GetRoomProperty<double>(TIMER_KEY);
            }
            if (changedProps.ContainsKey(MARKER_KEY))
            {
                _marker = MarkerUtils.DeserializeMarker(PhotonUtils.GetRoomProperty<Byte[]>(MARKER_KEY));
                markerManager.SetTargetMarker(_marker);
                Logger.Log("[MatchManager] Get target marker: " + MarkerUtils.MarkerToLog(marker));
            }

            // show UI depend on match state
            if (changedProps.ContainsKey(STATE_KEY))
            {
                _matchState = PhotonUtils.GetRoomProperty<MatchState>(STATE_KEY);

                // skip UI update for invalid players
                if (PlayerManager.GetLocalPlayerRole() < 0)
                {
                    // eject all spectators after ending animation
                    if (matchState == MatchState.ENDING || matchState == MatchState.ERROR) Invoke("ReturnToLobby", 5f);
                    return;
                }

                // update UI
                OnStateChanged();
            }

            if (changedProps.ContainsKey(ROUND_RESULT_KEY))
            {
                _roundResult = PhotonUtils.GetRoomProperty<int[]>(ROUND_RESULT_KEY);
                if (PlayerManager.GetLocalPlayerRole() >= 0)
                {
                    // play round end animation
                    int winner = roundResult[roundResult.Length - 1];
                    ui.RoundEnd(winner);
                }
            }

            if (!PhotonNetwork.IsMasterClient) return;
            if (changedProps.ContainsKey(MARKER_KEY) && matchState == MatchState.INITIALIZING)
            {
                // change state when host has selected the target marker
                CheckPlayers(out bool enoughPlayers, out bool allReady);
                if (allReady)
                {
                    StartRound();
                }
                else
                {
                    matchState = MatchState.READY;
                }
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            bool healthChanged = changedProps.ContainsKey(PlayerManager.HEALTH_KEY);
            bool readyChanged = changedProps.ContainsKey(PlayerManager.READY_KEY);
            if (targetPlayer == PhotonNetwork.LocalPlayer)
            {
                // update align UI
                if (readyChanged && matchState == MatchState.READY)
                {
                    ui.ShowAlignUI();
                }

                // damaged animation
                if (healthChanged && matchState == MatchState.PLAYING &&
                    PlayerManager.localPlayer.health < HealthManager.MAX_HEALTH)
                {
                    ui.OnDamaged();
                }
            }

            // update match state
            if (!PhotonNetwork.IsMasterClient) return;
            if (changedProps.ContainsKey(PlayerManager.ROLE_KEY)) CheckPlayerRoles();
            CheckPlayers(out bool enoughPlayers, out bool allReady);

            switch (matchState)
            {
                case MatchState.READY:
                    if (readyChanged && allReady)
                    {
                        // start first round
                        StartRound();
                    }
                    break;
                case MatchState.PLAYING:
                    if (healthChanged)
                    {
                        if (startTime >= 0 && startTime < PhotonNetwork.Time)
                        {
                            // check if the round is over
                            TryEndRound();
                        }
                    }
                    break;
                case MatchState.WAITING:
                    if (readyChanged && allReady)
                    {
                        // start next round or end game
                        if (matchResult.Length == 0) StartRound();
                        else matchState = MatchState.ENDING;
                    }
                    break;
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            // NOTE: players will be removed from the player list if they are leaving not just become inactive

            if (!PhotonNetwork.IsMasterClient) return;

            CheckPlayers(out bool enoughPlayers, out bool allReady);
            // NOTE: player may not be ready in WAITING state
            // so we have to check enoughPlayers instead of allReady
            if (enoughPlayers) return;

            // not enough player in the room
            switch (matchState)
            {
                case MatchState.PLAYING:
                case MatchState.WAITING:
                    // lock room so no one can join the game until all player leave
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    matchState = MatchState.ERROR;
                    break;
            }

        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (matchState == MatchState.INITIALIZING)
            {
                // update UI for new host
                ui.ShowInitUI();
            }
        }

        public void ReturnToLobby()
        {
            sceneTransition.FadeOut(() =>
                {
                    // leave room
                    PhotonNetwork.LeaveRoom();
                }
            );
        }

        public override void OnLeftRoom()
        {
            // go back to lobby scene
            PhotonNetwork.LoadLevel("Lobby");
        }
    }
}