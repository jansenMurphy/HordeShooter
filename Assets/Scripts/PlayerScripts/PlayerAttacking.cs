using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttacking : MonoBehaviour
{
    public PlayerManager pm;
    private List<Weapon> weapons = new List<Weapon>();
    public HitboxGroup ammoSelections;
    private Weapon currentWeapon;
    private int currentWeaponIndex = -1;
    private const string PRIMARY_FIRE = "primaryFire", SECONDARY_FIRE = "secondaryFire";

    private UnityEngine.Events.UnityEvent<int> addedWeapon,removedWeapon;//The weapon's ID is passed
    private void Start()
    {
        if (pm == null) pm = GetComponent<PlayerManager>();
        pm.fireDelegate += Fire;
        pm.fireDelegate += AltFire;
        //pm.incDecWeaponDElegate += IncDecWeapon;
        //pm.switchAmmoDElegate += SwitchAmmo;
        //pm.switchWeaponToIDDElegate += SwitchWeaponToID;
    }

    private void Fire(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(currentWeapon == null)
        {
            Debug.LogWarning("Null weapon object");
            return;
        }
        currentWeapon.TryFire(PRIMARY_FIRE);
    }

    private void AltFire(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (currentWeapon == null)
        {
            Debug.LogWarning("Null weapon object");
            return;
        }
        currentWeapon.TryFire(SECONDARY_FIRE);
    }

    private void SwitchAmmo(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (currentWeapon == null)
        {
            Debug.LogWarning("Null weapon object");
            return;
        }
        //currentWeapon.TrySwitchAmmo(); TODO
    }

    private void SetFireMode(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (currentWeapon == null)
        {
            Debug.LogWarning("Null weapon object");
            return;
        }
        currentWeapon.TrySetFireMode("primary",0);
    }

    private void Reload(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (currentWeapon == null)
        {
            Debug.LogWarning("Null weapon object");
            return;
        }
        currentWeapon.TryReload(PRIMARY_FIRE);
    }

    private void IncDecWeapon(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        float scrollVal;
        if((scrollVal = context.ReadValue<float>()) > 0)
        {
            NextWeapon();
        }
        else if (scrollVal < 0)
        {
            PreviousWeapon();
        }
    }
    private void SwitchWeaponToID(int id)
    {
        int nextIndex, currentID = currentWeapon.GetID();
        if(id == currentID)
        {
            if (weapons[nextIndex =(currentWeaponIndex+1)%weapons.Count].GetID() != currentID)
            {
                SwitchWeaponToIndex(nextIndex);
                return;
            }
            for (nextIndex = 0; nextIndex < currentWeaponIndex; nextIndex++)
            {
                if(weapons[nextIndex].GetID() == id)
                {
                    SwitchWeaponToIndex(nextIndex);
                    return;
                }
            }
            return;
        }
        for (nextIndex = 0; nextIndex < weapons.Count; nextIndex++)
        {
            if (weapons[nextIndex].GetID() == id)
            {
                SwitchWeaponToIndex(nextIndex);
                return;
            }
        }
    }
    
    private void SwitchWeaponToIndex(int newIndex, bool forceSwap=false)
    {
        if (weapons.Count == 0)
        {
            Debug.Log("No weapons to switch to or from");
            return;
        }

        newIndex %= weapons.Count;
        if(newIndex != currentWeaponIndex && (forceSwap || currentWeapon.CanSwapWeapons()))
        {
            Weapon previousWeapon = currentWeapon;
            currentWeapon = weapons[newIndex];
            currentWeaponIndex = newIndex;

            previousWeapon.weaponAnim.Holster();
            previousWeapon.weaponAnim.endHolsterEvent.AddListener(CallWeaponDrawAnim);
        }
    }

    private void SwitchWeaponToID(int newID, bool forceSwap = false)
    {
        if (weapons.Count == 0)
        {
            Debug.Log("No weapons to switch to or from");
            return;
        }

        for (int i = 0; i < weapons.Count; i++)
        {
            if(weapons[i].GetID() == newID)
            {
                SwitchWeaponToIndex(i);
                return;
            }
        }
    }

    public void NextWeapon()
    {
        SwitchWeaponToIndex(currentWeaponIndex + 1);
    }

    public void PreviousWeapon()
    {
        SwitchWeaponToIndex(currentWeaponIndex - 1);
    }

    private void CallWeaponDrawAnim()
    {
        if (currentWeapon == null)
        {
            Debug.LogWarning("Null weapon object");
            return;
        }
        currentWeapon.weaponAnim.Draw();
    }

    public void AddWeapon(WeaponInfoObject weaponInfoObject, bool switchToNewWeapon = false, bool replaceWeaponIfSameSlots = false)
    {
        int addAtIndex = -1;
        (int, int)? slotVals;
        do
        {
            addAtIndex++;
            slotVals = weapons[addAtIndex]?.GetSlot();
        } while (addAtIndex < weapons.Count && (slotVals.Value.Item1 < weaponInfoObject.slot || (slotVals.Value.Item1 == weaponInfoObject.slot && slotVals.Value.Item2 < weaponInfoObject.depthInSlot )));
        if(replaceWeaponIfSameSlots && slotVals.Value.Item1 == weaponInfoObject.slot && slotVals.Value.Item2 == weaponInfoObject.depthInSlot)
            RemoveWeapon(addAtIndex);
        if(weaponInfoObject.id != weapons[addAtIndex].GetID())
            weapons.Insert(addAtIndex, new Weapon(weaponInfoObject,ammoSelections));
        if (weapons.Count == 1)
            SwitchWeaponToIndex(0);
        if (switchToNewWeapon)
            SwitchWeaponToIndex(addAtIndex);
        addedWeapon.Invoke(weaponInfoObject.id);
    }

    public void RemoveWeapon(WeaponInfoObject weaponInfoObject)
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            if(weapons[i].GetID() == weaponInfoObject.id)
            {
                RemoveWeapon(i);
                return;
            }
        }
    }
    public void RemoveCurrentWeapon()
    {
        RemoveWeapon(currentWeaponIndex);
    }

    private void RemoveWeapon(int index)
    {
        weapons[index].RemoveWeapon();
        weapons.RemoveAt(index);
        removedWeapon.Invoke(weapons[index].GetID());
    }
}
