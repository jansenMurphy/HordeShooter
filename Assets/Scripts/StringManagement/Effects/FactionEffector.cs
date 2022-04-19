using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionEffector : EffectListener
{
    public override void DoEffect(string functionCall, Conversant conversant, string arg1, int arg2)
    {
        switch (functionCall)
        {
            case "ChangeFactionAttribute":
                switch (arg1)
                {
                    case ("Affability"):
                        //simpleFaction.affability += arg2;
                        return;
                    default:
                        Debug.LogWarning("Could not find desired value to modify");
                        return;
                }
            default:
                return;
        }
    }
}
