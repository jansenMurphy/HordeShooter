using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New HealthPoolInfoObject",menuName = "Health And Damage/HealthPoolInfoObject")]
public class HealthPoolInfoObject : ScriptableObject
{
    public int minHp = 0, maxHp = 200, startingHp;
    [SerializeField]
    private ResistVulnSet resistsAndVulnsList;
    public Dictionary<DamageInfo.DamageTypes, ResistanceVulnerability> resistsAndVulns = new Dictionary<DamageInfo.DamageTypes, ResistanceVulnerability>();
}
