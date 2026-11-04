using UnityEngine;

/// <summary>Fits the armored warrior's resting feet to the terrain below its movement capsule.</summary>
[RequireComponent(typeof(Animator))]
public sealed class WarriorFootPlacement : MonoBehaviour
{
    public float leftSoleHeight = 0.08f;
    public float rightSoleHeight = 0.08f;

    private Animator animator;
    private ThirdPersonAvatarController movement;
    private CharacterController capsule;
    private readonly RaycastHit[] hits = new RaycastHit[16];
    private float weight;
    private float bodyOffset;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponentInParent<ThirdPersonAvatarController>();
        capsule = GetComponentInParent<CharacterController>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (layerIndex != 0 || !movement || !capsule || !animator.isHuman)
            return;

        // Keep moving and airborne poses entirely under the existing animation controller.
        bool onLand = capsule.isGrounded && !animator.GetBool("IsJumping") &&
            !animator.GetBool("IsSwimming");
        bool resting = onLand && !animator.GetBool("IsWalking") && !animator.GetBool("IsRunning");
        weight = onLand ? Mathf.MoveTowards(weight, resting ? 1f : 0f, Time.deltaTime * 8f) : 0f;

        Vector3 left = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
        Vector3 right = animator.GetIKPosition(AvatarIKGoal.RightFoot);
        RaycastHit leftHit = default;
        RaycastHit rightHit = default;
        bool grounded = weight > 0f && GroundBelow(left, out leftHit) &&
            GroundBelow(right, out rightHit);

        if (!grounded)
        {
            weight = 0f;
            bodyOffset = 0f;
            SetWeights(0f);
            return;
        }

        Vector3 leftTarget = leftHit.point + Vector3.up * leftSoleHeight;
        Vector3 rightTarget = rightHit.point + Vector3.up * rightSoleHeight;
        float offset = Mathf.Clamp(Mathf.Min(leftTarget.y - left.y, rightTarget.y - right.y), -0.35f, 0.12f);
        bodyOffset = Mathf.Lerp(bodyOffset, offset, 1f - Mathf.Exp(-12f * Time.deltaTime));
        animator.bodyPosition += Vector3.up * (bodyOffset * weight);

        SetWeights(weight);
        animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftTarget);
        animator.SetIKPosition(AvatarIKGoal.RightFoot, rightTarget);
        animator.SetIKRotation(AvatarIKGoal.LeftFoot,
            Quaternion.FromToRotation(Vector3.up, leftHit.normal) * animator.GetIKRotation(AvatarIKGoal.LeftFoot));
        animator.SetIKRotation(AvatarIKGoal.RightFoot,
            Quaternion.FromToRotation(Vector3.up, rightHit.normal) * animator.GetIKRotation(AvatarIKGoal.RightFoot));
    }

    private void SetWeights(float value)
    {
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, value);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, value);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, value * 0.5f);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, value * 0.5f);
    }

    private bool GroundBelow(Vector3 foot, out RaycastHit ground)
    {
        ground = default;
        int count = Physics.RaycastNonAlloc(foot + Vector3.up * 0.5f, Vector3.down,
            hits, 1.2f, ~0, QueryTriggerInteraction.Ignore);
        float distance = float.PositiveInfinity;
        bool found = false;
        for (int i = 0; i < count; i++)
        {
            var hit = hits[i];
            if (hit.collider.transform.IsChildOf(movement.transform) || hit.normal.y < 0.4f || hit.distance >= distance)
                continue;
            ground = hit;
            distance = hit.distance;
            found = true;
        }
        return found;
    }
}
