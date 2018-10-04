using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CharacterCameraReference))]
public class CharacterMovement : MonoBehaviour
{
    public event Action<Vector2> OnMove;
    
    [SerializeField] private float gravity = 10f;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float acceleration = 1f;
    
    private CharacterController characterController;
    private CharacterCameraReference characterCameraReference;
    private Vector2 currentDirection;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        characterCameraReference = GetComponent<CharacterCameraReference>();
    }

    private void Update()
    {
        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        currentDirection = Vector2.MoveTowards(currentDirection, input, acceleration * Time.deltaTime);
        
        if(currentDirection.magnitude > 0)
        {
            // Move relative to the camera
            Transform relativeMovement = characterCameraReference.GetCameraTransform();
            if(relativeMovement != null)
            {
                var forward = Vector3.ProjectOnPlane(relativeMovement.forward, Vector3.up);
                forward.Normalize();
                var right = Vector3.ProjectOnPlane(relativeMovement.right, Vector3.up);
                right.Normalize();
                
                var dir = right * currentDirection.x + forward * currentDirection.y;
                characterController.Move(dir * Time.deltaTime * speed);
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }

        characterController.Move(Vector3.down * gravity * Time.deltaTime); 
        
        OnMove?.Invoke(currentDirection);
    }
}