using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private void Start()
    {

#if UNITY_SERVER
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
#endif

        GameServer.Server.Start(16,28002);
    }

    private void OnApplicationQuit()
    {
        GameServer.Server.Stop();
    }
}
