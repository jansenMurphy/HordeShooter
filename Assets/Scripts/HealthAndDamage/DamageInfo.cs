using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName ="New Damage Info", menuName ="Health And Damage/Damage Info")]
public class DamageInfo : ScriptableObject
{
    public enum DamageTypes//ONLY EIGHT BITS IN THIS BITMASK?
    {
        bullet,
        needle,
        lightning,
        explosive,
        bullet_pickup,
        needle_pickup,
        lightning_pickup,
        health_pickup,
        smashing,
        magic_pickup
    }
    public int dmg,armorPiercing,impedence;
    public DamageTypes damageType;
}
