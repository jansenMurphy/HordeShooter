using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Info for a Damage Over Time script to work with
/// </summary>
[CreateAssetMenu(fileName = "New StatusEffect", menuName = "Health And Damage/StatusEffect")]
public class StatusEffectInfoObject : ScriptableObject
{
    public readonly string statusEffectName;
    public ResistanceVulnerability rv;
    public int ticks, tickDelayBeforeStart, msPerTick;
    public StatusEffect.StatusEffectStackMethod rvStackMethod;
    public bool removeStatusEffectWhenFinished;
}