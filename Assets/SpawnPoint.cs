using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SpawnPoint : MonoBehaviour
{
    Queue<AssetReference> enemiesToSpawn = new Queue<AssetReference>();
    private bool isAnimating;
    public void QueueSpawnEnemy(AssetReference enemyAsset)
    {
        enemiesToSpawn.Enqueue(enemyAsset);
        SpawnIfAble();
    }

    private void FinishedSpawning()
    {
        isAnimating = false;
        SpawnIfAble();
    }

    private void SpawnIfAble()
    {
        if (!isAnimating && enemiesToSpawn.Count > 0)
        {
            ObjectPools.Spawn(enemiesToSpawn.Dequeue(), go => { go.GetComponent<EnemyManager>().OnFinishedSpawnAnimating += FinishedSpawning; return true; });
        }
    }
}
