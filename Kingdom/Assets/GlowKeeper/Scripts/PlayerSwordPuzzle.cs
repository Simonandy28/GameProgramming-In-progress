using System.Collections;
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
    [SerializeField, HideInInspector] private float comboDuration = 3.633333f;
    [SerializeField, HideInInspector] private float storeHoldTime = 1f;
    [HideInInspector] public GameObject[] storedBlades = new GameObject[8];
    [HideInInspector] public int storedBladeCount;
    public bool IsAttacking { get; private set; }
    public bool CanClaimSword => !IsAttacking && !HeldSword && ClaimableSword;
    public bool CanClaimCursedBlade => cursedSword && CanClaimSword && ClaimableSword == cursedSword;
    public bool IsHoldingCursedBlade => cursedSword && cursedSword.IsHeld;
    public FloatingSword HeldSword => sword && sword.IsHeld ? sword :
        TigerSword && TigerSword.IsHeld ? TigerSword : IsHoldingCursedBlade ? cursedSword : null;

    private FloatingSword TigerSword => tigerStone ? tigerStone.sword : null;
    private FloatingSword ClaimableSword => IsCloseEnoughToPickUp(sword) ? sword :
        IsCloseEnoughToPickUp(TigerSword) ? TigerSword :
        IsCloseEnoughToPickUp(cursedSword) ? cursedSword : null;

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
