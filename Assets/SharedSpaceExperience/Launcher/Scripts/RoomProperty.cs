using Unity.Netcode;
using System;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience
{
    public class RoomProperty : NetworkBehaviour
    {
        // update when host finished align
        public NetworkVariable<AlignManager.AlignMethod> alignMethod = new(AlignManager.AlignMethod.NotAligned, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public static RoomProperty Instance { get; private set; }

        public Action OnRealign;

        private void OnEnable()
        {
            // singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this) Destroy(gameObject);

            alignMethod.OnValueChanged += OnAlignMethodChanged;
        }

        private void OnDisable()
        {
            alignMethod.OnValueChanged -= OnAlignMethodChanged;
        }

        [ClientRpc]
        public void RealignClientRPC()
        {
            OnRealign?.Invoke();
        }

        private void OnAlignMethodChanged(AlignManager.AlignMethod previous, AlignManager.AlignMethod current)
        {
            Logger.Log("align method: " + alignMethod.Value);
        }
    }
}