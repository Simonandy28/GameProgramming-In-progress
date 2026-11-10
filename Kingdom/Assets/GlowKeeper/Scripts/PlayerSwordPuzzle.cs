using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(10)]
public class PlayerSwordPuzzle : MonoBehaviour
{
    public CrackableMagicStone stone;
    public FloatingSword sword;
    public CrackableMagicStone tigerStone;
    [SerializeField] private FloatingSword cursedSword;
    [SerializeField] private Collider cursedSwordPlatform;
    [SerializeField] private StoneStrikeEvents strikeEvents;
    public Animator animator;
    public InputActionReference attackAction;
    public InputActionReference interactAction;
    public Transform swordHandTarget;
    public Transform storage;
    public float attackRange = 4.1f;
    public float pickupRange = 2.6f;
    [SerializeField] public List<FloatingSword> storedBlades = new List<FloatingSword>();
    [SerializeField, HideInInspector] private float comboDuration = 3.633333f;
    [SerializeField, HideInInspector] private float storeHoldTime = 1f;
    public bool IsAttacking { get; private set; }
    public bool CanClaimSword => !IsAttacking && !HeldSword && ClaimableSword;
    public bool CanClaimCursedBlade => cursedSword && CanClaimSword && ClaimableSword == cursedSword;
    public bool IsHoldingCursedBlade => cursedSword && cursedSword.IsHeld;
    public bool CarryingUnsecuredCursedBlade => IsHoldingCursedBlade && !cursedBladeSecured;
    public FloatingSword HeldSword
    {
        get
        {
            foreach (FloatingSword blade in FloatingSword.ActiveBlades)
            {
                if (blade && blade.IsHeld)
                    return blade;
            }

            return null;
        }
    } //sword && sword.IsHeld ? sword :
      //TigerSword && TigerSword.IsHeld ? TigerSword : IsHoldingCursedBlade ? cursedSword : null;

    private FloatingSword TigerSword => tigerStone ? tigerStone.sword : null;
    private FloatingSword ClaimableSword
    {
        get
        {
            FloatingSword closest = null;
            float closestDistance = pickupRange;

            foreach (FloatingSword blade in FloatingSword.ActiveBlades)
            {
                if (!blade || !IsCloseEnoughToPickUp(blade))
                    continue;

                float distance = Vector3.Distance(
                    transform.position,
                    blade.transform.position
                );

                if (distance >= closestDistance)
                    continue;

                closest = blade;
                closestDistance = distance;
            }

            return closest;
        }
    }
    private float heldTime;
    private bool holdingToStore;
    private bool cursedBladeSecured;
    private Coroutine strikeRoutine;
    private ThirdPersonAvatarController movement;

    private void Awake()
    {
        movement = GetComponent<ThirdPersonAvatarController>();
    }

    private void OnEnable()
    {
        if (attackAction) attackAction.action.Enable();
        if (interactAction) interactAction.action.Enable();
    }

    private void Update()
    {
        if (movement && movement.IsSwimming)
        {
            // Entering the water also cancels a swing started on land.
            if (IsAttacking)
            {
                StopCoroutine(strikeRoutine);
                ClearStrike();
                animator.Play("Firefly Casting.Ready", animator.GetLayerIndex("Firefly Casting"), 0f);
            }
        }
        else if (attackAction && attackAction.action.WasPressedThisFrame())
        {
            if (strikeRoutine != null) StopCoroutine(strikeRoutine);
            strikeRoutine = StartCoroutine(Strike());
        }
        if (interactAction) UpdateSwordInput();
    }

    private bool IsCloseEnoughToPickUp(FloatingSword blade)
    {
        if (!blade || Vector3.Distance(transform.position, blade.transform.position) > pickupRange)
            return false;
        if (blade != cursedSword) return blade.IsHovering;

        // The guardian enables the blade with the platforms. Claim it from the centre stone.
        return blade.gameObject.activeInHierarchy && !blade.IsHeld && cursedSwordPlatform &&
            GetComponent<CharacterController>().isGrounded &&
            Physics.Raycast(transform.position + Vector3.up * 0.15f, Vector3.down,
                out RaycastHit ground, 0.55f, ~0, QueryTriggerInteraction.Ignore) &&
            ground.collider == cursedSwordPlatform;
    }

    private void UpdateSwordInput()
    {
        // A new press either picks up the sword or begins a storage hold.
        if (!IsAttacking && interactAction.action.WasPressedThisFrame())
        {
            if (HeldSword) holdingToStore = !CarryingUnsecuredCursedBlade;
            else
            {
                FloatingSword blade = ClaimableSword;

                if (blade)
                    blade.PickUp(swordHandTarget);
                else
                    DrawSword();
            }
        }

        if (IsAttacking || !interactAction.action.IsPressed())
        {
            holdingToStore = false;
            heldTime = 0f;
        }
        if (!holdingToStore) return;

        heldTime += Time.deltaTime;
        if (heldTime < storeHoldTime) return;
        StoreSword(HeldSword);
        holdingToStore = false;
        heldTime = 0f;
    }

    private IEnumerator Strike()
    {
        IsAttacking = true;
        strikeEvents.striking = true;
        int layer = animator.GetLayerIndex("Firefly Casting");
        animator.Play("Firefly Casting.Strike Magic Stone", layer, 0f);
        yield return new WaitForSeconds(comboDuration + 0.3f);
        ClearStrike();
    }

    private void ClearStrike()
    {
        strikeEvents.striking = false;
        IsAttacking = false;
        strikeRoutine = null;
    }

    private void StoreSword(FloatingSword blade)
    {
        if (!blade || !blade.IsHeld || storedBlades.Contains(blade))
            return;

        storedBlades.Add(blade);
        blade.Store(storage);
    }

    public void SecureCursedBlade()
    {
        if (IsHoldingCursedBlade) cursedBladeSecured = true;
    }

    public void LoseUnsecuredCursedBlade()
    {
        if (!CarryingUnsecuredCursedBlade) return;
        cursedSword.ReturnToStand();
        holdingToStore = false;
        heldTime = 0f;
    }

    private void OnDisable()
    {
        if (attackAction) attackAction.action.Disable();
        if (IsAttacking)
        {
            StopAllCoroutines();
            ClearStrike();
        }
    }

    private void DrawSword()
    {
        if (HeldSword || storedBlades.Count == 0) return;

        FloatingSword bestBlade = null;

        foreach (FloatingSword blade in storedBlades)
        {
            if (!blade) continue;

            if (!bestBlade || blade.damage > bestBlade.damage)
                bestBlade = blade;
        }

        if (!bestBlade) return;

        storedBlades.Remove(bestBlade);
        bestBlade.Draw(swordHandTarget);
    }
}
