using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("Could not initialize");
        }
        string name = Steamworks.SteamFriends.GetPersonaName();
        Debug.Log(name);
    }
}
