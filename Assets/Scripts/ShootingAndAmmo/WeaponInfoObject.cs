using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInfoObject : ScriptableObject
{
    public List<WeaponModuleInfoObject> startingWeaponModules = new List<WeaponModuleInfoObject>();
    public int slot = 0, depthInSlot = 0, id=0;
    public UnityEngine.AddressableAssets.AssetReference heroModel,thirdPersonModel;
}
