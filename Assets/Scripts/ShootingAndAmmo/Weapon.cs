using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A weapon platform that holds one or more modules that actually do the raycasting or projectile creation
/// </summary>
public class Weapon
{
    private WeaponInfoObject weaponInfoObject;
    private Dictionary<string, WeaponModule> weaponModules = new Dictionary<string, WeaponModule>();
    bool hasInitializedWeapon = false;
    [HideInInspector]
    public float nextFrameCanFire, nextFrameCanReload, nextFrameCanSwapWeapons, nextFrameCanSwitchAmmoType, nextFrameCanSetFireMode;//These values affect the whole weapon instead of individual modules

    public WeaponAnimator weaponAnim;
    private GameObject heroModel,thirdPersonModel;//Don't know if I actually need these

    public Weapon(WeaponInfoObject wio, HitboxGroup ammoPools)
    {
        if (hasInitializedWeapon) return;
        hasInitializedWeapon = true;
        weaponInfoObject = wio;
        //TODO Add First Person model if Appropriate
        //TODO Add Third Person model if appropriate
        //TODO Connect Animator
        //TODO Connect Functions to Animator

        //Populate Weapon Module Dictionary
        foreach (WeaponModuleInfoObject item in weaponInfoObject.startingWeaponModules)
        {
            WeaponModule wm = new WeaponModule(item, ammoPools);
            weaponModules.Add(item.Name, wm);
            weaponModules.Add(item.Position, wm);
        }
    }

    public void ChangeWeaponOwner(HitboxGroup ammoPools)
    {
        //TODO Add First Person model if Appropriate
        //TODO Add Third Person model if appropriate
        //TODO Connect Animator
        //TODO Connect Functions to Animator

        foreach (var item in weaponModules.Values)
        {
            item.ChangeAmmoPoolsSource(ammoPools);
        }
    }

    public virtual bool TryFire(string moduleName)
    {
        if (Time.time < nextFrameCanFire) return false;
        WeaponModule activeModule;
        if (!weaponModules.TryGetValue(moduleName, out activeModule))
        {
            Debug.LogWarning("Improper Weapon Module name, cannot fire");
            return false;
        }
        if (!activeModule.CanFire())
        {
            if(activeModule.NeedsToReload()) TryReload(moduleName);
            return false;
        }

        weaponAnim.partialFireEvent.RemoveAllListeners();

        weaponAnim.partialFireEvent.AddListener(activeModule.Fire);
        weaponAnim.Fire(weaponInfoObject.id, activeModule.weaponModuleInfoObject.id, activeModule.weaponModuleInfoObject.fireMode);
        return true;
    }

    public virtual bool TrySetFireMode(string moduleName, short newFireMode)
    {
        WeaponModule activeModule;
        if (!weaponModules.TryGetValue(moduleName, out activeModule))
        {
            Debug.LogWarning("Improper Weapon Module name");
            return false;
        }
        if (Time.time > nextFrameCanSetFireMode || !activeModule.CanSetFireMode()) return false;

        weaponAnim.partialSetFireModeEvent.RemoveAllListeners();
        weaponAnim.partialSetFireModeEvent.AddListener(() => activeModule.SetFireMode(newFireMode));
        weaponAnim.SetFireMode();
        return true;
    }

    public virtual bool TrySwitchAmmo(string moduleName, DamageInfo.DamageTypes ammoType)
    {
        WeaponModule activeModule;
        if (!weaponModules.TryGetValue(moduleName, out activeModule))
        {
            Debug.LogWarning("Improper Weapon Module name");
            return false;
        }
        if (Time.time > nextFrameCanSwitchAmmoType || !activeModule.CanSwitchAmmo()) return false;

        weaponAnim.partialUnloadEvent.RemoveAllListeners();
        weaponAnim.endUnloadEvent.RemoveAllListeners();

        weaponAnim.partialUnloadEvent.AddListener(() => activeModule.Unload(-1));
        weaponAnim.endUnloadEvent.AddListener(() => { activeModule.SwitchAmmo(ammoType); TryReload(moduleName); });
        weaponAnim.Unload();

        return true;
    }

    public virtual bool TryReload(string moduleName)
    {
        WeaponModule activeModule;
        if (!weaponModules.TryGetValue(moduleName, out activeModule))
        {
            Debug.LogWarning("Improper Weapon Module name");
            return false;
        }
        if (Time.time < nextFrameCanReload && !activeModule.CanReload()) return false;

        weaponAnim.partialReloadEvent.RemoveAllListeners();
        weaponAnim.partialReloadEvent.AddListener(() => activeModule.Reload(-1));
        weaponAnim.Reload();
        return true;
    }

    public (int,int) GetSlot()
    {
        return (weaponInfoObject.slot,weaponInfoObject.depthInSlot);
    }
    public int GetID()
    {
        return weaponInfoObject.id;
    }
    public virtual bool CanSwapWeapons()
    {
        return nextFrameCanSwapWeapons <= Time.time;
    }

    public void RemoveWeapon()
    {
        //Add ammo currently in weapon modules back into pools
        //TODO Destroy model/animator
        //TODO Destroy weapon module models
    }
}
