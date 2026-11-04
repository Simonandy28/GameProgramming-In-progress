
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(StaffFireflyCollector))]
public class StaffLampDepositor : MonoBehaviour
{
    [Header("Lamps")]
    public LampReceiver[] lamps;

    [Header("Deposit Settings")]
    public float depositRange = 5f;
    public float flySpeed = 8f;
    public float spacing = 0.12f;

    private StaffFireflyCollector collector;
    private bool isDepositing = false;

    void Awake()
    {
        collector = GetComponent<StaffFireflyCollector>();
    }

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryDeposit();
        }
    }

    void TryDeposit()
    {
        if (isDepositing)
            return;

        if (collector.CollectedCount == 0)
        {
            Debug.Log("No fireflies on the staff!");
            return;
        }

        LampReceiver nearestLamp = null;
        float nearestDistance = depositRange;

        foreach (LampReceiver lamp in lamps)
        {
            if (lamp == null ||
                lamp.IsLit ||
                lamp.fireflyDestination == null)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                lamp.transform.position
            );

            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearestLamp = lamp;
            }
        }

        if (nearestLamp == null)
        {
            Debug.Log("No unlit lamp nearby!");
            return;
        }

        // Release ALL fireflies from the staff.
        List<Transform> releasedFireflies =
            new List<Transform>();

        while (collector.CollectedCount > 0)
        {
            Transform firefly = collector.ReleaseFirefly();

            if (firefly != null)
                releasedFireflies.Add(firefly);
        }

        if (releasedFireflies.Count > 0)
        {
            StartCoroutine(
                FlyAllToLamp(releasedFireflies, nearestLamp)
            );
        }
    }

    IEnumerator FlyAllToLamp(
        List<Transform> fireflies,
        LampReceiver lamp)
    {
        isDepositing = true;

        Transform destination = lamp.fireflyDestination;

        bool allArrived = false;

        while (!allArrived && destination != null)
        {
            allArrived = true;

            for (int i = 0; i < fireflies.Count; i++)
            {
                Transform firefly = fireflies[i];

                if (firefly == null)
                    continue;

                // Give each firefly a slightly different
                // position inside the same lamp.
                float angle =
                    i * Mathf.PI * 2f / fireflies.Count;

                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * spacing,
                    0f,
                    Mathf.Sin(angle) * spacing
                );

                Vector3 targetPosition =
                    destination.position + offset;

                firefly.position = Vector3.MoveTowards(
                    firefly.position,
                    targetPosition,
                    flySpeed * Time.deltaTime
                );

                if (Vector3.Distance(
                    firefly.position,
                    targetPosition
                ) > 0.05f)
                {
                    allArrived = false;
                }
            }

            yield return null;
        }

        // Keep all fireflies inside the lamp.
        if (destination != null)
        {
            for (int i = 0; i < fireflies.Count; i++)
            {
                Transform firefly = fireflies[i];

                if (firefly == null)
                    continue;

                float angle =
                    i * Mathf.PI * 2f / fireflies.Count;

                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * spacing,
                    0f,
                    Mathf.Sin(angle) * spacing
                );

                firefly.SetParent(destination, true);
                firefly.position =
                    destination.position + offset;
            }

            lamp.LightUp();

            Debug.Log(
                fireflies.Count +
                " fireflies deposited into " +
                lamp.gameObject.name
            );
        }

        isDepositing = false;
    }
}
