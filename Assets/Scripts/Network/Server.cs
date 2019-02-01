using System;
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

        private AssetBundle assets;


        private void Awake()
        {
            assets = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/assets");
            
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

            var msg = server.NetPeer.CreateMessage();
            msg.Write((byte) NetOp.SystemInfo);
            connection.SendMessage(msg, NetDeliveryMethod.ReliableUnordered, 0);
            Debug.Log(connection.RemoteEndPoint + " connected");
        }

        private void ServerOnDisconnected(NetConnection connection)
        {
            clients.Remove(connection);
            Debug.Log(connection.RemoteEndPoint + " disconnected");
        }

        private void ServerOnData(ref NetMessage msg)
        {
            switch (msg.op)
            {
                case NetOp.SystemInfo:
                    Debug.Log("Received platform info: "+ (RuntimePlatform)msg.msg.ReadByte());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}