using UnityEngine;

// Put this on the avatar, because its Animator sends the hit events here.
public class StoneStrikeEvents : MonoBehaviour
{
    public CrackableMagicStone stone;
    public Animator animator;
    public bool striking;
    public float handClearance = 0.16f;

    private ThirdPersonAvatarController movement;
    private PlayerSwordPuzzle player;
    private float treadWeight;
    private CrackableMagicStone CurrentStone => player && player.stone ? player.stone : stone;

    private void Awake()
    {
        movement = GetComponentInParent<ThirdPersonAvatarController>();
        player = GetComponentInParent<PlayerSwordPuzzle>();
    }

    public void HitStone()
    {
        var target = CurrentStone;
        if (!striking || !player || !target || target.IsBroken || (movement && movement.IsSwimming)) return;
        Vector3 direction = target.transform.position - player.transform.position;
        direction.y = 0f;
        bool near = Vector3.Distance(player.transform.position, target.transform.position) <= player.attackRange;
        bool facing = Vector3.Dot(player.transform.forward, direction.normalized) > 0.5f;
        // The animation works anywhere. Only a nearby, correctly aimed hit damages the stone.
        if (near && facing) target.TakeHit(player.GetComponent<CharacterController>());
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (layerIndex != animator.GetLayerIndex("Firefly Casting")) return;
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);

        bool swimming = movement && animator.GetBool("IsSwimming");
        bool treading = swimming && !animator.GetBool("IsSwimMoving");
        treadWeight = Mathf.MoveTowards(treadWeight, treading ? 1f : 0f, Time.deltaTime * 8f);
        if (swimming)
        {
            float surface = movement.WaterSurface;
            if (treadWeight > 0f && !float.IsNegativeInfinity(surface))
            {
                LowerTreadingHand(AvatarIKGoal.RightHand, surface);
                LowerTreadingHand(AvatarIKGoal.LeftHand, surface);
            }
            return;
        }

        var target = CurrentStone;
        if (!striking || !target || target.IsBroken || !target.solidCollider.enabled) return;
        KeepHandOutside(AvatarIKGoal.RightHand, HumanBodyBones.RightUpperArm, target.solidCollider);
        KeepHandOutside(AvatarIKGoal.LeftHand, HumanBodyBones.LeftUpperArm, target.solidCollider);
    }

    private void LowerTreadingHand(AvatarIKGoal goal, float surface)
    {
        // Keep the paddling motion and ease high wrists toward the waterline.
        Vector3 hand = animator.GetIKPosition(goal);
        if (hand.y <= surface + 0.1f) return;
        hand.y = Mathf.Max(surface + 0.1f, hand.y - 0.55f);
        animator.SetIKPositionWeight(goal, treadWeight);
        animator.SetIKPosition(goal, hand);
    }

    private void KeepHandOutside(AvatarIKGoal goal, HumanBodyBones arm, Collider stoneCollider)
    {
        Vector3 shoulder = animator.GetBoneTransform(arm).position;
        Vector3 hand = animator.GetIKPosition(goal);
        Vector3 direction = hand - shoulder;
        Vector3 target = hand;
        RaycastHit hit;
        // Stop the hand just outside the rock, even if the clip swings through it.
        if (stoneCollider.Raycast(new Ray(shoulder, direction.normalized), out hit, direction.magnitude + handClearance))
            target = hit.point + hit.normal * handClearance;
        else
        {
            // Also protect hands when a swing starts very close to the rock.
            Vector3 outward = animator.transform.position - stoneCollider.bounds.center;
            outward.y = 0f;
            if (outward.sqrMagnitude < 0.001f) outward = -animator.transform.forward;
            outward.Normalize();
            float reach = stoneCollider.bounds.size.magnitude + 1f;
            if (stoneCollider.Raycast(new Ray(hand + outward * reach, -outward), out hit, reach + handClearance))
                target = hit.point + hit.normal * handClearance;
        }
        if (target != hand)
        {
            animator.SetIKPositionWeight(goal, 1f);
            animator.SetIKPosition(goal, target);
        }
    }
}
