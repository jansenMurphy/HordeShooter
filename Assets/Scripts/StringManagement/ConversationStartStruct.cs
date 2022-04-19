using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ConversationStartStruct
{
    public Conversant conversant;
    public Conversation conversation;
    public ConversationStartStruct(Conversant conversant, Conversation conversation)
    {
        this.conversant = conversant;
        this.conversation = conversation;
    }
}
