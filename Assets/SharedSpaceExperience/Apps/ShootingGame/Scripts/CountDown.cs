using System.Collections;
using System;
using UnityEngine;
using Unity.Netcode;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.Example
{
    public class CountDown : NetworkBehaviour
    {
        private double timeout;

        public Action OnStartCountDown;
        public Action OnTimeout;

        private Coroutine countDownCoroutine = null;

        public void StartCountDown(float timeToCount)
        {
            StartCountDown((double)timeToCount);
        }

        public void StartCountDown(double timeToCount)
        {
            if (!NetworkManager.Singleton.IsServer) return;
            double timeout = NetworkManager.Singleton.ServerTime.Time + timeToCount + 1;
            CountDownClientRpc(timeout);
        }

        [ClientRpc]
        private void CountDownClientRpc(double timeout)
        {
            // stop previous countdown
            if (countDownCoroutine != null)
            {
                StopCoroutine(countDownCoroutine);
            }

            // set timeout 
            this.timeout = timeout;
            Logger.Log("Set timeout: " + timeout);
            OnStartCountDown?.Invoke();
            countDownCoroutine = StartCoroutine(WaitForTimeout(GetCounterValue()));
        }

        private IEnumerator WaitForTimeout(float timeToCount)
        {
            if (timeToCount > 0)
            {
                yield return new WaitForSeconds(timeToCount);
            }
            Logger.Log("Timeout: " + GetCounterValue());
            OnTimeout?.Invoke();

            countDownCoroutine = null;
        }

        public float GetCounterValue()
        {
            return (float)(timeout - NetworkManager.Singleton.ServerTime.Time);
        }

        public float GetCounterValue(float min, float max)
        {
            return Mathf.Clamp(GetCounterValue(), min, max);
        }
    }
}