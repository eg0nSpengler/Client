 using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Gun : MonoBehaviour
{
    [SerializeField]
    private float force = 200f;
    [SerializeField]
    private float damage = 1f;
    [SerializeField]
    private float recoil = 10f;
    [SerializeField]
    private float range = 100f;

    public Rigidbody gun;
    public Transform gunBarrel;
    public ParticleSystem muzzleFlash;
    [SerializeField]
    private AudioSource gunSound;

    


    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.E))
        {

            Shoot();

        }

    }

    void Shoot()
    {
        RaycastHit hit;

        if (gunSound != null)
        {
            gunSound.Play();
        }

        if (gunSound != null)
        {
            gunSound.Play();
        }

        gun.AddForce(0, recoil, 0, ForceMode.Impulse);

        if (Physics.Raycast(gunBarrel.transform.position, gunBarrel.transform.forward, out hit, range))
        {

            
            Debug.Log(hit.transform.name);

        }

        if (hit.rigidbody)
        {
            hit.rigidbody.AddForceAtPosition(gunBarrel.transform.forward * force, hit.point);
        }

    }
}