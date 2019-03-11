using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleOnOff : MonoBehaviour {

    [SerializeField] private bool isActive = false;
    [SerializeField] private GameObject torch = null;

    private void Update ()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            isActive = !isActive;               
        }

        torch.SetActive(isActive);
    }
}
