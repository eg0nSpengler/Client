using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(Animator))]
public class CharacterAnimator : MonoBehaviour {
	
	[SerializeField] private string speedParameter = "Speed";

	private CharacterMovement characterMovement;
	private Animator animator;
    
	private void Start()
	{
		characterMovement = GetComponent<CharacterMovement>();
		animator = GetComponent<Animator>();

		characterMovement.OnMove += OnMove;
	}

	private void OnMove(Vector2 movement)
	{
        animator.SetFloat(speedParameter, movement.magnitude);
	}
}
