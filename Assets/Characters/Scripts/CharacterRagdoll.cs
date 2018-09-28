using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterRagdoll : MonoBehaviour {
    
    [SerializeField] private bool ragdollToggle;
    [SerializeField] private AnimationCurve deragdollCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float deragdollTime = 0.5f;
    [SerializeField] private Transform chest;
    [SerializeField] private Transform armature;
    [SerializeField] private Transform root;
    [SerializeField] private Transform[] feet;
    
    private Animator animator;
    private Rigidbody[] ragdollBodies;

    // These are used to interpolate from ragdoll to animation
    private Vector3[] ragdollPosition;
    private Quaternion[] ragdollRotation;
    private float ragdollTime;

    // This only exists to let us ragdoll using the inspector. Alternatively a property is optimal.
    private bool ragdoll;

    private void Start()
    {
        animator = GetComponent<Animator>();
        
        ragdollBodies = animator.GetComponentsInChildren<Rigidbody>();
        ragdollPosition = new Vector3[ragdollBodies.Length];
        ragdollRotation = new Quaternion[ragdollBodies.Length];

        CheckRagdoll(true);
    }

    private void Update()
    {
        // Temporary for debugging. See comment on ragdoll
        CheckRagdoll();
    }

    private void CheckRagdoll(bool initialSetup = false)
    {
        if (ragdollToggle == ragdoll && !initialSetup)
        {
            return;
        }

        ragdoll = ragdollToggle;

        animator.enabled = !ragdollToggle;
        foreach (var body in ragdollBodies)
        {
            body.isKinematic = !ragdollToggle;
            body.detectCollisions = ragdollToggle;
        }

        // If returning to animation, lerp prepare to lerp from ragdoll to animation
        if (!ragdollToggle && !initialSetup)
        {
            
            animator.SetTrigger(Vector3.Dot(Vector3.up, chest.forward) < 0 ? "LyingFront" : "LyingBack");
            for (var i = 1; i < ragdollBodies.Length; i++)
            {
                ragdollPosition[i] = ragdollBodies[i].transform.position;
                ragdollRotation[i] = ragdollBodies[i].transform.rotation;
            }

            transform.position = feet.Aggregate(Vector3.zero, (sum, foot) => sum += foot.position) / feet.Length;
            armature.localPosition = Vector3.zero;
            root.localPosition = Vector3.zero;

            var forward = root.position - transform.position;
            forward.y = 0;
            transform.rotation = Quaternion.LookRotation(forward);
            armature.localRotation = Quaternion.identity;
            root.localRotation = Quaternion.identity;

            ragdollTime = deragdollTime;
        }
    }

    private void LateUpdate()
    {
        if (ragdollTime <= 0)
        {
            return;
        }

        var t = deragdollCurve.Evaluate(ragdollTime / deragdollTime);

        for (var i = 1; i < ragdollBodies.Length; i++)
        {
            ragdollBodies[i].transform.position = 
                Vector3.LerpUnclamped(ragdollBodies[i].transform.position, ragdollPosition[i], t);
            ragdollBodies[i].transform.rotation = 
                Quaternion.LerpUnclamped(ragdollBodies[i].transform.rotation, ragdollRotation[i], t);
        }
        ragdollTime -= Time.deltaTime;
    }
}
