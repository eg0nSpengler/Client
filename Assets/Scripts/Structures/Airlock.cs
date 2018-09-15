using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Structures.Doors
{
    [RequireComponent(typeof(Electrical))]
    [RequireComponent(typeof(AudioSource))]
    public class Airlock : MonoBehaviour, IInteractable
    {
        [Header("Sounds")] // TODO move these to some global settings somewhere
        public AudioClip openSound;
        public AudioClip closeSound;
        public AudioClip accessDeniedSound;

        [Header("Doors")] 
        [SerializeField] private Transform left;
        [SerializeField] private Transform right;

        [Header("States")] 
        [SerializeField] private bool open;
        [SerializeField] private bool moving;
        [SerializeField] private bool welded;
        [SerializeField] private bool bolted;
        [SerializeField] private bool disabled;

        [Header("Animation Settings")] 
        [SerializeField] private Vector3 openAxis = Vector3.right;
        [SerializeField] private float openDistance = 1.0f;
        [SerializeField] private float openTime = 1.0f;
        [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0,0,1,1);
        
        private AudioSource audioSource;
        private Electrical electrical;

        public bool Stuck => welded || bolted || disabled || electrical.Powered;

        
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
        
        public void Interact(GameObject obj)
        {
            if (moving || Stuck)
            {
                return;
            }

            moving = true;
            open = !open;

            StartCoroutine(Move());
            
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