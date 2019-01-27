using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetactiveOnClick : MonoBehaviour {

    public bool IsActive = true;
    public GameObject ParentOfPanels;

	public void parentOfAllOfThePanels()
    {
        IsActive = !IsActive;

        if (IsActive == true)
        {
            ParentOfPanels.SetActive(true);    //if the boolean is true
        }                                       //You can add her also more things like stopping the time With (Time.timeScale = 0;)
        else                                  //and turn it back with(Time.timeScale = 1;)
        {
            ParentOfPanels.SetActive(false);  //if the boolean is false
        }

    }
}
