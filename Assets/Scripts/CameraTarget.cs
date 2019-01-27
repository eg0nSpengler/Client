using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 positionAngle = new Vector2(0, 45);
    [SerializeField] private float distance = 5;
    [SerializeField] private float viewAngle = 45;

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        transform.position = target.transform.position + new Vector3(
                                                Mathf.Cos(Mathf.Deg2Rad * positionAngle.x),
                                                Mathf.Sin(Mathf.Deg2Rad * positionAngle.y),
                                                Mathf.Sin(Mathf.Deg2Rad * positionAngle.x)).normalized * distance;
        
        transform.rotation = Quaternion.Euler(viewAngle, 270-positionAngle.x, 0);
    }
}