using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 positionAngle = new Vector2(0, 45);
    [SerializeField] private float distance = 6;
    [SerializeField] private float viewAngle = 45;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {

            positionAngle.x += 90;
            
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {

            positionAngle.x -= 90;

        }

        if(Input.GetKeyDown(KeyCode.Equals))
        {

            distance = distance + 2;

        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            distance = distance - 2;
        }

        if (distance < 6)
        {
            distance = distance + 1;
            return;
        }

        if (distance > 15)
        {
            distance = distance - 1;
            return;
        }



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