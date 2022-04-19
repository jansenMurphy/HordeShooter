using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEffector : EffectListener
{
    public override void DoEffect(string functionCall, Conversant previousStatementSpeaker, string arg1, int arg2)
    {
        switch (functionCall) {
            case "GiveItem":
                switch (arg1)
                {
                    case ("yen"):
                        Debug.Log("Gave yen");
                        //simpleInventory.yen += arg2;
                        return;
                    default:
                        Debug.LogWarning("Could not find desired item to modify");
                        return;
                }
            case "EquipItem":
                switch (arg1)
                {
                    case ("gun"):
                        //simpleInventory.isShowingGun = true;
                        return;
                    default:
                        Debug.LogWarning("Could not find desired item to modify");
                        return;
                }
            case "GiveAskerItem":
                previousStatementSpeaker.DoSingleEffect("GiveItem", arg1, arg2);
                    return;
            default:
                return;
            }
    }
}
