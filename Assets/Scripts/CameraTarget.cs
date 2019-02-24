using System.Collections;
using System.Linq;
using Brisk;
using Brisk.Entities;
using Cinemachine;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField] private new CinemachineVirtualCamera camera;
    
    private Client client;
    private Transform target;
    
    private void Start()
    {
        client = FindObjectOfType<Client>();

        StartCoroutine(LookForPlayer());
    }

    private IEnumerator LookForPlayer()
    {
        while (target == null)
        {
            // TODO this is a hack. find a better solution. an event might be nice
            target = client == null 
                ? FindObjectOfType<CharacterMovement>()?.transform 
                : FindObjectsOfType<CharacterMovement>().SingleOrDefault(c => c.GetComponent<NetEntity>().Owner)?.transform;
            
            yield return new WaitForSeconds(0.1f);
        }
        
        // Assign the player to the camera
        camera.Follow = target;
        camera.LookAt = target;
        
        
        // Repeat in case we lose the player
        yield return new WaitUntil(() => target == null);
        StartCoroutine(LookForPlayer());
    }
}