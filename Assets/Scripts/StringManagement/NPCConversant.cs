using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCConversant : Conversant
{
    public UIDialogueManager uidm;
    public string startConversationStatementID;

    public override void LeaveConversation()
    {
        throw new System.NotImplementedException();
    }

    public override void MaybeJoinConversation(ConversationStartStruct css)
    {
        css.conversation.JoinConversation(this);//Will always join the conversation
                                                //This is probably not what we want
        convo = css.conversation;

    }

    public override void StartConversation(Conversant atLeastThisOne)
    {
        convo = new Conversation(uidm.SetDialogue, null, conversationStartEvent);
        uidm.conversation = convo;
        Statement s = GetStatementById(startConversationStatementID);
        if (s != null)
        {
            convo.StartConversation(this, s);
        }
        else
        {
            Debug.LogWarning("Could not start conversation because no acceptable statement matches key.");
        }
    }
}
