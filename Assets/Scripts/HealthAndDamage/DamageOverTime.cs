using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Maintains DOTs and HOTS that are attached to Hitboxes
/// </summary>
[System.Serializable]
public class DamageOverTime
{
    public DamageOverTimeObject damageOverTimeObject;

    protected IEnumerator dotTicker;
    private Hitbox connectedBox;
    private int currentTick;
    private int simultaneousID;

    public enum DOTStackMethod
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

    public void StartDOT(Hitbox hitbox)
    {
        connectedBox = hitbox;
        dotTicker = DOTTicker();
    }

    public void StopDOT(bool disconnect=false)
    {
        currentTick = damageOverTimeObject.ticks;
        if(disconnect) connectedBox = null;
    }

    public void ExtendDot(int tick)
    {
        currentTick -= tick;
    }
    public void ResetDot(int tick)
    {
        dotTicker.Reset();
    }

    protected IEnumerator DOTTicker()
    {
        currentTick = 0;
        while(currentTick <= damageOverTimeObject.tickDelayBeforeStart)
        {
            yield return new WaitForSeconds(damageOverTimeObject.msPerTick);
        }
        while(currentTick <= damageOverTimeObject.ticks)
        {
            connectedBox.DoDamageToHitbox(damageOverTimeObject.di);
            yield return new WaitForSeconds(damageOverTimeObject.msPerTick);
        }

        if (damageOverTimeObject.removeDOTWhenFinished)
        {
            if (connectedBox != null)
            {
                if(damageOverTimeObject.dotStackMethod == DOTStackMethod.Simultaneous)
                {
                    connectedBox.RemoveDOT(damageOverTimeObject.DOTName+simultaneousID);
                }
                else
                {
                    connectedBox.RemoveDOT(damageOverTimeObject.DOTName);
                }
            }
        }
    }
}
