using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadLanguages : MonoBehaviour
{

    [SerializeField] private BoolGameEvent loadLanguageEvent;

    void Awake()
    {
        TextLookup.SetLoadLanguageEvent(loadLanguageEvent);
        DontDestroyOnLoad(gameObject);
        Debug.Log(TextLookup.SetLanguage("en")?"Succsessfully loaded ENG":"Failed to load ENG");
    }
}
