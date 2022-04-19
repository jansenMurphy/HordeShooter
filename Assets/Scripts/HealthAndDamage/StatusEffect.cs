using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect
{
    public StatusEffectInfoObject statusEffectInfoObject;

    private IEnumerator seTicker;
    private Hitbox connectedBox;
    private int currentTick;
    private int simultaneousID;

    public enum StatusEffectStackMethod
    {
        ExtendTime,
        Simultaneous,
        ResetTimer,
        Replace,
        Ignore
    }

    public int SetSimultaneousIntId()
    {
        simultaneousID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        return simultaneousID;
    }

    public void Startse(Hitbox hitbox)
    {
        connectedBox = hitbox;
        //seTicker = SETicker(); TODO FIX status effects
    }

    public void Stopse(bool disconnect = false)
    {
        currentTick = statusEffectInfoObject.ticks;
        if (disconnect) connectedBox = null;
    }

    public void Extendse(int tick)
    {
        currentTick -= tick;
    }
    public void Resetse(int tick)
    {
        seTicker.Reset();
    }

    /*
    private IEnumerator SETicker()
    {
        currentTick = 0;
        while (currentTick <= statusEffectInfoObject.tickDelayBeforeStart)
        {
            yield return new WaitForSeconds(statusEffectInfoObject.msPerTick);
        }
        while (currentTick <= statusEffectInfoObject.ticks)
        {
            connectedBox.DoDamageToHitbox(statusEffectInfoObject.di);
            yield return new WaitForSeconds(statusEffectInfoObject.msPerTick);
        }

        if (statusEffectInfoObject.removeStatusEffectWhenFinished)
        {
            if (connectedBox != null)
            {
                if (statusEffectInfoObject.rvStackMethod == StatusEffectStackMethod.Simultaneous)
                {
                    connectedBox.RemoveStatusEffect(statusEffectInfoObject.statusEffectName + simultaneousID);
                }
                else
                {
                    connectedBox.RemoveStatusEffect(statusEffectInfoObject.statusEffectName);
                }
            }
        }
    }
    */
}