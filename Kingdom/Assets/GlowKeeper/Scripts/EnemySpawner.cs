using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] List<Transform> spawnPoints;
    [SerializeField] float spawnCooldown = 10;
    [SerializeField] EnemyController enemyPrefab;
    [SerializeField] GameObject player;
    private float spawnTimer = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnTimer > spawnCooldown)
        {
            spawnEnemy();
            spawnTimer -= spawnCooldown;
        }
        spawnTimer += Time.deltaTime;
    }

    private void spawnEnemy()
    {
        int spawnIndex = Random.Range(0, spawnPoints.Count);
        EnemyController instance = Instantiate<EnemyController>(enemyPrefab, spawnPoints[spawnIndex]);
        instance.player = player;
        return;
    }
}
