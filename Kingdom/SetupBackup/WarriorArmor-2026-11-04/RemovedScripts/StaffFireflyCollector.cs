
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class StaffFireflyCollector : MonoBehaviour
{
    [System.Serializable]
    public class FireflyEntry
    {
        public Transform fireflyRoot;
        public MonoBehaviour flightMotion;
    }

    [Header("Staff")]
    public Transform fireflyHoldPoint;

    [Header("Available Fireflies")]
    public FireflyEntry[] fireflies;

    [Header("Collection")]
    public float collectionRange = 12f;
    public float pullSpeed = 8f;

    [Header("Firefly Arrangement")]
    public float spacing = 0.15f;

    private List<Transform> heldFireflies = new List<Transform>();
    private HashSet<Transform> pullingFireflies = new HashSet<Transform>();

    public int CollectedCount
    {
        get { return heldFireflies.Count; }
    }

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.xKey.wasPressedThisFrame)
        {
            CollectNearbyFireflies();
        }
    }

    void LateUpdate()
    {
        if (fireflyHoldPoint == null)
            return;

        // Arrange collected fireflies around the staff tip.
        for (int i = 0; i < heldFireflies.Count; i++)
        {
            Transform firefly = heldFireflies[i];

            if (firefly == null)
                continue;

            float angle = i * Mathf.PI * 2f / 3f;

            Vector3 offset =
                fireflyHoldPoint.right * Mathf.Cos(angle) * spacing +
                fireflyHoldPoint.up * Mathf.Sin(angle) * spacing;

            firefly.position = fireflyHoldPoint.position + offset;
        }
    }

    void CollectNearbyFireflies()
    {
        if (fireflyHoldPoint == null)
        {
            Debug.LogWarning("Assign FireflyHoldPoint!");
            return;
        }

        int found = 0;

        foreach (FireflyEntry entry in fireflies)
        {
            if (entry == null || entry.fireflyRoot == null)
                continue;

            Transform firefly = entry.fireflyRoot;

            // Don't collect the same firefly twice.
            if (heldFireflies.Contains(firefly) ||
                pullingFireflies.Contains(firefly))
                continue;

            float distance = Vector3.Distance(
                transform.position,
                firefly.position
            );

            if (distance <= collectionRange)
            {
                StartCoroutine(PullFirefly(entry));
                found++;
            }
        }

        if (found == 0)
            Debug.Log("No uncollected fireflies nearby!");
    }

    IEnumerator PullFirefly(FireflyEntry entry)
    {
        Transform firefly = entry.fireflyRoot;

        if (firefly == null)
            yield break;

        pullingFireflies.Add(firefly);

        // Stop a movement script if one was assigned.
        if (entry.flightMotion != null)
            entry.flightMotion.enabled = false;

        // Stop the figure-eight Animator on the child.
        Animator[] animators =
            firefly.GetComponentsInChildren<Animator>();

        foreach (Animator animationController in animators)
        {
            animationController.enabled = false;
        }

        // Reset the animated FlightMotion child to the
        // firefly root so the visible fly follows the root.
        Transform flightChild = firefly.Find("FlightMotion");

        if (flightChild != null)
            flightChild.localPosition = Vector3.zero;

        Rigidbody rb = firefly.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Fly smoothly toward the moving staff.
        while (firefly != null &&
               fireflyHoldPoint != null &&
               Vector3.Distance(
                   firefly.position,
                   fireflyHoldPoint.position
               ) > 0.05f)
        {
            firefly.position = Vector3.MoveTowards(
                firefly.position,
                fireflyHoldPoint.position,
                pullSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (firefly != null)
        {
            heldFireflies.Add(firefly);

            Debug.Log(
                "Firefly collected! Total: " +
                heldFireflies.Count
            );

            pullingFireflies.Remove(firefly);
        }
    }

    // Releases one collected firefly.
    // We'll connect this to the lamp system next.
    public Transform ReleaseFirefly()
    {
        if (heldFireflies.Count == 0)
            return null;

        Transform released = heldFireflies[0];
        heldFireflies.RemoveAt(0);

        return released;
    }
}
