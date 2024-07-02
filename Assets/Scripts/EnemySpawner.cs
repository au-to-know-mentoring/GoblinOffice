using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // Assign this in the Inspector with your enemy prefab
    public GameObject EnemiesParent; // Assign this in the Inspector
    public Transform[] spawnPoints; // Assign spawn points in the Inspector if you have specific spawn locations

    public void SpawnRandomAmountOfEnemies(float difficulty)
    {
        Debug.Log("enemies should be spawned");
        // Assuming 'difficulty' is a float variable ranging from 1 to 5
        int minEnemies = Mathf.CeilToInt(difficulty); // This ensures that the minimum number of enemies increases with difficulty
        int maxEnemies = 1 + (int)difficulty; // Maximum number of enemies remains the same
        Debug.Log("MaxE: " + maxEnemies);
        int enemiesToSpawn = UnityEngine.Random.Range(minEnemies, maxEnemies); // Random number between 1 and 5
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            GameObject enemy;
            if (availableSpawnPoints.Count > 0)
            {
                // Select a random spawn point from the available ones
                int spawnIndex = UnityEngine.Random.Range(0, availableSpawnPoints.Count);
                Transform spawnPoint = availableSpawnPoints[spawnIndex];
                enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity, EnemiesParent.transform);

                // Remove the used spawn point from the list
                availableSpawnPoints.RemoveAt(spawnIndex);
            }
            else
            {
                // Otherwise, just spawn them at random positions or a default position
                Vector3 randomPosition = new Vector3(UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10), 0); // Example random position
                enemy = Instantiate(enemyPrefab, randomPosition, Quaternion.identity, EnemiesParent.transform);
            }

            // Optionally, initialize the enemy or set it up as needed
            // Example: enemy.GetComponent<Enemy>().Initialize(...);
        }
    }
}