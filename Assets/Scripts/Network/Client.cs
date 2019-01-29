using Lidgren.Network;
using Network.Config;
using UnityEngine;

namespace Network
{
    public class Client : MonoBehaviour
    {
        [SerializeField] private ServerConfig config = null;
        [SerializeField] private string host = "localhost";

        private NetClient client;
        
        
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

            if (string.IsNullOrWhiteSpace(host))
            {
                Debug.LogError("No host server specified");
                return;
            }
            
            // Prepare a config
            var netConfig = new NetPeerConfiguration(config.AppName);

            // Connect to the server
            client = new NetClient(netConfig);
            client.Start();
            client.Connect(host, config.Port);
            
            Debug.Log($"Connected to server");
        }

        private void OnDestroy()
        {
            Debug.Log("Disconnecting.");
            client.Shutdown("Disconnecting.");
        }
    }
}
