using UnityEngine;

public class CharacterAnimator : MonoBehaviour {
	
	[SerializeField] private string speedParameter = "Speed";
	[SerializeField] private Animator animator = null;
	[SerializeField] private CharacterMovement characterMovement = null;
    
	private void Start()
	{
		characterMovement.OnMove += OnMove;
	}

	private void OnMove(Vector2 movement)
	{
        animator.SetFloat(speedParameter, movement.magnitude);
	}
}
