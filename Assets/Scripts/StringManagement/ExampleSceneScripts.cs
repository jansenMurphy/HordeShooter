using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleSceneScripts : MonoBehaviour
{
    public Conversant player, MrSmiley;
    public string startConversationStatementID = "START_CONVO";
    private void Start()
    {
        player.StartConversation(MrSmiley);
    }
}
