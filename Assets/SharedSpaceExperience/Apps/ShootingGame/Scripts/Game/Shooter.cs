using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Wave.Essence;
using Unity.Netcode;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.Example
{
    public enum BulletType
    {
        Normal,
        Charged
    }

    public class Shooter : NetworkBehaviour
    {
        [SerializeField]
        private PlayerProperty player;

        [SerializeField]
        private XR_Device controller;
        [SerializeField]
        private Transform shootingPoint;

        [Header("Bullet")]
        public BulletType bulletType = BulletType.Normal;
        [SerializeField]
        private GameObject normalBulletPrefab;
        [SerializeField]
        private GameObject chargeBulletPrefab;
        [SerializeField]
        private BulletPreview normalBulletPreview;
        [SerializeField]
        private BulletPreview chargeBulletPreview;


        [Header("Shoot Action")]
        public InputAction shootAction;
        [Header("Swap Bullet Action")]
        public InputAction swapBulletAction;

        private const float BULLET_SPEED = 5;
        private const float BULLET_LIFE_TIME = 3;
        private const float NORMAL_BULLET_CD = 0.5f;
        private const float CHARGE_BULLET_CD = 0.3f;
        private const float MAX_CHARGE_TIME = 1f;
        private const float MAX_CHARGE_SIZE = 3f;
        private const float HAPTIC_SHOOT_AMPLITUDE = 1;
        private const float HAPTIC_SHOOT_DURATION = 0.2f;
        private const float HAPTIC_CHARGE_DURATION = 0.1f;

        [SerializeField]
        private bool isActive = false;
        private bool isVisible = false;
        private bool isCharging = false;
        private double nextShootTime = 0; // shooting CD
        private double startChargingTime = 0;
        private float chargeBulletSize = 1;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            shootAction.started += OnShootActionStart;
            shootAction.canceled += OnShootActionStop;
            swapBulletAction.started += OnSwapBullet;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;
            StopShooting();
            shootAction.started -= OnShootActionStart;
            shootAction.canceled -= OnShootActionStop;
            swapBulletAction.started -= OnSwapBullet;
            shootAction.Disable();
            swapBulletAction.Disable();
        }

        /* Visiblity */
        public void UpdateVisibility()
        {
            isVisible = player.isVisible && nextShootTime < NetworkManager.ServerTime.Time;
            Logger.Log("[V] " + player.OwnerClientId + " " + player.isVisible + " " + (nextShootTime < NetworkManager.ServerTime.Time));
            // combine with condition in this level
            normalBulletPreview.UpdateVisibility(isVisible && bulletType == BulletType.Normal);
            chargeBulletPreview.UpdateVisibility(isVisible && bulletType == BulletType.Charged);
        }

        /* Ability */
        public void UpdateAbilityActive()
        {
            if (!IsOwner) return;

            isActive = player.isAbilityActive;
            if (isActive)
            {
                nextShootTime = 0;
                shootAction.Enable();
                swapBulletAction.Enable();
            }
            else
            {
                shootAction.Disable();
                swapBulletAction.Disable();
            }
        }

        private void Update()
        {
            if (!IsOwner || !isActive) return;

            // update preview
            UpdateVisibility();

            if (bulletType == BulletType.Charged)
            {
                // charging
                if (isCharging && chargeBulletSize < MAX_CHARGE_SIZE)
                {
                    float rawSize =
                        (float)(NetworkManager.ServerTime.Time - startChargingTime)
                            / MAX_CHARGE_TIME;
                    chargeBulletSize = Mathf.Lerp(1, MAX_CHARGE_SIZE, rawSize);
                }
                chargeBulletPreview.SetSize(chargeBulletSize);
            }
        }

        void OnShootActionStart(InputAction.CallbackContext context)
        {
            // compute start time
            double startDelay = Math.Max(nextShootTime - NetworkManager.ServerTime.Time, 0);
            Logger.Log("Shoot pressed " + startDelay);

            if (bulletType == BulletType.Normal)
            {
                InvokeRepeating(nameof(NormalWeaponShooting), (float)startDelay, NORMAL_BULLET_CD);
            }
            else
            {
                if (startDelay == 0) BulletCharging();
                else Invoke(nameof(BulletCharging), (float)startDelay);
                InvokeRepeating(nameof(ChargingHaptics), (float)startDelay, HAPTIC_CHARGE_DURATION);
            }
        }

        void OnShootActionStop(InputAction.CallbackContext context)
        {
            if (isCharging && bulletType == BulletType.Charged)
            {
                // charge bullet shoot when release
                ChargeWeaponShooting();
            }
            StopShooting();
        }

        public void StopShooting()
        {
            Logger.Log("Stop shooting");

            if (bulletType == BulletType.Normal)
            {
                CancelInvoke(nameof(NormalWeaponShooting));
            }
            else
            {
                CancelInvoke(nameof(BulletCharging));
                CancelInvoke(nameof(ChargingHaptics));
                isCharging = false;
                chargeBulletSize = 1;
            }
        }

        void OnSwapBullet(InputAction.CallbackContext context)
        {
            Logger.Log("Swap bullet");

            nextShootTime = 0;

            // cancel shooting
            StopShooting();

            // change bullet type
            bulletType = bulletType == BulletType.Normal ?
                BulletType.Charged : BulletType.Normal;

            // update preview
            UpdateVisibility();

        }

        /* Normal Bullet */
        private void NormalWeaponShooting()
        {
            // compute next shooting time
            nextShootTime = NetworkManager.ServerTime.Time + NORMAL_BULLET_CD;

            // haptics
            WXRDevice.SendHapticImpulse(
                controller,
                HAPTIC_SHOOT_AMPLITUDE,
                HAPTIC_SHOOT_DURATION
            );

            // shoot bullet
            Logger.Log("shoot normal bullet: " + OwnerClientId);
            ShootBulletServerRpc(
                OwnerClientId,
                BulletType.Normal,
                shootingPoint.position,
                shootingPoint.forward
            );
        }

        /* Charge Bullet */
        private void BulletCharging()
        {
            isCharging = true;
            startChargingTime = NetworkManager.ServerTime.Time;
        }

        private void ChargingHaptics()
        {
            WXRDevice.SendHapticImpulse(
                controller,
                Mathf.InverseLerp(1, MAX_CHARGE_SIZE, chargeBulletSize),
                HAPTIC_CHARGE_DURATION
            );
        }

        private void ChargeWeaponShooting()
        {
            // compute next shooting time
            nextShootTime = NetworkManager.ServerTime.Time + CHARGE_BULLET_CD;

            // haptics
            CancelInvoke(nameof(ChargingHaptics));
            WXRDevice.SendHapticImpulse(
                controller,
                HAPTIC_SHOOT_AMPLITUDE,
                HAPTIC_SHOOT_DURATION
            );

            // shoot bullet
            Logger.Log("shoot charge bullet: " + OwnerClientId);
            ShootBulletServerRpc(
                OwnerClientId,
                BulletType.Charged,
                shootingPoint.position,
                shootingPoint.forward,
                chargeBulletSize
            );
        }

        [ServerRpc]
        private void ShootBulletServerRpc(ulong uid, BulletType type, Vector3 startPoint, Vector3 direction, float chargeTime = 1)
        {
            if (!IsServer) return;
            Logger.Log("shoot bullet: " + uid);

            // create bullet
            GameObject bulletObj = AppManager.Instance.SpawnNetworkObject(
                uid,
                type == BulletType.Normal ? normalBulletPrefab : chargeBulletPrefab,
                startPoint,
                Quaternion.identity
            );

            // set bullet
            bulletObj.GetComponent<Bullet>().Init(
                BULLET_LIFE_TIME,
                chargeTime,
                BULLET_SPEED * chargeTime * direction.normalized
            );

        }

    }
}
