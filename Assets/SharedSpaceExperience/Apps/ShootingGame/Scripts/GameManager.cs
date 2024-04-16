using System;
using UnityEngine;
using Unity.Netcode;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.Example
{
    public class GameManager : NetworkBehaviour
    {
        public enum GameState
        {
            WaitForReady,   // wait for all player ready
            WaitForStart,   // wait for host start the game
            Starting,       // start animation
            Playing,        // game play
            Ending,         // ending animation
            Error           // not enough player
        }

        public static GameManager Instance { get; private set; }

        [SerializeField]
        private PlayerManager playerManager;

        [SerializeField]
        private HUDUI hud;

        [SerializeField]
        private CountDown countDown;

        public NetworkVariable<GameState> gameState = new(
            GameState.WaitForReady,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        public NetworkVariable<long> winner = new(
            -1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public Action<GameState> OnGameStateChanged;

        [SerializeField]
        private float MIN_PLAYERS = 2;
        public float DELAY_BEFORE_START = 3 + 5; // start animation length (3s) + sync delay (2s)
        public float GAME_DURATION = 99;
        public static int MAX_HEALTH = 4;

        private void OnEnable()
        {
            // singleton
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(this);

            // register callbacks
            gameState.OnValueChanged += OnGameStateValueChanged;

            if (!NetworkController.Instance.isServer) return;
            playerManager.OnAllPlayersReadyChanged += OnAllPlayersReadyChanged;
            playerManager.OnPlayerCountChanged += OnPlayerCountChanged;
        }

        private void OnDisable()
        {
            // singleton
            if (Instance == this) Instance = null;

            // deregister callbacks
            gameState.OnValueChanged -= OnGameStateValueChanged;
            DeregisterCallbacks();
        }

        private void DeregisterCallbacks()
        {
            if (NetworkController.Instance == null || !NetworkController.Instance.isServer) return;
            if (playerManager != null)
            {
                playerManager.OnAllPlayersReadyChanged -= OnAllPlayersReadyChanged;
                playerManager.OnPlayerCountChanged -= OnPlayerCountChanged;
            }
            countDown.OnTimeout = null;
            playerManager.OnPlayerWins = null;
        }

        private void OnGameStateValueChanged(GameState previous, GameState current)
        {
            OnGameStateChanged?.Invoke(current);
        }

        private void OnAllPlayersReadyChanged(bool allReady)
        {
            // Game state transition: WaitForReady <-> WaitForStart

            if (!NetworkController.Instance.isServer) return;
            gameState.Value = allReady ?
                (playerManager.playerProperties.Count < MIN_PLAYERS ?
                    GameState.Error : GameState.WaitForStart) :
                GameState.WaitForReady;
        }

        /** Server side game flow functions **/
        public void StartGame()
        {
            // Game state transition: WaitForStart -> Starting

            if (!NetworkController.Instance.isServer) return;
            playerManager.OnAllPlayersReadyChanged -= OnAllPlayersReadyChanged;

            // reset player states
            playerManager.ResetAllPlayersHealth();
            // set player model style
            playerManager.SetPlayersStyles();

            // Note: player models visiblility is set in playerManager
            gameState.Value = GameState.Starting;

            // play start animation
            PlayStartVideoClientRpc(NetworkManager.Singleton.ServerTime.Time + DELAY_BEFORE_START);
            Invoke(nameof(StartGameAfterAnimation), DELAY_BEFORE_START);
        }

        private void StartGameAfterAnimation()
        {
            // Game state transition: Starting -> Playing

            if (!NetworkController.Instance.isServer) return;
            gameState.Value = GameState.Playing;

            Logger.Log("[Sync] Server start game at " + NetworkManager.Singleton.ServerTime.Time);

            // Note: player abilities are enabled in playerManager

            // start game
            countDown.OnTimeout = OnGameTimeout;
            countDown.StartCountDown(GAME_DURATION);

            // check for winners
            playerManager.OnPlayerWins = GameEnds;
            // playerManager.CheckDoesAnyPlayerWins();
        }

        private void OnGameTimeout()
        {
            GameEnds();
        }

        private void GameEnds(long winner = -1)
        {
            // Game state transition: Playing -> Ending

            if (!NetworkController.Instance.isServer) return;

            // deregister callbacks
            countDown.OnTimeout = null;
            playerManager.OnPlayerWins = null;

            // Note: player abilities are disabled in playerManager

            // set winner 
            this.winner.Value = winner;
            gameState.Value = GameState.Ending;

            // reset all player to not ready
            playerManager.ResetAllPlayersIsReady();

            // play ending video
            PlayEndingVideoClientRpc(winner);

            // wait for all player finish ending video
            playerManager.OnAllPlayersReadyChanged += OnAllPlayersReadyChanged;
        }

        private void OnPlayerCountChanged()
        {
            if (playerManager.playerProperties.Count < MIN_PLAYERS &&
                gameState.Value != GameState.WaitForReady &&
                gameState.Value != GameState.Error)
            {
                gameState.Value = GameState.Error;
                DeregisterCallbacks();
            }
        }

        /** Client side functions **/
        [ClientRpc]
        public void PlayStartVideoClientRpc(double expectEndTime)
        {
            Logger.Log("[Sync] Expected end time " + expectEndTime + " at " + NetworkManager.Singleton.ServerTime.Time);
            hud.PlayStartVideo(expectEndTime);
        }

        [ClientRpc]
        public void PlayEndingVideoClientRpc(long winner)
        {
            hud.PlayEndingVideo(winner);
        }

        public void OnEndingAnimationFinished()
        {
            // This function will be triggered by videoController.OnVideoFinished
            // which is registered in HUDUI

            // set local player ready
            if (playerManager.TryGetLocalPlayer(out PlayerProperty localPlayer))
            {
                localPlayer.SetPlayerIsReadyServerRpc(true);
            }
            else
            {
                Logger.LogError("Failed to get local player");
            }
        }
    }
}
