using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using Network.Config;
using Network.Entities;
using UnityEngine;

namespace Network
{
    public class Client : MonoBehaviour
    {
        [SerializeField] private ServerConfig config = null;
        [SerializeField] private string host = "localhost";
        [SerializeField] private float connectTimeout = 10f;
        [SerializeField] private float sendRate = 10;

        private readonly Peer<NetClient> client = new Peer<NetClient>();
        private readonly List<NetEntity> entities = new List<NetEntity>();

        private bool isConnecting;
        private bool connected;
        
        private void Awake()
        {
            client.Connected += ClientConnected;
            client.Disconnected += ClientDisconnected;
            client.Data += ClientData;
            
            if (string.IsNullOrWhiteSpace(host))
            {
                Debug.LogError("No host server specified");
                return;
            }
            
            var success = client.Start(ref config, false);

            if (!success) return;

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
            connected = false;
            Debug.Log("Disconnecting");
        }

        private void ClientConnected(NetConnection connection)
        {
            isConnecting = false;
            connected = true;
            Debug.Log("Connected to server: " + connection.RemoteEndPoint);

            StartCoroutine(SendData());
        }

        private void OnDestroy()
        {
            client.Data -= ClientData;
            client.Stop("Disconnecting");
        }

        private IEnumerator SendData()
        {
            var wait = new WaitForSeconds(1/sendRate);
            while (true)
            {
                yield return wait;

                if (!connected) continue;

                var msg = client.NetPeer.CreateMessage();
                msg.Write(entities.Count(entity => entity.Local));
            
                foreach (var entity in entities)
                    entity.Send(msg);

                client.NetPeer.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
            }
        }

        private void Update()
        {
            client.Receive();
        }

        private void ClientData(NetIncomingMessage msg)
        {
            var id = msg.ReadInt32();
            var pos = new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());

            var entity = entities.SingleOrDefault(e => e.Id == id);
            if (entity != null)
                entity.transform.position = pos;
            else
                Debug.LogWarning("Not found: "+id);
        }

        public void Register(NetEntity entity)
        {
            entities.Add(entity);
        }

        public void Deregister(NetEntity entity)
        {
            entities.Remove(entity);
        }
    }
}
