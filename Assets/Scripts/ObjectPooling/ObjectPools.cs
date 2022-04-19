using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Static keeper of object instances
/// </summary>
public static class ObjectPools
{
    private static Dictionary<AssetReference, ObjectPoolObjectInfo> objectInfos = new Dictionary<AssetReference, ObjectPoolObjectInfo>();

    public static void Spawn(AssetReference ar, Func<GameObject, bool> doOnSpawned, bool forceAllStay = false)
    {
        if (!ar.RuntimeKeyIsValid())
        {
            Debug.LogError("Invalid Asset Reference");
            return;
        }

        if (objectInfos.ContainsKey(ar))
        {
            if (objectInfos[ar].asyncOperationHandles.IsDone)
            {
                SpawnObjectFromLoadedReference(ar, doOnSpawned);
            }
            else
            {
                EnqueSpawnForAfterInitialization(ar, doOnSpawned);
            }
            return;
        }
        LoadAndSpawn(ar, doOnSpawned);
    }

    public static void Spawn(string assetReference, Func<GameObject, bool> doOnSpawned)
    {
        AssetReference ar = new AssetReference(assetReference);  

        if (!ar.RuntimeKeyIsValid())
        {
            Debug.LogError("Invalid Asset Reference");
            return;
        }

        if (objectInfos.ContainsKey(ar))
        {
            if (objectInfos[ar].asyncOperationHandles.IsDone)
            {
                SpawnObjectFromLoadedReference(ar, doOnSpawned);
            }
            else
            {
                EnqueSpawnForAfterInitialization(ar, doOnSpawned);
            }
            return;
        }
        LoadAndSpawn(ar, doOnSpawned);
    }


    private static void SpawnObjectFromLoadedReference(AssetReference ar, Func<GameObject, bool> doOnSpawned)
    {
        if (objectInfos[ar].removeQueuedCorutine == null)
        {
            objectInfos[ar].removeQueuedCorutine = objectInfos[ar].RemoveQueuedCorutine();
        }
        else
        {
            objectInfos[ar].removeQueuedCorutine.Reset();
        }

        ar.InstantiateAsync().Completed += (asyncOperationHandle) =>
        {

            while (objectInfos[ar].disabledInstantiatedObjects.Count > 0)
            {
                GameObject temp = objectInfos[ar].disabledInstantiatedObjects.Dequeue().gameObject;
                if (temp != null) {
                    doOnSpawned(temp);
                    return;
                }
            }

            objectInfos[ar].spawnedObjects.Add(asyncOperationHandle.Result);
            var notify = asyncOperationHandle.Result.AddComponent<ObjectPoolObject>();
            notify.Destroyed += Remove;
            notify.Disabled += RePool;
            notify.assetReference = ar;
            doOnSpawned(asyncOperationHandle.Result);
        };
    }

    private static void EnqueSpawnForAfterInitialization(AssetReference ar, Func<GameObject, bool> doOnSpawned)
    {
        if (!objectInfos.ContainsKey(ar))
        {
            objectInfos[ar].queuedSpawnRequests = new Queue<Func<GameObject, bool>>();
        }
        objectInfos[ar].queuedSpawnRequests.Enqueue(doOnSpawned);
    }

    private static void LoadAndSpawn(AssetReference ar, Func<GameObject, bool> doOnSpawned)
    {
        var op = Addressables.LoadAssetAsync<GameObject>(ar);
        objectInfos.Add(ar, new ObjectPoolObjectInfo());
        objectInfos[ar].asyncOperationHandles = op;
        objectInfos[ar].doNotRemoveReference = false;
        objectInfos[ar].disabledInstantiatedObjects = new Queue<ObjectPoolObject>();
        op.Completed += (operation) =>
        {
            SpawnObjectFromLoadedReference(ar, doOnSpawned);
            if (objectInfos.ContainsKey(ar))
            {
                while (objectInfos[ar].queuedSpawnRequests?.Any() == true)
                {
                    var instantiatedFunction = objectInfos[ar].queuedSpawnRequests.Dequeue();
                    SpawnObjectFromLoadedReference(ar, instantiatedFunction);
                }
            }
        };
    }

    private static void Remove(AssetReference assetReference, ObjectPoolObject obj)
    {
        Addressables.ReleaseInstance(obj.gameObject);
        objectInfos[assetReference].spawnedObjects.Remove(obj.gameObject);
        bool checkForceStay =false, tryFindRemoveState = objectInfos[assetReference].doNotRemoveReference;
        if (objectInfos[assetReference].spawnedObjects.Count == 0 && (!checkForceStay || !tryFindRemoveState))
        {
            if (objectInfos[assetReference].asyncOperationHandles.IsValid())
            {
                Addressables.Release(objectInfos[assetReference].asyncOperationHandles);
            }
            objectInfos.Remove(assetReference);
        }
    }

    private static void RePool(AssetReference assetReference, ObjectPoolObject obj)
    {
        if (objectInfos.ContainsKey(assetReference))
        {
            objectInfos[assetReference].disabledInstantiatedObjects.Enqueue(obj);
        }
    }


    public static void SetRemoveState(AssetReference ar, bool forceStay)
    {
        if (objectInfos.ContainsKey(ar))
        {
            objectInfos[ar].doNotRemoveReference = forceStay;
        }
        else
        {
            Debug.LogWarning("Attempting to alter ObjectPoolObjectInfo that does not exist");
        }

        if (objectInfos[ar].spawnedObjects.Count == 0 && !forceStay)
        {
            if (objectInfos[ar].asyncOperationHandles.IsValid())
            {
                Addressables.Release(objectInfos[ar].asyncOperationHandles);
            }
            objectInfos.Remove(ar);
        }
    }

    private class ObjectPoolObjectInfo
    {
        public List<GameObject> spawnedObjects = new List<GameObject>();
        public Queue<Func<GameObject, bool>> queuedSpawnRequests = new Queue<Func<GameObject, bool>>();
        public AsyncOperationHandle<GameObject> asyncOperationHandles = new AsyncOperationHandle<GameObject>();
        public bool doNotRemoveReference;
        public Queue<ObjectPoolObject> disabledInstantiatedObjects = new Queue<ObjectPoolObject>();
        public IEnumerator removeQueuedCorutine;
        public int delayBeforeRemovingPooledObject = 20;

        public IEnumerator RemoveQueuedCorutine()
        {
            yield return new WaitForSeconds(delayBeforeRemovingPooledObject);
            while (disabledInstantiatedObjects.Count != 0 && !doNotRemoveReference)
            {
                GameObject.Destroy(disabledInstantiatedObjects.Dequeue().gameObject);
            }
        }
        public void ChangeDelayBeforeRemoval(int newDelay)// 0 or less is infinite
        {
            delayBeforeRemovingPooledObject = newDelay;
        }
    }
}
