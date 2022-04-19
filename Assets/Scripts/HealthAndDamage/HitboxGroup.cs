using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HitboxGroup : MonoBehaviour
{

    //TODO Status Effects


    private List<Hitbox> hitboxes = new List<Hitbox>();

    /// <summary>
    /// From Innermost to outermost, which health pools are connected to this hitbox group; Outermost are hit first when doing positive damage and last when doing negative damage
    /// </summary>
    public HealthPool[] healthPools;
    public ResistVulnSet startingResistances;
    private Dictionary<DamageInfo.DamageTypes, ResistanceVulnerability> hitboxResistsAndVulns = new Dictionary<DamageInfo.DamageTypes, ResistanceVulnerability>();

    private Dictionary<string, DamageOverTime> DOTs = new Dictionary<string, DamageOverTime>();

    //private Dictionary<string, StatusEffect> statusEffects = new Dictionary<string, StatusEffect>();

    public UnityEvent<int> takeDamageEvent = new UnityEvent<int>();

    void Start()
    {
        HitboxGroupManager.hitboxByReference.Add(GetHashCode(), this);
        ResistanceVulnerability.AddResistancesToDictionary(startingResistances, hitboxResistsAndVulns);
    }

    public void AddHitboxToGroup(Hitbox hitbox)
    {
        hitboxes.Add(hitbox);
    }
    public void RemoveHitboxFromGroup(Hitbox hitbox)
    {
        hitboxes.Remove(hitbox);
    }

    public bool TryGetResistVuln(DamageInfo.DamageTypes reference, out ResistanceVulnerability resistVuln)
    {
        return hitboxResistsAndVulns.TryGetValue(reference, out resistVuln);
    }
    /*
    public bool TryGetStatusEffect(string reference, out StatusEffect statusEffect)
    {
        return statusEffects.TryGetValue(reference, out statusEffect);
    }
    */


    /// <summary>
    /// Does damage to a hitbox group
    /// </summary>
    /// <param name="damageInfo">The damage type dealt</param>
    /// <returns>The remainder of damage that could not be taken by any pool</returns>
    public int DoDamageToHitboxGroup(DamageInfo damageInfo, bool actuallyDoDamage = true)
    {
        int dummy;
        return DoDamageToHitboxGroup(damageInfo,damageInfo.dmg, out dummy,actuallyDoDamage);
    }

    /// <summary>
    /// Does damage to a hitbox group
    /// </summary>
    /// <param name="damageInfo">The damage type dealt</param>
    /// <param name="damage">An override for damageInfo's dmg value</param>
    /// <param name="damageDealt">How much damage was dealt after Armor</param>
    /// <returns>The remainder of damage that could not be taken by any pool</returns>
    public int DoDamageToHitboxGroup(DamageInfo damageInfo, int damage, bool actuallyDoDamage = true)
    {
        int dummy;
        return DoDamageToHitboxGroup(damageInfo, damage, out dummy, actuallyDoDamage);
    }

    /// <summary>
    /// Does damage to a hitbox group
    /// </summary>
    /// <param name="damageInfo">The damage type dealt</param>
    /// <param name="damage">An override for damageInfo's dmg value</param>
    /// <param name="damageDealt">How much damage was dealt after Armor</param>
    /// <returns>The remainder of damage that could not be taken by any pool</returns>
    public int DoDamageToHitboxGroup(DamageInfo damageInfo, int damage, out int damageDealt, bool actuallyDoDamage = true)
    {
        damageDealt = 0;
        ResistanceVulnerability hitResistVuln;
        if (TryGetResistVuln(damageInfo.damageType, out hitResistVuln))
        {
            //Multiply damage by vulnerability, then reduce by Armor - Armor Piercing min 
            if (damage > 0)
            {
                if (hitResistVuln.blockDamage) return 0;
                damage = (int)(damage * hitResistVuln.damageMult) - Mathf.Max(hitResistVuln.armor - damageInfo.armorPiercing, 0);
                damageInfo.armorPiercing = Mathf.Max(-hitResistVuln.armor + damageInfo.armorPiercing, 0);
            }
            else
            {
                if (hitResistVuln.blockHeal) return 0;
                damage = (int)(damage * hitResistVuln.healmult) - Mathf.Max(hitResistVuln.impedence - damageInfo.armorPiercing, 0);
                damageInfo.armorPiercing = Mathf.Max(-hitResistVuln.impedence + damageInfo.armorPiercing, 0);
            }


            //If there are status effects on this hitbox, apply them
            /*
            StatusEffect se;
            if (hitboxGroup.TryGetStatusEffect(damageInfo.damageType, out se))
            {
                if (damage > 0)
                {
                    if (se.rv.blockDamage) return 0;
                    damage = (int)(damage * se.rv.damageMult) - Mathf.Max(se.rv.armor - damageInfo.armorPiercing, 0);
                    damageInfo.armorPiercing = Mathf.Max(-se.rv.armor + damageInfo.armorPiercing, 0);
                }
                else
                {
                    if (se.rv.blockHeal) return 0;
                    damage = (int)(damage * se.rv.healmult) - Mathf.Max(se.rv.impedence - damageInfo.armorPiercing, 0);
                    damageInfo.armorPiercing = Mathf.Max(-se.rv.impedence + damageInfo.armorPiercing, 0);
                }
            }
            */


            {//Do Damage to pools
                int sanityCheck = 0, i = damageInfo.damageType > 0 ? healthPools.Length - 1 : 0;
                while (i >= 0 && i < healthPools.Length && damage != 0)
                {
                    int temp;
                    damage = healthPools[i].DoDamageToPool(damageInfo, damage, out temp);
                    damageDealt += temp;
                    i = i - (int)Mathf.Sign(damage);
                    sanityCheck++;
                    if (sanityCheck > 100)
                    {
                        Debug.LogWarning("Damage is pingponging indefinitely");
                        if (actuallyDoDamage)
                            takeDamageEvent.Invoke(damageDealt);
                        return damage;
                    }
                }
                if (actuallyDoDamage)
                    takeDamageEvent.Invoke(damageDealt);
                return damage;
            }
        }
        Debug.LogWarning("No connected pool was able to deal with that damage");
        return 0;
    }

    /// <summary>
    /// Adds a Damage Over Time (or Heal Over Time) to this Hitbox's active DOTs. Currently active DOTs with the same DOTName may be replaced, extended, or refreshed, or ignore the new DOT.
    /// </summary>
    /// <param name="dot"></param>
    public void AddDOT(DamageOverTime dot)
    {
        DamageOverTime activeDOT;
        if (!DOTs.TryGetValue(dot.damageOverTimeObject.DOTName, out activeDOT))
        {
            if (dot.damageOverTimeObject.dotStackMethod == DamageOverTime.DOTStackMethod.Simultaneous)
            {
                DOTs.Add(dot.damageOverTimeObject.DOTName + dot.SetSimultaneousIntId(), dot);
            }
            else
            {
                DOTs.Add(dot.damageOverTimeObject.DOTName, dot);
            }
            dot.StartDOT(hitboxes[0]);
            return;
        }

        switch (dot.damageOverTimeObject.dotStackMethod)
        {
            case DamageOverTime.DOTStackMethod.ExtendTime:
                activeDOT.ExtendDot(dot.damageOverTimeObject.ticks);
                return;
            case DamageOverTime.DOTStackMethod.Simultaneous:
                DOTs.Add(dot.damageOverTimeObject.DOTName + dot.SetSimultaneousIntId(), dot);
                dot.StartDOT(hitboxes[0]);
                return;
            case DamageOverTime.DOTStackMethod.ResetTimer:
                activeDOT.ResetDot(dot.damageOverTimeObject.ticks);
                return;
            case DamageOverTime.DOTStackMethod.Replace:
                activeDOT.StopDOT(true);
                DOTs[dot.damageOverTimeObject.DOTName] = dot;
                dot.StartDOT(hitboxes[0]);
                return;
            case DamageOverTime.DOTStackMethod.Ignore:
            default:
                return;
        }
    }

    /// <summary>
    /// Removes a DOT by referencing its key. This is usually its DOTName, unless DOTs are simultaneously stacked
    /// </summary>
    /// <param name="dotName">DOT string ID</param>
    public void RemoveDOT(string dotName)
    {
        DOTs.Remove(dotName);
    }

    /*
    public void RemoveStatusEffect(string effectName)
    {
        statusEffects.Remove(effectName);
    }
    */
}
