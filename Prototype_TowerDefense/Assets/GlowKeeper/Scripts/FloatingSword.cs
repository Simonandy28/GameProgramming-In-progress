using System.Collections;
using UnityEngine;

public class FloatingSword : MonoBehaviour
{
    public Rigidbody body;
    public Collider swordCollider;
    public float hoverHeight = 0.9f;
    public float bobHeight = 0.1f;
    [Header("Hand grip")]
    public Transform swordVisual;
    [Tooltip("Center of the handle in the sword model's own coordinates.")]
    public Vector3 modelGripPoint = new Vector3(0f, -0.05f, 0f);
    public Light swordLight;
    public float heldLightIntensity = 0.4f;
    public bool IsHovering { get; private set; }
    public bool IsHeld { get; private set; }

    private bool landed;
    private Vector3 landingPoint;
    private Vector3 hoverCenter;
    private Transform followedHand;

    public void ReleaseFromStone()
    {
        // Real gravity drops the sword before we start the magic hovering.
        body.isKinematic = false;
        body.useGravity = true;
        swordCollider.enabled = true;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.AddTorque(transform.forward * 0.35f, ForceMode.Impulse);
        StartCoroutine(FallThenHover());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.GetContact(0).normal.y > 0.4f)
        {
            landed = true;
            landingPoint = collision.GetContact(0).point;
        }
    }

    private IEnumerator FallThenHover()
    {
        yield return new WaitForSeconds(0.35f);
        float waiting = 0f;
        while (!landed && waiting < 1.5f)
        {
            waiting += Time.deltaTime;
            yield return null;
        }
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        body.isKinematic = true;
        hoverCenter = transform.position + Vector3.up * 0.4f;
        if (landed) hoverCenter.y = landingPoint.y + hoverHeight;
        IsHovering = true;
    }

    private void Update()
    {
        if (!IsHovering) return;
        Vector3 target = hoverCenter + Vector3.up * Mathf.Sin(Time.time * 2f) * bobHeight;
        float smooth = 1f - Mathf.Exp(-5f * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, target, smooth);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, Time.time * 35f, 0f), smooth);
    }

    private void LateUpdate()
    {
        if (followedHand)
            transform.SetPositionAndRotation(followedHand.position, followedHand.rotation);
    }

    public void PickUp(Transform hand)
    {
        StopAllCoroutines();
        IsHovering = false;
        IsHeld = true;
        body.isKinematic = true;
        swordCollider.enabled = false;
        // A sword inside a prefab group follows the socket in world space.
        if (transform.parent)
        {
            followedHand = hand;
            transform.SetPositionAndRotation(hand.position, hand.rotation);
        }
        else
        {
            transform.SetParent(hand, true); // Keep the sword's world size.
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
        // Stone-placement offsets must not leave the handle floating beside the palm.
        if (swordVisual)
        {
            swordVisual.localRotation = Quaternion.identity;
            swordVisual.localPosition = -Vector3.Scale(modelGripPoint, swordVisual.localScale);
        }
        if (swordLight)
        {
            swordLight.transform.localPosition = Vector3.zero;
            swordLight.intensity = heldLightIntensity;
        }
    }

    public void Store(Transform storage)
    {
        IsHeld = false;
        if (followedHand)
        {
            followedHand = null;
            transform.SetPositionAndRotation(storage.position, storage.rotation);
        }
        else
            transform.SetParent(storage, true);
        gameObject.SetActive(false); // The inventory array still remembers it.
    }
}
