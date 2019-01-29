using System.Net.Sockets;
using Lidgren.Network;
using Network.Config;
using UnityEngine;

namespace Network
{
    public class Server : MonoBehaviour
    {
        [SerializeField] private ServerConfig config = null;

        private NetServer server;
        
        
        private void Awake()
        {
            // Find some ServerConfig in Resources if it's not already assigned
            if (config == null)
            {
                var allSettings = Resources.FindObjectsOfTypeAll<ServerConfig>();
                if (allSettings.Length == 0)
                {
                    Debug.LogError(
                        Application.isEditor
                            ? @"No Server Config found. Please create one using the ""Assets/Create/Network/Server Config"" menu item."
                            : @"No Server Config found.");
                    return;
                }

                if (allSettings.Length > 1)
                {
                    Debug.LogError(
                        "More than one Server Config found in Resources. Please delete on or assign one to this Server component");
                    return;
                }

                config = allSettings[0];
            }
            
            // Prepare a config
            var netConfig = new NetPeerConfiguration(config.AppName)
            {
                Port = config.Port,
            };

            // Start the server
            server = new NetServer(netConfig);
            try
            {
                server.Start();
            }
            catch (SocketException e)
            {
                Debug.LogError(e.Message);
                return;
            }
            
            Debug.Log($"Server running on port {netConfig.Port}");
        }

        private void OnDestroy()
        {
            Debug.Log("Server shutting down.");
            server.Shutdown("Server shutting down.");
        }
    }
}
