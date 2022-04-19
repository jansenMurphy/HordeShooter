using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestUI : MonoBehaviour
{
    public Text displayText;

    public void UpdateHP(int newNumber,HealthPool healthPool)
    {
        displayText.text = healthPool.GetCurrentHP().ToString();
    }
}
