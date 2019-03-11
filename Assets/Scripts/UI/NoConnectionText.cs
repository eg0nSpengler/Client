using System.Collections;
using System.Collections.Generic;
using Brisk;
using UnityEngine;
using UnityEngine.UI;

public class NoConnectionText : MonoBehaviour
{
    [SerializeField] private Client client = null;
    [SerializeField] private Text text = null;
    
    private void Start()
    {
        if (client == null || text == null) return;
        
        text.enabled = false;
        client.ConnectionFailed += ClientOnConnectionFailed;
    }

    private void ClientOnConnectionFailed()
    {
        text.enabled = true;
    }
}
