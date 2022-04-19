using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpdateUIText : MonoBehaviour
{
    TextMeshProUGUI text;
    public string key;
    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        LookForUpdates();
    }

    public void LookForUpdates(bool b)
    {
        text.text = TextLookup.GetValue(key) ?? text.text;
    }
    public void LookForUpdates()
    {
        text.text = TextLookup.GetValue(key) ?? text.text;
    }
}
