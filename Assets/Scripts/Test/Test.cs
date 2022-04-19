using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Test : MonoBehaviour
{
    public AssetReference ar;

    private void Start()
    {
        Debug.Log("Runtime key valid " + ar.RuntimeKeyIsValid());
        Debug.Log("Runtime key " + ar.RuntimeKey);
        AssetReference copy = new AssetReference(ar.AssetGUID);
        Debug.Log("Runtime key valid 2 " + copy.RuntimeKeyIsValid());
        Debug.Log("Runtime key 2 " + copy.RuntimeKey);
    }
}
