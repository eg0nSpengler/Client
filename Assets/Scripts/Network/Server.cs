using System.Net.Sockets;
using Lidgren.Network;
using Network.Config;
using UnityEngine;

namespace Network
{
    public class Server : MonoBehaviour
    {
        [SerializeField] private ServerConfig config = null;

        private readonly Peer<NetServer> server = new Peer<NetServer>();


        private void Awake()
        {
            var success = server.Start(ref config, true);

            if (!success) return;
            server.Data += ServerData;
            Debug.Log($"Server running on port {config.Port}");
        }

        private void OnDestroy()
        {
            server.Data -= ServerData;
            Debug.Log("Server shutting down");
            server.Stop("Server shutting down");
        }

        private void FixedUpdate()
        {
            server.Update();
        }

        private void ServerData(NetIncomingMessage msg)
        {
            Debug.Log(msg);
        }
    }
}
