using UnityEngine;
using Unity.Netcode.Components;

// This script allows the owner to change the network transform.
// https://docs-multiplayer.unity3d.com/netcode/current/components/networktransform/#owner-authoritative-mode

namespace SharedSpaceExperience
{
    [DefaultExecutionOrder(100000)]
    public class ClientNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}