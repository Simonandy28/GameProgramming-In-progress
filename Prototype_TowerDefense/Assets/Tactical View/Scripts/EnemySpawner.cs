using System.Collections;
using UnityEngine;

/// Enemy barracks: spawns waves of units automatically and sends them along the paths to the player's main tower.
/// On the way they fight everything they meet (UnitBehavior's own target search) and then march on.
public class EnemySpawner : MonoBehaviour
{
    [Tooltip("Where new units appear")]
    public Transform spawnPoint;
    [Tooltip("Picked at random for each unit, e.g. Skeleton Enemy and Knight Enemy")]
    public GameObject[] unitPrefabs;
    [Tooltip("What they march towards: the player's main tower (Ally Main)")]
    public Transform target;

    [Header("Waves")]
    [Tooltip("Seconds before the first wave")]
    public float firstDelay = 20f;
    [Tooltip("Seconds between waves")]
    public float waveInterval = 30f;
    [Tooltip("Units per wave: groups survive tower fire much better than single units")]
    [Min(1)] public int unitsPerWave = 3;

    // Start can be a coroutine too: it simply keeps running in the background.
    // The endless loop is fine because every pass waits (yield) before continuing.
    IEnumerator Start()
    {
        yield return new WaitForSeconds(firstDelay);
        while (true)
        {
            for (int i = 0; i < unitsPerWave; i++)
            {
                Spawn();
                yield return new WaitForSeconds(1f);   // small gap so they don't spawn inside each other
            }
            yield return new WaitForSeconds(waveInterval);
        }
    }

    void Spawn()
    {
        GameObject prefab = unitPrefabs[Random.Range(0, unitPrefabs.Length)];
        GameObject unit = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        unit.GetComponent<UnitBehavior>().SetMoveDestination(target.position);
    }
}
