using Brisk.Actions;
using Brisk.Entities;
using UnityEngine;

public class ActionTest : MonoBehaviour
{
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) GetComponent<NetPlayer>().Net_Shoot();
    }    
}