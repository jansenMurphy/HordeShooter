using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Info for a Damage Over Time script to work with
/// </summary>
[CreateAssetMenu(fileName = "New DOT", menuName = "Health And Damage/DOT")]
public class DamageOverTimeObject : ScriptableObject
{
    public readonly string DOTName;
    public DamageInfo di;
    public int ticks, tickDelayBeforeStart, msPerTick;
    public DamageOverTime.DOTStackMethod dotStackMethod;
    public bool removeDOTWhenFinished;
}