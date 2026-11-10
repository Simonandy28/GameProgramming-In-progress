
using System.Collections.Generic;
using UnityEngine;

public class Puzzle3PoisonWater : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    public int RespawnCount { get; private set; }

    private static readonly List<Puzzle3PoisonWater> activeZones = new();
    private BoxCollider area;

    private void OnEnable()
    {
        area = GetComponent<BoxCollider>();
        activeZones.Add(this);
    }

    private void OnDisable() => activeZones.Remove(this);

    public static bool BlocksSwimming(Vector3 position)
    {
        foreach (var zone in activeZones)
        {
            if (!zone.area || !zone.area.enabled) continue;
            Vector3 local = zone.area.transform.InverseTransformPoint(position) - zone.area.center;
            Vector3 halfSize = zone.area.size * 0.5f;
            if (Mathf.Abs(local.x) <= halfSize.x && Mathf.Abs(local.z) <= halfSize.z)
                return true;
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        CharacterController player =
            other.GetComponent<CharacterController>();

        if (!player || !respawnPoint) return;
        RespawnCount++;
        player.GetComponent<PlayerSwordPuzzle>()?.LoseUnsecuredCursedBlade();

        player.enabled = false;
        player.transform.position = respawnPoint.position;
        player.enabled = true;
    }
}
