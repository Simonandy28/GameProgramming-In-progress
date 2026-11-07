using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(10)]
public class PlayerSwordPuzzle : MonoBehaviour
{
    public CrackableMagicStone stone;
    public FloatingSword sword;
    public CrackableMagicStone tigerStone;
    public StoneStrikeEvents strikeEvents;
    public Animator animator;
    public InputActionReference attackAction;
    public InputActionReference interactAction;
    public Transform swordHandTarget;
    public Transform storage;
    public float attackRange = 4.1f;
    public float pickupRange = 2.6f;
    public float comboDuration = 3.633333f;
    public float storeHoldTime = 1f;
    public GameObject[] storedBlades = new GameObject[8];
    public int storedBladeCount;
    public bool IsAttacking { get; private set; }
    public bool CanClaimSword => !IsAttacking && !HeldSword && ClaimableSword;
    public FloatingSword HeldSword => sword && sword.IsHeld ? sword :
        TigerSword && TigerSword.IsHeld ? TigerSword : null;

    private FloatingSword TigerSword => tigerStone ? tigerStone.sword : null;
    private FloatingSword ClaimableSword => IsCloseEnoughToPickUp(sword) ? sword :
        IsCloseEnoughToPickUp(TigerSword) ? TigerSword : null;

    private float heldTime;
    private bool holdingToStore;
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
        return blade && blade.IsHovering &&
            Vector3.Distance(transform.position, blade.transform.position) <= pickupRange;
    }

    private void UpdateSwordInput()
    {
        // A new press either picks up the sword or begins a storage hold.
        if (!IsAttacking && interactAction.action.WasPressedThisFrame())
        {
            if (HeldSword) holdingToStore = true;
            else
            {
                var blade = ClaimableSword;
                if (blade) blade.PickUp(swordHandTarget);
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
        if (!blade) return;
        for (int i = 0; i < storedBlades.Length; i++)
        {
            if (storedBlades[i]) continue;
            storedBlades[i] = blade.gameObject;
            storedBladeCount++;
            blade.Store(storage);
            return;
        }
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
}
