using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCloseCryo : MonoBehaviour
{
    [SerializeField] private Animator cryoTubeAnimator = null;

    private void OnMouseDown()
    {
        cryoTubeAnimator.Play(cryoTubeAnimator.GetCurrentAnimatorStateInfo(0).IsName("CryoClose") ? "CryoOpen" : "CryoClose");
    }
}