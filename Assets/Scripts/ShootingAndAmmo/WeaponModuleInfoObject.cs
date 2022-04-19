using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New WeaponModule", menuName = "Create/WeaponModule")]
public class WeaponModuleInfoObject : ScriptableObject
{
    public string Name;//underslungLauncher, burstFireRifle, demonWeapon, etc
    public string Position;//primaryFire, secondaryFire, switchFireMode, etc
                           //Because of how they're stored, the weapon cannot have two of the same Name or Position
    public List<MagazineInfo> acceptableAmmos = new List<MagazineInfo>();//Acceptable ammo types. The first one on the list the player has ammo for is default
    public UnityEngine.AddressableAssets.AssetReference heroModel, thirdPersonModel;
    public short fireMode;
    public int id;
}

[System.Serializable]
public class MagazineInfo
{
    public DamageInfo damageToAmmoPool;//Damage is cost per shot; damage type corresponds to ammo
    public short magazineSize = -1;//magazineSize of -1 is infinite
    public float cyclicRate;//TODO It may be better to accomplish this through animation
    public enum AttackType { raycast,projectile,hurtbox,applySelf};
    public AttackType attackType;
    public DamageInfo raycastDamageInfo;
    public float raycastDistance;
    public LayerMask raycastHittableLayers;
    public QueryTriggerInteraction hitTriggers;

    public UnityEngine.AddressableAssets.AssetReference projectileReference;
    public Vector3 localProjectileShotForce;
}
