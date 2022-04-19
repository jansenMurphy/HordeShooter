using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the conversants and interacts with whatever UI or in-game thing chooses statements
/// </summary>
public class Conversation
{
    /// <summary>
    /// The people, places, or objects (Conversants) involved in a conversation.
    /// </summary>
    List<Conversant> conversants = new List<Conversant>();
    public NewDialogue uiNewDialogue = new NewDialogue();
    /// <summary>
    /// A conversation ends naturally when no conversant has responses to a statement. This event triggers and stops people from participating in a conversation; mostly useful for state updates.
    /// </summary>
    public UnityEngine.Events.UnityEvent endConversation;
    /// <summary>
    /// When the Conversation is created, it may naturally draw other Conversants in. This function determines who else is brought in to the conversation by virtue of proximity or otherwise.
    /// </summary>
    public ConversationStartEvent conversationStartEvent;

    public Conversant previousConversant;

    /// <summary>
    /// Conversations may be attached to UI elements that are able to deal with statements and objects and a number of listeners . 
    /// </summary>
    /// <param name="userInterfaceStatementListener">The Conversant who started the conversation</param>
    /// <param name="endConversationListeners">Things that need to happen at the end of the conversation</param>
    /// <param name="conversationStartEvent">Function finding who else should be in this conversation</param>
    public Conversation(UnityEngine.Events.UnityAction<Statement, List<Statement>> userInterfaceStatementListener, List<UnityEngine.Events.UnityAction> endConversationListeners, ConversationStartEvent conversationStartEvent)
    {
        endConversation = new UnityEngine.Events.UnityEvent();
        this.conversationStartEvent = conversationStartEvent;
        uiNewDialogue.AddListener(userInterfaceStatementListener);
        if (endConversationListeners != null && endConversation != null)
        {
            foreach (var item in endConversationListeners)
            {
                endConversation.AddListener(item);
            }
        }
    }

    /// <summary>
    /// Raises this Conversations ConversationStartEvent with a specific conversant
    /// </summary>
    public void GetNearbyConversants(Conversant con)
    {
        conversationStartEvent.Raise(new ConversationStartStruct(con,this));
    }
    /// <summary>
    /// This gets nearby conversants and then starts a Conversation with a specific Conversant
    /// </summary>
    public void StartConversation(Conversant con, Statement startConvo)
    {
        GetNearbyConversants(con);
        Say(startConvo);
    }
    /// <summary>
    /// Adds a conversant to the conversation, checking for duplicates
    /// </summary>
    public void JoinConversation(Conversant con)
    {
        if (!conversants.Contains(con)) conversants.Add(con);
    }

    /// <summary>
    /// A Conversant says a statement and all other conversants must add their replies.
    /// If no replies exist the conversation ends
    /// </summary>
    /// <param name="statement">Statement to trigger effects and possible responses</param>
    public void Say(Statement statement)
    {
        List<Statement> replies = new List<Statement>();
        foreach (Conversant c in conversants)
        {
            replies.AddRange(c.GetReplies(statement.SID));
        }
        if (replies.Count >= 1)
        {
            uiNewDialogue.Invoke(statement, replies);
        }
        else
        {
            if (endConversation != null) {
                Debug.Log("Had no responses to: " + statement.SID);
                endConversation.Invoke(); 
            }
        }
        previousConversant = statement.conversant;

        if (replies.Count >= 1)
        {
            replies.Sort(statement.Compare);

            if (replies[replies.Count - 1].p > 0)
            {
                replies[replies.Count-1].conversant.Say(replies[replies.Count - 1],this);
                Debug.Log("SayingNext");
            }
        }
        
    }
    [System.Serializable]
    public class NewDialogue : UnityEngine.Events.UnityEvent<Statement, List<Statement>> { }
}
