using System.Collections;
using System.Collections.Generic;
using Brisk;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour
{
    [SerializeField] private Client client = null;
    [SerializeField] private ToggleInventory inventoryUI = null;
    [SerializeField] private Image loadingImage = null;
    [SerializeField] private bool destroyObjectWhenReady = false;
    
    private void Update()
    {
        var progress = client.LoadingProgress;
        loadingImage.fillAmount = progress;
        
        if (!(progress >= 1f)) return;
        inventoryUI.EnableNonToggleable();
        if(destroyObjectWhenReady)
            Destroy(gameObject);
        else
            Destroy(this);
    }
}
