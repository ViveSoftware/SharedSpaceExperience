using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.Example
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }

        [SerializeField]
        private GameManager gameManager;

        public Dictionary<ulong, PlayerProperty> playerProperties = new();

        public Action OnPlayerCountChanged;
        public Action<bool> OnAllPlayersReadyChanged;
        public Action OnLocalPlayerDamaged;
        public Action<long> OnPlayerWins;

        public bool isVisible = false;
        public bool isAbilityActive = false;

        private void OnEnable()
        {
            // singleton
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(this);

            OnPlayerCountChanged += CheckAreAllPlayersReady;
            OnPlayerCountChanged += CheckDoesAnyPlayerWins;
            gameManager.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            // singleton
            if (Instance == this) Instance = null;

            OnPlayerCountChanged -= CheckAreAllPlayersReady;
            OnPlayerCountChanged -= CheckDoesAnyPlayerWins;
            gameManager.OnGameStateChanged -= OnGameStateChanged;
        }

        /* Access Player */
        public PlayerProperty GetLocalPlayer()
        {
            return playerProperties.ContainsKey(NetworkManager.Singleton.LocalClientId) ?
                playerProperties[NetworkManager.Singleton.LocalClientId] : null;
        }

        public bool TryGetLocalPlayer(out PlayerProperty localPlayer)
        {
            if (NetworkManager.Singleton == null)
            {
                localPlayer = null;
                return false;
            }
            return playerProperties.TryGetValue(NetworkManager.Singleton.LocalClientId, out localPlayer);
        }

        public bool TryGetPlayer(ulong uid, out PlayerProperty player)
        {
            return playerProperties.TryGetValue(uid, out player);
        }

        /* Ready */
        public void ResetAllPlayersIsReady()
        {
            // only invoked by server
            if (!NetworkController.Instance.isServer) return;
            foreach (ulong uid in playerProperties.Keys)
            {
                playerProperties[uid].isReady.Value = false;
            }
        }

        public void CheckAreAllPlayersReady()
        {
            Logger.Log("CheckAreAllPlayersReady");
            bool isAllReady = true;
            foreach (ulong uid in playerProperties.Keys)
            {
                isAllReady &= playerProperties[uid].isReady.Value;
            }

            Logger.Log("CheckAreAllPlayersReady: " + isAllReady);
            OnAllPlayersReadyChanged?.Invoke(isAllReady);
        }

        /* Style */
        public void SetPlayersStyles()
        {
            // only invoked by server
            if (!NetworkController.Instance.isServer) return;
            try
            {
                int i = 0;
                foreach (ulong uid in playerProperties.Keys)
                {
                    if (playerProperties[uid] != null)
                    {
                        playerProperties[uid].style.Value = ModelStyleList.Instance.GetStyleIndex(i);
                        ++i;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to set player style: " + e);
            }
        }

        /* Health */
        public void ResetAllPlayersHealth()
        {
            // only invoked by server
            if (!NetworkController.Instance.isServer) return;
            foreach (ulong uid in playerProperties.Keys)
            {
                playerProperties[uid].ResetHealth();
            }
        }

        public void CheckDoesAnyPlayerWins()
        {
            long winner = -1;
            foreach (ulong uid in playerProperties.Keys)
            {
                if (
                    !playerProperties[uid].isSpectator.Value &&
                    playerProperties[uid].health.Value > 0
                )
                {
                    // more than 2 player still alive
                    if (winner != -1) return;
                    // find survivor
                    winner = (long)uid;
                }
            }

            Logger.Log("winner: " + winner);

            if (winner != -1) OnPlayerWins?.Invoke(winner);
        }

        private void OnGameStateChanged(GameManager.GameState gameState)
        {
            isVisible = gameState == GameManager.GameState.Starting ||
                        gameState == GameManager.GameState.Playing ||
                        gameState == GameManager.GameState.Ending;

            isAbilityActive = gameState == GameManager.GameState.Playing;

            foreach (ulong uid in playerProperties.Keys)
            {
                // Note: update visibility first since ability can not be active when invisible
                playerProperties[uid]?.UpdateVisibility();
                playerProperties[uid]?.UpdateAbilityActive();
            }
        }

    }
}
