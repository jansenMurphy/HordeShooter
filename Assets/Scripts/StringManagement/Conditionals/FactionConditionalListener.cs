using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionConditionalListener : ConditionalListener
{
    public override bool CheckConditional(Statement.Conditional conditional)
    {
        switch (conditional.fc)
        {
            default:
                return true;
        }
    }

    public override void Setup(GameObject connectedObject)
    {
        throw new System.NotImplementedException();
    }
}
