using Brisk.Entities;
using UnityEngine;

public class SimpleMove : MonoBehaviour
{
    private void Start()
    {
        if(!GetComponent<NetEntity>().Owner) Destroy(this);
    }

    private void Update()
    {
        transform.Translate(
            Input.GetAxisRaw("Horizontal")*Time.deltaTime*4,
            0,
            Input.GetAxisRaw("Vertical")*Time.deltaTime*4);
    }
}