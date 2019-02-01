using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Lidgren.Network;
using Network.Config;
using UnityEditor;
using UnityEngine;

namespace Network
{
    public class Server : MonoBehaviour
    {
        [SerializeField] private ServerConfig config = null;

        private readonly Peer<NetServer> server = new Peer<NetServer>();
        private readonly List<NetConnection> clients = new List<NetConnection>();

        private byte[] assets;


        private void Awake()
        {
            assets = File.ReadAllBytes($"{Application.streamingAssetsPath}/assets");
            
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
            RuntimePlatform platform;
            byte[] bundle;
            switch (msg.op)
            {
                case NetOp.SystemInfo:
                    platform = (RuntimePlatform) msg.msg.ReadByte();
                    bundle = Bundle(platform);
                    if (bundle != null)
                    {
                        msg.res.Write((byte)NetOp.AssetsStart);
                        msg.res.Write(bundle.Length);
                    }
                    break;
                case NetOp.AssetsStart:
                    platform = (RuntimePlatform) msg.msg.ReadByte();
                    bundle = Bundle(platform);
                    if (bundle != null) StartCoroutine(SendAssetBundle(bundle, msg.msg.SenderConnection));
                    break;
                case NetOp.Ready:
                    Debug.Log(msg.msg.SenderEndPoint + " is ready");
                    break;
            }
        }

        private byte[] Bundle(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return assets;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return assets;
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                    return assets;
                default:
                    Debug.LogErrorFormat("Platform not supported: {0}", platform);
                    return null;
            }
        }

        private IEnumerator SendAssetBundle(byte[] bundle, NetConnection connection)
        {
            if (bundle == null) yield break;
            if (connection == null) yield break;

            var data = new byte[connection.CurrentMTU - 100];

            for (var i = 0; i < bundle.Length; i += data.Length)
            {
                yield return new WaitForSeconds(connection.AverageRoundtripTime);

                var size = Mathf.Min(data.Length, bundle.Length - i);
                
                for (var j = 0; j < size; j++)
                {
                    data[j] = bundle[i + j];
                }

                var msg = server.NetPeer.CreateMessage();
                msg.Write((byte) NetOp.AssetsData);
                msg.Write(i);
                msg.Write(size);
                msg.Write(data);

                connection.SendMessage(msg, NetDeliveryMethod.ReliableUnordered, 0);
            }
        }
    }
}