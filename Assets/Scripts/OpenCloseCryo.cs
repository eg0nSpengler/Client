using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCloseCryo : MonoBehaviour
{

    Animator m_Animator;
    int animLayer = 0;

    public GameObject CryoTube;

    void Start()
    {
        m_Animator = CryoTube.GetComponent<Animator>();

    }

    void OnMouseDown()
    {

        if (m_Animator.GetCurrentAnimatorStateInfo(animLayer).IsName("CryoClose"))
        {
            m_Animator.Play("CryoOpen");
        
        }

        else
        {

            m_Animator.Play("CryoClose");

        }


    }
}