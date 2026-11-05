using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwordPuzzle : MonoBehaviour
{
    public CrackableMagicStone stone;
    public FloatingSword sword;
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

    private float heldTime;
    private Coroutine strikeRoutine;

    private void OnEnable()
    {
        if (attackAction) attackAction.action.Enable();
    }

    private void Update()
    {
        // Left click always attacks in the direction the player is facing.
        // Another click restarts the combo instead of being ignored.
        if (attackAction.action.WasPressedThisFrame())
        {
            if (strikeRoutine != null) StopCoroutine(strikeRoutine);
            strikeRoutine = StartCoroutine(Strike());
        }
        if (IsAttacking) return;

        bool nearSword = Vector3.Distance(transform.position, sword.transform.position) <= pickupRange;
        if (sword.IsHovering && nearSword && interactAction.action.WasPressedThisFrame())
            sword.PickUp(swordHandTarget);

        // A tap equips the sword. Holding E for a second puts it in the array.
        if (sword.IsHeld && interactAction.action.IsPressed())
        {
            heldTime += Time.deltaTime;
            if (heldTime >= storeHoldTime) StoreSword();
        }
        else heldTime = 0f;
    }

    private IEnumerator Strike()
    {
        IsAttacking = true;
        strikeEvents.striking = true;
        int layer = animator.GetLayerIndex("Firefly Casting");
        animator.Play("Firefly Casting.Strike Magic Stone", layer, 0f);
        yield return new WaitForSeconds(comboDuration + 0.3f);
        strikeEvents.striking = false;
        IsAttacking = false;
        strikeRoutine = null;
    }

    private void StoreSword()
    {
        for (int i = 0; i < storedBlades.Length; i++)
        {
            if (storedBlades[i]) continue;
            storedBlades[i] = sword.gameObject;
            storedBladeCount++;
            sword.Store(storage);
            heldTime = 0f;
            return;
        }
    }

    private void OnGUI()
    {
        string message = "";
        if (!stone.IsBroken && stone.detector.swarm.IsAtStone && Vector3.Distance(transform.position, stone.transform.position) <= attackRange)
            message = IsAttacking ? "Attacking..." : "Face the stone and left click to strike";
        if (sword.IsHovering && Vector3.Distance(transform.position, sword.transform.position) <= pickupRange) message = "Tap E to hold the sword";
        if (sword.IsHeld) message = storedBladeCount >= storedBlades.Length ? "Blade storage is full" : "Hold E for 1 second to store the sword";
        if (message != "") GUI.Box(new Rect((Screen.width - 390f) / 2f, Screen.height - 90f, 390f, 40f), message);
        if (storedBladeCount > 0) GUI.Box(new Rect(Screen.width - 200f, 20f, 180f, 35f), "Stored blades: " + storedBladeCount + "/" + storedBlades.Length);
    }

    private void OnDisable()
    {
        if (attackAction) attackAction.action.Disable();
        if (IsAttacking)
        {
            StopAllCoroutines();
            strikeEvents.striking = false;
            IsAttacking = false;
            strikeRoutine = null;
        }
    }
}
