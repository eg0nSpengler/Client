using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterIK : MonoBehaviour
{
    [Header("Limbs")]
    [SerializeField] private Transform spine;
    [SerializeField] private Transform rightShoulder;
    [SerializeField] private Transform rightHand;
    [SerializeField] private Transform leftShoulder;
    [SerializeField] private Transform leftHand;

    [Header("Targets")]
    [SerializeField] private Transform rightHandTarget;
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform lookTarget;
    [SerializeField] private AnimationCurve weightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float weightTime = 0.5f;

    private Animator animator;

    // Different variables relating to IK, using dictionaries rather than left and right to keep it clean
    private readonly Dictionary<AvatarIKGoal, Vector3> generalDirection = new Dictionary<AvatarIKGoal, Vector3>();
    private readonly Dictionary<AvatarIKGoal, Vector3> armDirection = new Dictionary<AvatarIKGoal, Vector3>();
    private readonly Dictionary<AvatarIKGoal, Vector3> armUp = new Dictionary<AvatarIKGoal, Vector3>();
    private readonly Dictionary<AvatarIKGoal, Transform> armTarget = new Dictionary<AvatarIKGoal, Transform>();
    private readonly Dictionary<AvatarIKGoal, float> weight = new Dictionary<AvatarIKGoal, float>();
    private readonly Dictionary<AvatarIKGoal, Coroutine> weightRoutine = new Dictionary<AvatarIKGoal, Coroutine>();

    private void Start()
    {
        animator = GetComponent<Animator>();

        Initialize(AvatarIKGoal.RightHand);
        Initialize(AvatarIKGoal.LeftHand);
    }

    private void Initialize(AvatarIKGoal goal)
    {
        generalDirection[goal] = Vector3.up;
        armDirection[goal] = Vector3.forward;
        armUp[goal] = Vector3.up;
        armTarget[goal] = null;
        weight[goal] = 0;
        weightRoutine[goal] = null;
    }


    private void Update()
    {
        var leftDirection = leftShoulder.position - spine.position;
        var rightDirection = rightShoulder.position - spine.position;
        leftDirection.y = 0;
        rightDirection.y = 0;
        generalDirection[AvatarIKGoal.LeftHand] = leftDirection.normalized;
        generalDirection[AvatarIKGoal.RightHand] = rightDirection.normalized;
        
        armDirection[AvatarIKGoal.LeftHand] = (leftHand.position - leftShoulder.position).normalized;
        armDirection[AvatarIKGoal.RightHand] = (rightHand.position - rightShoulder.position).normalized;
        armUp[AvatarIKGoal.LeftHand] = -leftShoulder.forward;
        armUp[AvatarIKGoal.RightHand] = -rightShoulder.forward;

        CheckTarget(AvatarIKGoal.RightHand, rightHandTarget);
        CheckTarget(AvatarIKGoal.LeftHand, leftHandTarget);
    }

    private void CheckTarget(AvatarIKGoal goal, Transform target)
    {
        if (weightRoutine[goal] != null)
        {
            return;
        }

        if (target == null && armTarget[goal] != null)
        {
            HandleWeight(goal, 0, null);
        }
        else if (target != null && armTarget[goal] == null)
        {
            HandleWeight(goal, 1, target);
        }
    }

    private void HandleWeight(AvatarIKGoal goal, float newWeight, Transform target)
    {
        if (target != null)
        {
            armTarget[goal] = target;
        }

        if (weightRoutine[goal] != null)
        {
            StopCoroutine(weightRoutine[goal]);
        }

        weightRoutine[goal] = StartCoroutine(SetWeight(goal, newWeight));
    }

    private IEnumerator SetWeight(AvatarIKGoal goal, float target)
    {
        var origin = weight[goal];
        for (var time = 0f; time < weightTime; time += Time.deltaTime)
        {
            yield return null;

            weight[goal] = Mathf.LerpUnclamped(origin, target, weightCurve.Evaluate(time / weightTime));
        }

        weight[goal] = target;
        if (Mathf.Approximately(target, 0))
        {
            armTarget[goal] = null;
        }

        weightRoutine[goal] = null;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (lookTarget)
        {
            animator.SetLookAtWeight(1);
            animator.SetLookAtPosition(lookTarget.position);
        }
        else
        {
            animator.SetLookAtWeight(0);
        }

        HandIk(AvatarIKGoal.RightHand, AvatarIKHint.RightElbow);
        HandIk(AvatarIKGoal.LeftHand, AvatarIKHint.LeftElbow);
    }

    private void HandIk(AvatarIKGoal goal, AvatarIKHint hint)
    {
        animator.SetIKPositionWeight(goal, weight[goal]);
        animator.SetIKRotationWeight(goal, weight[goal]);
        animator.SetIKHintPositionWeight(hint, weight[goal]);

        if (armTarget[goal] == null)
        {
            return;
        }

        animator.SetIKPosition(goal, armTarget[goal].position);
        animator.SetIKRotation(goal, Quaternion.LookRotation(armDirection[goal], armUp[goal]));

        var dot = Vector3.Dot(generalDirection[goal], (armTarget[goal].position - spine.position).normalized);
        animator.SetIKHintPosition(hint, 
            Vector3.LerpUnclamped(spine.position + generalDirection[goal], spine.position - spine.up, dot));
    }

    // TODO: Rework this so it actually places in the hand in a non-arbitrary way
    public Transform GetItemHoldTransform(int handIndex) {
        handIndex %= 2;
        return handIndex == 0 ? leftHand : rightHand; 
    }
}