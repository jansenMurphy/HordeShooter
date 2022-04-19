using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterConversant : Conversant
{
    public UIDialogueManager uidm;
    public string startConversationStatementID;

    public override void LeaveConversation()
    {
        Debug.Log("Left Conversation");
    }

    public override void MaybeJoinConversation(ConversationStartStruct css)
    {
        css.conversation.JoinConversation(this);//Will always join the conversation
        convo = css.conversation;
        //This is probably not what we want
    }

    public override void StartConversation(Conversant atLeastThisOne)
    {
        convo = new Conversation(uidm.SetDialogue,new List<UnityAction>() { LeaveConversation }, conversationStartEvent);
        uidm.conversation = convo;
        Statement s = GetStatementById(startConversationStatementID);
        if (s != null) {
            convo.StartConversation(this, s);
        }
        else
        {
            Debug.LogWarning("Could not start conversation because no acceptable statement matches key.");
        }
    }
}
