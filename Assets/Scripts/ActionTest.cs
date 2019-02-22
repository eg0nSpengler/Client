using Brisk.Actions;
using Brisk.Entities;
using UnityEngine;

public class ActionTest : MonoBehaviour
{
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) GetComponent<NetPlayer>().Shoot();
    }    
}