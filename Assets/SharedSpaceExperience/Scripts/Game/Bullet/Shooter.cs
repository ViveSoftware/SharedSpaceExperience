using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Wave.Essence;

using Photon.Pun;

namespace SharedSpaceExperience
{

    public enum BulletType
    {
        NORMAL_BULLET,
        CHARGE_BULLET
    }

    public class Shooter : MonoBehaviourPun
    {
        [SerializeField]
        private XR_Device controller;

        public BulletType bulletType = BulletType.NORMAL_BULLET;

        [SerializeField]
        private GameObject normalBulletPrefab;
        [SerializeField]
        private GameObject chargeBulletPrefab;
        [SerializeField]
        private BulletPreview normalBulletPreview;
        [SerializeField]
        private BulletPreview chargeBulletPreview;
        [SerializeField]
        private Transform shootingPoint;

        [Header("Shoot Action")]
        public InputAction shootAction;
        [Header("Swap Bullet Action")]
        public InputAction swapBulletAction;

        private const float NORMAL_BULLET_CD = 0.5f;
        private const float CHARGE_BULLET_CD = 0.3f;
        private const float NORMAL_BULLET_SPEED = 5;
        private const float MAX_CHARGE_TIME = 1f;
        private const float MAX_CHARGE_SIZE = 3f;
        private const float MAX_DISTANCE = 1000;
        private const float BULLET_LIFE_TIME = 10;
        private const float VIBRATE_PERIOD = 0.1f;

        private double nextShootTime = 0; // shooting CD
        private bool isCharging = false;
        private double startChargingTime = 0;
        private float chargeBulletSize = 1;

        private void OnEnable()
        {
            if (photonView.IsMine)
            {
                shootAction.Enable();
                swapBulletAction.Enable();
                shootAction.started += OnShootActionStart;
                shootAction.canceled += OnShootActionStop;
                swapBulletAction.started += OnSwapBullet;

                nextShootTime = 0;
                normalBulletPreview.SetPreviewActive(bulletType == BulletType.NORMAL_BULLET);
                chargeBulletPreview.SetPreviewActive(bulletType == BulletType.CHARGE_BULLET);
            }
        }

        private void OnDisable()
        {
            if (photonView.IsMine)
            {
                StopShooting();
                shootAction.started -= OnShootActionStart;
                shootAction.canceled -= OnShootActionStop;
                swapBulletAction.started -= OnSwapBullet;
                shootAction.Disable();
                swapBulletAction.Disable();

                normalBulletPreview.SetPreviewActive(false);
                chargeBulletPreview.SetPreviewActive(false);
            }
        }

        private void Update()
        {
            // update preview
            if (bulletType == BulletType.NORMAL_BULLET)
            {
                normalBulletPreview.SetPreviewActive(nextShootTime < PhotonNetwork.Time);
            }
            else
            {
                if (isCharging && chargeBulletSize < MAX_CHARGE_SIZE)
                {
                    chargeBulletSize = Mathf.Lerp(1, MAX_CHARGE_SIZE, (float)(PhotonNetwork.Time - startChargingTime) / MAX_CHARGE_TIME);
                }
                chargeBulletPreview.SetPreviewActive(nextShootTime < PhotonNetwork.Time);
                chargeBulletPreview.SetPreviewSize(chargeBulletSize);
            }
        }


        void OnShootActionStart(InputAction.CallbackContext context)
        {
            // compute start time
            double startDelay = Math.Max(nextShootTime - PhotonNetwork.Time, 0);
            Logger.Log("[Shooter] Shoot pressed " + startDelay + " " + (float)startDelay);

            if (bulletType == BulletType.NORMAL_BULLET)
            {
                InvokeRepeating("NormalWeaponShooting", (float)startDelay, NORMAL_BULLET_CD);
            }
            else
            {
                if (startDelay.Equals(0))
                {
                    BulletCharging();
                }
                else
                {
                    Invoke("BulletCharging", (float)startDelay);
                }
                InvokeRepeating("ChargingHaptics", (float)startDelay, VIBRATE_PERIOD);
            }
        }

