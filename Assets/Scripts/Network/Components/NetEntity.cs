using Lidgren.Network;
using UnityEngine;

namespace Network.Components
{
    public sealed class NetEntity : MonoBehaviour
    {
        [SerializeField] private Client client = null;
        [SerializeField] private UpdateType updateType = UpdateType.None;
        [SerializeField] private bool moving = true;
        [SerializeField] private bool local = false;
        [SerializeField] private int id = 0;

        public bool Local => local;
        public int Id => id;
        
        private NetBehaviour[] behaviours;
        
        
        private void Start()
        {
            if (client == null) client = FindObjectOfType<Client>();
            behaviours = GetComponents<NetBehaviour>();

            if(client == null)
                Debug.LogError("No Client found for NetEntity", this);
            else
                client.Register(this);
        }

        private void OnDestroy()
        {
            if(client != null)
                client.Deregister(this);
        }

        public void Serialize(NetOutgoingMessage msg)
        {
            msg.Write(id);
            msg.Write(transform.position.x);
            msg.Write(transform.position.y);
            msg.Write(transform.position.z);
        }
    }
}