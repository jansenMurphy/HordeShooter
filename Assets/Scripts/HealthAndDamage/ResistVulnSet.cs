using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Info for a Damage Over Time script to work with
/// </summary>
[CreateAssetMenu(fileName = "New ResistVulnSet", menuName = "Health And Damage/ResistVulnSet")]
public class ResistVulnSet : ScriptableObject
{
    public List<ResistanceVulnerability> resistanceVulnerabilities;
}
