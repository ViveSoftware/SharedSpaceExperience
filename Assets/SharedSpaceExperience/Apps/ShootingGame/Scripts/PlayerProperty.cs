using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.Example
{
    public class PlayerProperty : NetworkBehaviour
    {
        public NetworkVariable<bool> isSpectator = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<bool> isReady = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> style = new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> health = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public bool isVisible = false;
        public bool isAbilityActive = false;

        [SerializeField]
        private List<ModelStyle> playerModelStyles;


        [SerializeField]
        private HealthManager healthManager;
        [SerializeField]
        private Shooter shooter;
        [SerializeField]
        private Shield shield;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                isReady.OnValueChanged += OnIsReadyChanged;
            }
            health.OnValueChanged += OnHealthChanged;

            // add to player list
            PlayerManager.Instance.playerProperties.Add(OwnerClientId, this);
            PlayerManager.Instance.OnPlayerCountChanged?.Invoke();

            // update model
            style.OnValueChanged += OnStyleChanged;
            UpdatePlayerModelStyle();
            UpdateVisibility();

            if (IsOwner)
            {
                // set is ready
                SetPlayerIsReadyServerRpc(true);
                // update ability
                UpdateAbilityActive();
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                isReady.OnValueChanged -= OnIsReadyChanged;
            }
            health.OnValueChanged -= OnHealthChanged;
            style.OnValueChanged -= OnStyleChanged;

            // remove from player list
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.playerProperties.Remove(OwnerClientId);
                PlayerManager.Instance.OnPlayerCountChanged?.Invoke();
            }
        }

        /* Ready */
        [ServerRpc]
        public void SetPlayerIsReadyServerRpc(bool ready)
        {
            isReady.Value = ready;
        }

        private void OnIsReadyChanged(bool previous, bool current)
        {
            PlayerManager.Instance.CheckAreAllPlayersReady();
        }

        /* Health */
        public void ResetHealth()
        {
            if (IsServer) healthManager.ResetHealth();
        }

        private void OnHealthChanged(int previous, int current)
        {
            if (IsServer)
            {
                UpdateVisibility();
                UpdateAbilityActive();

                if (current == 0)
                {
                    PlayerManager.Instance.CheckDoesAnyPlayerWins();
                }
            }

            // on damaged
            if (IsOwner)
            {
                if (previous > current)
                {
                    PlayerManager.Instance.OnLocalPlayerDamaged?.Invoke();
                }
            }
        }

        /* Style */
        public void OnStyleChanged(int previous, int current)
        {
            UpdatePlayerModelStyle();
        }

        public void UpdatePlayerModelStyle()
        {
            Logger.Log("style change " + style.Value);
            foreach (ModelStyle model in playerModelStyles)
            {
                model.SetStyle(style.Value);
            }
        }

        /* Visiblity */
        public void UpdateVisibility()
        {
            // combine with condition in this level
            isVisible = PlayerManager.Instance.isVisible && !isSpectator.Value;

            if (isSpectator.Value) return;

            // Note: other players will updated by syncing
            if (IsOwner) shooter.UpdateVisibility();
            if (IsServer)
            {
                healthManager.UpdateVisibility();
                shield.UpdateVisibility();
            }
        }

        /* Ability */
        public void UpdateAbilityActive()
        {
            // combine with condition in this level
            isAbilityActive = PlayerManager.Instance.isAbilityActive &&
                health.Value > 0 && !isSpectator.Value && isVisible;

            if (isSpectator.Value) return;

            // Note: other players will updated by syncing
            if (IsOwner) shooter.UpdateAbilityActive();
            if (IsServer)
            {
                healthManager.UpdateAbilityActive();
                shield.UpdateAbilityActive();
            }
        }

    }
}
