using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One thing on a weapon platform that either fires or does some other action
/// </summary>
public class WeaponModule
{
    public WeaponModuleInfoObject weaponModuleInfoObject; 
    private short currentAmmoCount=0;
    private float nextFrameCanFire, nextFrameCanReload, nextFrameCanSwitchAmmo, nextFrameCanSetFireMode;//These values affect the whole weapon instead of individual modules
    private HitboxGroup ammoPools;
    private MagazineInfo currentMagazineInfo;//Null if unloaded
    private Transform shotOrigin;
    private Action attackAction;

    public WeaponModule(WeaponModuleInfoObject weaponModuleInfoObject, HitboxGroup ammoPools)
    {
        this.weaponModuleInfoObject = weaponModuleInfoObject;
        this.ammoPools = ammoPools;
    }

    public virtual bool CanFire()
    {
        return nextFrameCanFire < Time.time && currentAmmoCount > currentMagazineInfo.damageToAmmoPool.dmg;
    }

    public virtual bool CanReload()
    {
        if (Time.time < nextFrameCanReload) return false;
        int canLoadSome;
        ammoPools.DoDamageToHitboxGroup(currentMagazineInfo.damageToAmmoPool, currentMagazineInfo.damageToAmmoPool.dmg, out canLoadSome, false);
        return canLoadSome != 0;
    }
    public virtual bool CanSwitchAmmo()
    {
        return Time.time > nextFrameCanSwitchAmmo;
    }
    public virtual bool CanSetFireMode()
    {
        return Time.time > nextFrameCanSetFireMode;
    }

    public virtual bool NeedsToReload()
    {
        return currentAmmoCount < currentMagazineInfo.damageToAmmoPool.dmg;
    }

    public virtual void ChangeAmmoPoolsSource(HitboxGroup ammoPools)
    {
        this.ammoPools = ammoPools;
    }

    public virtual void Fire()
    {
        if (!CanFire())
        {
            Debug.LogError("Unable to fire when called to do so. This should not happen");
            return;
        }
        ammoPools.DoDamageToHitboxGroup(currentMagazineInfo.damageToAmmoPool);
        //TODO make Firmode actually do something. Accuracy and burst are important, but this is also supposed to cover things like ADS

        attackAction();

        nextFrameCanFire = Time.time + currentMagazineInfo.cyclicRate;
        nextFrameCanReload = nextFrameCanFire;
        nextFrameCanSetFireMode = nextFrameCanFire;
        nextFrameCanSwitchAmmo = nextFrameCanFire;
    }

    public virtual void Reload(short quantity)
    {

        if (quantity <0 ) quantity = currentMagazineInfo.magazineSize;
        quantity %= currentMagazineInfo.magazineSize;
        int dummy;
        currentAmmoCount = (short)(quantity-ammoPools.DoDamageToHitboxGroup(currentMagazineInfo.damageToAmmoPool, quantity - currentAmmoCount, out dummy) % currentMagazineInfo.magazineSize);

        nextFrameCanFire = Time.time + currentMagazineInfo.cyclicRate;
        nextFrameCanReload = nextFrameCanFire;
        nextFrameCanSetFireMode = nextFrameCanFire;
        nextFrameCanSwitchAmmo = nextFrameCanFire;
    }

    public virtual void Unload(short quantity)
    {
        ammoPools.DoDamageToHitboxGroup(currentMagazineInfo.damageToAmmoPool, -currentAmmoCount);

        if (quantity < 0) quantity = currentMagazineInfo.magazineSize;
        quantity %= currentMagazineInfo.magazineSize;
        int valueGottenFromPool;
        ammoPools.DoDamageToHitboxGroup(currentMagazineInfo.damageToAmmoPool, -quantity, out valueGottenFromPool);

        nextFrameCanFire = Time.time + currentMagazineInfo.cyclicRate;
        nextFrameCanReload = nextFrameCanFire;
        nextFrameCanSetFireMode = nextFrameCanFire;
        nextFrameCanSwitchAmmo = nextFrameCanFire;
    }

    public virtual void SwitchAmmo(DamageInfo.DamageTypes newAmmoType)
    {
        if (newAmmoType == currentMagazineInfo.damageToAmmoPool.damageType)
        {
            Debug.Log("Ammo types are the same");
            return;
        }
        for (int i = 0; i < weaponModuleInfoObject.acceptableAmmos.Count; i++)
        {
            if(weaponModuleInfoObject.acceptableAmmos[i].damageToAmmoPool.damageType == newAmmoType)
            {
                currentMagazineInfo = weaponModuleInfoObject.acceptableAmmos[i];

                switch (currentMagazineInfo.attackType)
                {
                    case MagazineInfo.AttackType.raycast:
                        attackAction = RaycastShot;
                        break;
                    case MagazineInfo.AttackType.projectile:
                        attackAction = ProjectileShot;
                        break;
                    case MagazineInfo.AttackType.hurtbox:
                        //TODO
                        break;
                    case MagazineInfo.AttackType.applySelf:
                        //TODO
                        break;
                    default: Debug.LogError("invalid attack type"); break;
                }
                break;
            }
        }
    }

    public virtual void SetFireMode(short fireMode)
    {
        weaponModuleInfoObject.fireMode = fireMode;
    }

    protected virtual void RaycastShot()
    {
        RaycastHit rayHit;
        if (Physics.Raycast(shotOrigin.position, shotOrigin.position, out rayHit, currentMagazineInfo.raycastDistance, currentMagazineInfo.raycastHittableLayers, QueryTriggerInteraction.Ignore))
        {
            Hitbox hitbox = rayHit.collider.gameObject.GetComponent<Hitbox>();
            if (hitbox == null) return;
            hitbox.DoDamageToHitbox(currentMagazineInfo.raycastDamageInfo);
        }
    }

    protected virtual void ProjectileShot()
    {
        ObjectPools.Spawn(currentMagazineInfo.projectileReference, (GameObject go) => {
            go.transform.position = shotOrigin.position;
            go.transform.rotation = shotOrigin.rotation;
            Rigidbody rb = go.GetComponent<Rigidbody>();
            rb.position = shotOrigin.position;
            //TODO Add parent object velocity?
            rb.velocity = shotOrigin.TransformDirection(currentMagazineInfo.localProjectileShotForce);
            return true;
        });
    }

    protected virtual void Melee()
    {
        //TODO
    }
}