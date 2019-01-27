using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleOnOff : MonoBehaviour {

    public bool IsActive = false;
    public GameObject Torch;

	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.T))
        {
            IsActive = !IsActive;               
        }

        if (IsActive == true)
        {
            Torch.SetActive(true);    
        }                                      
        else                                 
        {
            Torch.SetActive(false);  
        }

    }
}
