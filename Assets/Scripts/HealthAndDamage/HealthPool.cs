using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthPool:MonoBehaviour
{
    public string Name;

    private int currentHp;
    public HealthPoolInfoObject healthPoolInfoObject;
    public UnityEvent<int, int, HealthPoolInfoObject> tookDamage;

    private void Start()
    {
        if (healthPoolInfoObject.minHp >= healthPoolInfoObject.maxHp)
        {
            Debug.LogWarning("Invalid hp range, " + healthPoolInfoObject.minHp.ToString() + " " + healthPoolInfoObject.maxHp.ToString());
        }
    }

    /// <summary>
    /// Called mainly by Hitbox; alters health Pool. Negative values heal
    /// </summary>
    /// <param name="damageInfo">What damage you're dealing to this pool</param>
    /// <returns>The remainder of damage that cannot be absorbed by this pool</returns>
    public int DoDamageToPool(DamageInfo damageInfo, bool actuallyDoDamage=true)
    {
        int dummy;
        return DoDamageToPool(damageInfo, damageInfo.dmg, out dummy,actuallyDoDamage);
    }

    /// <summary>
    /// Called mainly by Hitbox; alters health Pool. Negative values heal
    /// </summary>
    /// <param name="damageInfo">What damage you're dealing to this pool</param>
    /// <param name="damageDealt">The damage dealt to the pool after vulnerability and Armor</param>
    /// <returns>The remainder of damage that cannot be absorbed by this pool</returns>
    public int DoDamageToPool(DamageInfo damageInfo,out int damageDealt, bool actuallyDoDamage = true)
    {
        return DoDamageToPool(damageInfo, damageInfo.dmg, out damageDealt, actuallyDoDamage);
    }

    /// <summary>
    /// Called mainly by Hitbox; alters health Pool. Negative values heal
    /// </summary>
    /// <param name="damageInfo">What damage you're dealing to this pool</param>
    /// <param name="damageIn">A a value that overrides the damage the pool should take from damageInfo</param>
    /// <param name="damageDealt">The damage dealt to the pool after vulnerability and Armor</param>
    /// <returns>The remainder of damage that cannot be absorbed by this pool</returns>
    public int DoDamageToPool(DamageInfo damageInfo, int damageIn, out int damageDealt, bool actuallyDoDamage = true)
    {
        float mult = 1;
        damageDealt = 0;

        if (healthPoolInfoObject.resistsAndVulns.ContainsKey(damageInfo.damageType))
        {
            if (damageIn > 0)
            {
                if (healthPoolInfoObject.resistsAndVulns[damageInfo.damageType].blockDamage) return 0;
                mult = healthPoolInfoObject.resistsAndVulns[damageInfo.damageType].damageMult;

                int modifiedDamage = (int)(damageIn * mult);
                int newHP = currentHp - modifiedDamage, overflow = 0;

                if (newHP < healthPoolInfoObject.minHp)
                {
                    overflow = healthPoolInfoObject.minHp - newHP;
                }

                damageDealt = modifiedDamage - overflow;
                if (actuallyDoDamage)
                {
                    tookDamage.Invoke(damageDealt, currentHp, healthPoolInfoObject);
                    currentHp = newHP;
                }
                if (mult != 1 && mult != 0)
                {
                    return (int)(overflow / mult);
                }
                return overflow;
            }
            else
            {
                if (healthPoolInfoObject.resistsAndVulns[damageInfo.damageType].blockHeal) return 0;
                mult = healthPoolInfoObject.resistsAndVulns[damageInfo.damageType].healmult;

                int modifiedHeal = (int)(damageIn * mult);
                int newHP = currentHp + modifiedHeal, overflow = 0;

                if (newHP > healthPoolInfoObject.maxHp)
                {
                    overflow = newHP - healthPoolInfoObject.maxHp;
                }

                damageDealt = modifiedHeal - overflow;
                if (actuallyDoDamage)
                {
                    tookDamage.Invoke(damageDealt, currentHp, healthPoolInfoObject);
                    currentHp = newHP;
                }
                if (mult != 1 && mult != 0)
                {
                    return (int)(overflow / mult);
                }
                return overflow;
            }
        }
        return damageIn;
    }

    public int GetCurrentHP()
    {
        return currentHp;
    }
}