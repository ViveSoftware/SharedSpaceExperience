using UnityEngine;
using System;
using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace SharedSpaceExperience
{
    [RequireComponent(typeof(NetworkManager))]
    public class HostDiscrovery : NetworkDiscovery<DiscoveryBroadcastData, DiscoveryResponseData>
    {
        public string serverName = "HostName";
        public static Action<string, string, ushort> OnServerFound; // OnServerFound(serverName, serverIP, serverPort)

        protected override bool ProcessBroadcast(IPEndPoint sender, DiscoveryBroadcastData broadCast, out DiscoveryResponseData response)
        {
            response = new DiscoveryResponseData()
            {
                ServerName = serverName,
                Port = ((UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport).ConnectionData.Port,
            };
            return true;
        }

        protected override void ResponseReceived(IPEndPoint sender, DiscoveryResponseData response)
        {
            OnServerFound.Invoke(response.ServerName, sender.Address.ToString(), response.Port);
        }
    }
}