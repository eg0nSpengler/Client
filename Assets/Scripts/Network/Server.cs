using System.Collections.Generic;
using System.Net;
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
        private readonly List<NetConnection> clients = new List<NetConnection>();


        private void Awake()
        {
            server.Connected += ServerOnConnected;
            server.Disconnected += ServerOnDisconnected;
            server.Data += ServerOnData;
            var success = server.Start(ref config, true);

            if (!success) return;
            Debug.Log($"Server running on port {config.Port}");
        }

        private void OnDestroy()
        {
            server.Connected -= ServerOnConnected;
            server.Disconnected -= ServerOnDisconnected;
            server.Data -= ServerOnData;
            Debug.Log("Server shutting down");
            server.Stop("Server shutting down");
        }

        private void FixedUpdate()
        {
            server.Receive();
        }

        private void ServerOnConnected(NetConnection connection)
        {
            clients.Add(connection);
            Debug.Log(connection.RemoteEndPoint + " connected");
        }

        private void ServerOnDisconnected(NetConnection connection)
        {
            clients.Remove(connection);
            Debug.Log(connection.RemoteEndPoint + " disconnected");
        }

        private void ServerOnData(NetIncomingMessage msg)
        {
            var e = msg.ReadInt32();
            for (var i = 0; i < e; i++)
            {
                foreach (var client in clients)
                {
                    if (Equals(client, msg.SenderConnection)) continue;

                    var fwd = server.NetPeer.CreateMessage();

                    fwd.Write(msg.ReadInt32());
                    fwd.Write(msg.ReadFloat());
                    fwd.Write(msg.ReadFloat());
                    fwd.Write(msg.ReadFloat());

                    server.NetPeer.SendMessage(fwd, client, NetDeliveryMethod.ReliableOrdered);
                }
            }
        }
    }
}