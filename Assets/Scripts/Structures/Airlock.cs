using System.Collections;
using System.Collections.Generic;
using Brisk.Actions;
using Brisk.Entities;
using UnityEngine;

namespace Structures.Doors
{
    [RequireComponent(typeof(Electrical))]
    [RequireComponent(typeof(AudioSource))]
    public class Airlock : NetBehaviour, IInteractable
    {
        [Header("Sounds")] // TODO move these to some global settings somewhere
        [SerializeField] private AudioClip openSound = null;
        [SerializeField] private AudioClip closeSound = null;

        [Header("Doors")] 
        [SerializeField] private Transform left = null;
        [SerializeField] private Transform right = null;

        [Header("States")] 
        [SerializeField] private bool open = false;
        [SerializeField] private bool moving = false;
        [SerializeField] private bool welded = false;
        [SerializeField] private bool bolted = false;
        [SerializeField] private bool disabled = false;

        [Header("Animation Settings")] 
        [SerializeField] private Vector3 openAxis = Vector3.right;
        [SerializeField] private float openDistance = 1.0f;
        [SerializeField] private float openTime = 1.0f;
        [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0,0,1,1);
        
        private AudioSource audioSource;
        private Electrical electrical;

        public bool Stuck => welded || bolted || disabled || !electrical.Powered;

        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            electrical = GetComponent<Electrical>();
        }

        private void Start()
        {
            if (left == null)
            {
                Debug.LogError("Left Door not set on airlock");
            }
            if (right == null)
            {
                Debug.LogError("Right Door not set on airlock");
            }
        }

        private IEnumerator Move() // TODO make a more performant, declarative manager or these simple animations
        {
            moving = true;
            open = !open;
            
            var origin = open ? 0f : 1f;
            var target = open ? 1f : 0f;
            for (var time = 0f; time < openTime; time += Time.deltaTime)
            {
                yield return null;
                
                var t = Mathf.Lerp(origin, target, time / openTime);

                left.transform.localPosition = openAxis * openDistance * openCurve.Evaluate(t);
                right.transform.localPosition = -openAxis * openDistance * openCurve.Evaluate(t);
            }

            left.transform.localPosition = openAxis * openDistance * openCurve.Evaluate(target);
            right.transform.localPosition = -openAxis * openDistance * openCurve.Evaluate(target);
            moving = false;
        }

        // TODO: Add logic for restrictions
        public bool IsInteractable(GameObject source)
        {
            return !moving && !Stuck;
        }
        
        [GlobalAction(false)]
        public void Interact()
        {
            StartCoroutine(Move());

            if (audioSource != null) audioSource.PlayOneShot(open ? openSound : closeSound);
        }
        
        public void Interact(GameObject source)
        {
            if (!IsInteractable(source)) {
                return;
            }

            this.Net_Interact();
            
            //StartCoroutine(Move());
            
            audioSource.PlayOneShot(open ? openSound : closeSound);
        }

#if UNITY_EDITOR
        [ContextMenu("Toggle")]
        private void Toggle()
        {
            if (moving)
            {
                return;
            }

            moving = true;
            open = !open;

            StartCoroutine(Move());
            
            audioSource.PlayOneShot(open ? openSound : closeSound);
        }
#endif
    }
}