        void OnShootActionStop(InputAction.CallbackContext context)
        {
            if (isCharging && bulletType == BulletType.CHARGE_BULLET)
            {
                ChargeWeaponShooting();
            }
            StopShooting();
        }

        public void StopShooting()
        {
            Logger.Log("[Shooter] Stop shooting");

            if (bulletType == BulletType.NORMAL_BULLET)
            {
                CancelInvoke("NormalWeaponShooting");
            }
            else if (bulletType == BulletType.CHARGE_BULLET)
            {
                CancelInvoke("BulletCharging");
                CancelInvoke("ChargingHaptics");
                isCharging = false;
                chargeBulletSize = 1;
            }
        }

        void OnSwapBullet(InputAction.CallbackContext context)
        {
            nextShootTime = 0;

            Logger.Log("[Shooter] Swap bullet");

            // cancel shooting
            StopShooting();

            // cancel shooting
            if (bulletType == BulletType.NORMAL_BULLET)
            {
                bulletType = BulletType.CHARGE_BULLET;
            }
            else
            {
                bulletType = BulletType.NORMAL_BULLET;
            }

            normalBulletPreview.SetPreviewActive(bulletType == BulletType.NORMAL_BULLET);
            chargeBulletPreview.SetPreviewActive(bulletType == BulletType.CHARGE_BULLET);

        }

        /* Normal Bullet */
        private void NormalWeaponShooting()
        {
            double curTime = PhotonNetwork.Time;
            nextShootTime = curTime + NORMAL_BULLET_CD;

            // haptics
            WXRDevice.SendHapticImpulse(controller, 1, 0.2f);

            Logger.Log("[Shooter] normal bullet " + PlayerManager.GetLocalPlayerRole());
            photonView.RPC(
                "ShootBulletRPC",
                RpcTarget.MasterClient,
                PlayerManager.GetLocalPlayerRole(),
                BulletType.NORMAL_BULLET,
                1f,
                shootingPoint.position,
                shootingPoint.forward
            );
        }

        /* Charge Bullet */
        private void BulletCharging()
        {
            isCharging = true;
            startChargingTime = PhotonNetwork.Time;
        }

        private void ChargingHaptics()
        {
            WXRDevice.SendHapticImpulse(controller, Mathf.InverseLerp(1, MAX_CHARGE_SIZE, chargeBulletSize), VIBRATE_PERIOD);
        }

        private void ChargeWeaponShooting()
        {
            double curTime = PhotonNetwork.Time;
            nextShootTime = curTime + CHARGE_BULLET_CD;

            // haptics
            CancelInvoke("ChargingHaptics");
            WXRDevice.SendHapticImpulse(controller, 1, 0.2f);

            Logger.Log("[Shooter] charge bullet " + PlayerManager.GetLocalPlayerRole());
            photonView.RPC(
                "ShootBulletRPC",
                RpcTarget.MasterClient,
                PlayerManager.GetLocalPlayerRole(),
                BulletType.CHARGE_BULLET,
                chargeBulletSize,
                shootingPoint.position,
                shootingPoint.forward
            );
        }

        [PunRPC]
        private void ShootBulletRPC(int playerID, BulletType type, float chargeTime, Vector3 startPoint, Vector3 direction)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // create bullet
            object[] bulletParameters = new object[] { playerID, chargeTime }; // ownerID, size
            GameObject bulletObject = PhotonNetwork.InstantiateRoomObject(
                type == BulletType.NORMAL_BULLET ? normalBulletPrefab.name : chargeBulletPrefab.name,
                startPoint, Quaternion.identity, 0, bulletParameters
            );

            Bullet bullet = bulletObject.GetComponent<Bullet>();
            bullet.SetTimeout(BULLET_LIFE_TIME);
            bulletObject.GetComponent<Rigidbody>().velocity = direction.normalized * NORMAL_BULLET_SPEED * chargeTime;
        }
    }
}