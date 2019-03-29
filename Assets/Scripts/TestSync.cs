using Brisk.Entities;
using Brisk.Serialization;
using UnityEngine;

public class TestSync : NetBehaviour
{
    private bool b;
    private bool a;

    [SyncReliable]
    public bool A
    {
        get => a;
        set
        {
            Debug.Log("A: "+a);
            a = value;
        }
    }

    [SyncReliable]
    public bool B
    {
        get => b;
        set
        {
            Debug.Log("B: "+b);
            b = value;
        }
    }
    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
            A = !A;
        if (Input.GetKeyDown(KeyCode.Keypad2))
            B = !B;
    }
}
