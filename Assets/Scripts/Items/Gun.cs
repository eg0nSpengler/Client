 using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Gun : MonoBehaviour
{
    [SerializeField] private float force = 200f;
    [SerializeField] private float recoil = 10f;
    [SerializeField] private float range = 100f;

    [SerializeField] private Rigidbody gun = null;
    [SerializeField] private Transform gunBarrel = null;
    [SerializeField] private AudioSource gunSound = null;

    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (gunSound != null)
        {
            gunSound.Play();
        }

        if (gunSound != null)
        {
            gunSound.Play();
        }

        gun.AddForce(0, recoil, 0, ForceMode.Impulse);

        if (Physics.Raycast(gunBarrel.transform.position, gunBarrel.transform.forward, out var hit, range))
        {
            Debug.Log(hit.transform.name);
        }

        if (hit.rigidbody)
        {
            hit.rigidbody.AddForceAtPosition(gunBarrel.transform.forward * force, hit.point);
        }
    }
}