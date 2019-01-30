using System;
using System.Collections;
using Lidgren.Network;
using Network.Config;
using UnityEngine;

namespace Network
{
    public class Client : MonoBehaviour
    {
        [SerializeField] private ServerConfig config = null;
        [SerializeField] private string host = "localhost";
        [SerializeField] private float connectTimeout = 10f;

        private readonly Peer<NetClient> client = new Peer<NetClient>();

        private bool isConnecting;
        
        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                Debug.LogError("No host server specified");
                return;
            }
            
            var success = client.Start(ref config, false);

            if (!success) return;
            client.Connected += ClientConnected;
            client.Disconnected +=ClientDisconnected;
            client.Data += ClientData;

            var resolved = client.Connect(host, config.Port);
            if (!resolved)
                Debug.Log($"Could not resolve host: {host}:{config.Port}");
            else
                StartCoroutine(CheckNoConnection());
        }

        private IEnumerator CheckNoConnection()
        {
            isConnecting = true;
            yield return new WaitForSeconds(connectTimeout);

            if (isConnecting)
                Debug.Log("Could not connect to server");
        }

        private void ClientDisconnected(NetConnection connection)
        {
            Debug.Log("Disconnecting");
        }

        private void ClientConnected(NetConnection connection)
        {
            isConnecting = false;
            Debug.Log("Connected to server: " + connection.RemoteEndPoint);
            
            var msg = client.NetPeer.CreateMessage();
            msg.Write("test");
            client.NetPeer.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        private void OnDestroy()
        {
            client.Stop("Disconnecting");
        }

        private void Update()
        {
            client.Data -= ClientData;
            client.Update();
        }

        private void ClientData(NetIncomingMessage msg)
        {
            Debug.Log(msg);
        }
    }
}
