using UnityEngine;

// Put this on the avatar, because its Animator sends the hit events here.
public class StoneStrikeEvents : MonoBehaviour
{
    public CrackableMagicStone stone;
    public Animator animator;
    public bool striking;
    public float handClearance = 0.16f;

    public void HitStone()
    {
        if (!striking || stone.IsBroken) return;
        var player = GetComponentInParent<PlayerSwordPuzzle>();
        Vector3 direction = stone.transform.position - player.transform.position;
        direction.y = 0f;
        bool near = Vector3.Distance(player.transform.position, stone.transform.position) <= player.attackRange;
        bool facing = Vector3.Dot(player.transform.forward, direction.normalized) > 0.5f;
        // The animation works anywhere. Only a nearby, correctly aimed hit damages the stone.
        if (near && facing) stone.TakeHit();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (layerIndex != animator.GetLayerIndex("Firefly Casting")) return;
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
        if (!striking || stone.IsBroken || !stone.solidCollider.enabled) return;
        KeepHandOutside(AvatarIKGoal.RightHand, HumanBodyBones.RightUpperArm);
        KeepHandOutside(AvatarIKGoal.LeftHand, HumanBodyBones.LeftUpperArm);
    }

    private void KeepHandOutside(AvatarIKGoal goal, HumanBodyBones arm)
    {
        Vector3 shoulder = animator.GetBoneTransform(arm).position;
        Vector3 hand = animator.GetIKPosition(goal);
        Vector3 direction = hand - shoulder;
        Vector3 target = hand;
        RaycastHit hit;
        // Stop the hand just outside the rock, even if the clip swings through it.
        if (stone.solidCollider.Raycast(new Ray(shoulder, direction.normalized), out hit, direction.magnitude + handClearance))
            target = hit.point + hit.normal * handClearance;
        else
        {
            // Also protect hands when a swing starts very close to the rock.
            Vector3 outward = animator.transform.position - stone.solidCollider.bounds.center;
            outward.y = 0f;
            if (outward.sqrMagnitude < 0.001f) outward = -animator.transform.forward;
            outward.Normalize();
            float reach = stone.solidCollider.bounds.size.magnitude + 1f;
            if (stone.solidCollider.Raycast(new Ray(hand + outward * reach, -outward), out hit, reach + handClearance))
                target = hit.point + hit.normal * handClearance;
        }
        if (target != hand)
        {
            animator.SetIKPositionWeight(goal, 1f);
            animator.SetIKPosition(goal, target);
        }
    }
}
