using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AddressableAssets;

public class EnemySpawner : MonoBehaviour
{
    private List<PlayerManager> players = new List<PlayerManager>();
    private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    private const int SPAWN_MIN_DISTANCE = 20, SPAWN_MAX_DISTANCE = 300;

    //Get current intensity based on player movement, action, and health.
    //If intensity is less than a certain value, then wait for a small period of time and spawn more enemies in.
    //I may want to move the "check intensity" bit to a different script.

    private void Update()
    {
        float avgIntensity = players.Average(x => x.intensity);
        if(avgIntensity < -20)
        {
            //SPAWN MOB and delay check before spawning mob again
        }
    }

    void SpawnEnemies(AssetReference enemyAsset, int enemyCount=1)
    {
        List<SpawnPoint> possiblePoints = spawnPoints.Where(x => {
            return players.All(y =>
            {
                float dist = Vector3.Distance(y.transform.position, x.transform.position);
                return dist <= SPAWN_MAX_DISTANCE && dist >= SPAWN_MIN_DISTANCE;
            });
        }).ToList();

        for (int i=0;i<enemyCount;i++)
        {
            possiblePoints[i % possiblePoints.Count].QueueSpawnEnemy(enemyAsset);
        }
    }
}
