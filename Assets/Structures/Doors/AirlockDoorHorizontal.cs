using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Structures.Doors
{
	public enum AirlockState
	{
		Opened, Closed, Opening, Closing
	}

	public class AirlockDoorHorizontal : MonoBehaviour, IAirlockDoor, IInteractable
	{
		public float travelDistance = 1.0f;
		public float travelSpeed = 1.0f;
		public AudioClip openSound = null;
		public AudioClip closeSound = null;
		public AudioClip accessDeniedSound = null;
		private AudioSource audioSource; 
		private float openRatio = 0.0f; // ratio from 0 to 1
		private GameObject left = null;
		private GameObject right = null;
		public AirlockState state { get; private set; } = AirlockState.Closed;
		public bool welded { get; private set; } = false;
		public bool bolted { get; private set; } = false;
		public bool poweredDown { get; private set; } = false;
		public bool disabled { get; private set; } = false;

		// Use this for initialization
		void Start ()
		{		
			left = GameObject.Find("Left");
			right = GameObject.Find("Right");
			audioSource = gameObject.AddComponent<AudioSource>();
		}
	
		// Update is called once per frame
		void Update ()
		{
			float dt = Time.deltaTime;
			switch (state)		
			{
				case AirlockState.Opening:
					if (openRatio >= 1.0)
					{
						openRatio = 1.0f;
						state = AirlockState.Opened;
					}
					else
					{
						openRatio += dt * travelSpeed;
					}

					break;
				case AirlockState.Closing:
					if (openRatio <= 0.0)
					{
						openRatio = 0.0f;
						state = AirlockState.Closed;
					}
					else
					{
						openRatio -= dt * travelSpeed;
					}

					break;					
			}

			if (state == AirlockState.Closing || state == AirlockState.Opening)
			{
				left.transform.localPosition = new Vector3(travelDistance*openRatio, 0, 0);
				right.transform.localPosition = new Vector3(-travelDistance*openRatio, 0, 0);
			}
		
			if (Input.GetKey(KeyCode.O))
				SetOpen(true);
			if (Input.GetKey(KeyCode.P))
				SetOpen(false);
		}

	

		public void SetOpen(bool open)
		{
			if (open)
			{
				if (state == AirlockState.Closed || state == AirlockState.Closing)
				{
					state = AirlockState.Opening;
					if (openSound != null)
					{
						audioSource.clip = openSound;
						audioSource.Play();	
					}			
				}
			}
			else
			{
				if (state == AirlockState.Opening || state == AirlockState.Opened)
				{
					state = AirlockState.Closing;
					if (openSound != null)
					{
						audioSource.clip = closeSound;
						audioSource.Play();	
					}			
				}
			}
		
		}

		public void Interact(int id)
		{
			if (state == AirlockState.Closing || state == AirlockState.Closed)
			{
				SetOpen(true);
			}
		}
	}
}