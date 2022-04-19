using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The most outward-facing component of the health system that accepts damage (and healing as negative damage) and puts the damage through connected health pools
/// </summary>
[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class Hitbox : MonoBehaviour
{
    public HitboxGroup hitboxGroup;

    private void Start()
    {
        if(hitboxGroup == null)
        {
            Debug.Log("Hitbox not assigned group at start " + gameObject.name);
            return;
        }
        hitboxGroup.AddHitboxToGroup(this);
    }

    /// <summary>
    /// Damages (or heals) all connected health pools from top to bottom (heals vice versa) until damage is absorbed or passes through all boxes
    /// </summary>
    /// <param name="damageInfo"></param>
    /// <returns>Damage taken by this hitbox</returns>
    public int DoDamageToHitbox(DamageInfo damageInfo)
    {
        return hitboxGroup.DoDamageToHitboxGroup(damageInfo);
    }

    public void RemoveDOT(string dotName)
    {
        hitboxGroup.RemoveDOT(dotName);
    }

    /*
    public void RemoveStatusEffect(string dotName)
    {
        hitboxGroup.RemoveStatusEffect(dotName);
    }
    */

    private void OnDestroy()
    {
        HitboxGroupManager.hitboxByReference.Remove(gameObject.GetHashCode());
    }
}
