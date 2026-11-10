
using UnityEngine;

public class Puzzle3PoisonWater : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    public int RespawnCount { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        CharacterController player =
            other.GetComponent<CharacterController>();

        if (!player || !respawnPoint) return;
        RespawnCount++;

        player.enabled = false;
        player.transform.position = respawnPoint.position;
        player.enabled = true;
    }
}
