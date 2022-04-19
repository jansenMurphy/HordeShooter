using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemConditionalListener : ConditionalListener
{
    public override bool CheckConditional(Statement.Conditional conditional)
    {
        switch (conditional.fc)
        {
            case ("IsShowingFirearm"):
                return true;//conditional.CompareCheck(simpleInventory.isShowingGun?1:0);
            default:
                return true;
        }
    }

    public override void Setup(GameObject connectedObject)
    {
        throw new System.NotImplementedException();
    }
}
