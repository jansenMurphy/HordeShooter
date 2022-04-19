using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Conversants get and hold hold their possible statements. The callback "say" is for UI or in-game elemnts that have an attached statement
/// </summary>
[RequireComponent(typeof(ConversationStartEventListener))]
public abstract class Conversant : MonoBehaviour
{
    public ConversationStartEvent conversationStartEvent;
    Dictionary<string, List<Statement>> statements = new Dictionary<string, List<Statement>>();//Everything this conversant can say by triggering statement ids
    private bool hasGottenStatements = false;//Has this Conversant yoinked all of the statements it can say
    [SerializeField] protected string[] statementDirectories;//File paths to find all statements
    [SerializeField] Color conversantColor = Color.black;//What color this conversant shows up as in text dialogue windows. May be overridden
    bool autoChoosesHighestPriority = false;//Whether or not this conversant will automatically choose an option or not. NPCs and NP objects auto-choose
    public ConditionalListener[] conditionalListeners;//Things that determine if a statement should be presented as an option or not
    public EffectListener[] effectListeners;//Things that may change a player or object's state given 
    public string conversantName;//The person/object's name
    protected Conversation convo;
    public UnityEventString animationEvent;

    protected void Start()
    {
        ReadInStatements();
    }

    public Color GetColor()
    {
        return conversantColor;
    }
    public string GetConversantName()
    {
        return conversantName;
    }
    /// <summary>
    /// Figure out whether a conversant should join a conversation. This works with the ConversationStartEvent
    /// </summary>
    /// <param name="con">Conversant who is attempting to add this to the conversation</param>
    /// <param name="convo">Conversation to possibly join</param>
    public abstract void MaybeJoinConversation(ConversationStartStruct css);

    /// <summary>
    /// Starts a Conversation with another Conversant.
    /// </summary>
    public abstract void StartConversation(Conversant otherConversant);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="triggerKey"></param>
    /// <returns>All valid responses to a given trigger string</returns>
    public virtual List<Statement> GetReplies(string triggerKey)
    {
        if (!hasGottenStatements)
        {
            ReadInStatements();
        }
        List<Statement> possibleResponses = new List<Statement>();
        List<Statement> retval = new List<Statement>();
        if (statements.TryGetValue(triggerKey, out possibleResponses))
        {
            bool works = true;
            foreach (Statement s in possibleResponses)
            {
                works = true;
                foreach (Statement.Conditional conditional in s.c)
                {
                    foreach (ConditionalListener listener in s.conversant.conditionalListeners)
                    {
                        works = listener.CheckConditional(conditional);
                        if (!works) break;
                    }
                    if (!works) break;
                }
                if (works)
                {
                    retval.Add(s);
                }
            }
        }
        return retval;
    }

    protected virtual void ReadInStatements()
    {
        hasGottenStatements = true;
        foreach (var direct in statementDirectories)
        {
            Statement[] statementArray = StatementReader.ReadStatements(direct, this);
            foreach (Statement s in statementArray)
            {
                s.conversant = this;
                foreach (string trigger in s.TID)
                {
                    if (statements.ContainsKey(trigger))
                    {
                        statements[trigger].Add(s);
                    }
                    else
                    {
                        statements.Add(trigger, new List<Statement>(new Statement[] { s }));
                    }
                }
                if(s.TID.Length == 0)
                {
                    statements.Add("", new List<Statement>(new Statement[] { s }));
                }
            }
        }
        if(statementDirectories.Count() == 0)
        {
            Debug.LogWarning("There were no directories listed with this character");
        }
        Debug.Log("There are " + statements.Count() + " statements");
    }

    public virtual void Say(Statement statement, Conversation convo)
    {
        DoEffects(statement);
        if (animationEvent.GetPersistentEventCount() > 1)
        {
            animationEvent.Invoke(statement.AT);
        }
        convo.Say(statement);
    }

    public virtual void DoEffects(Statement statementWithEffects)
    {
        foreach (var item in effectListeners)
        {
            foreach (var item2 in statementWithEffects.e)
            {
                item.DoEffect(item2.fc, convo.previousConversant, item2.s, item2.i);
            }
        }
    }
    public virtual void DoEffect(Statement.Effect effect)
    {
        foreach (var item in effectListeners)
        {
            item.DoEffect(effect.fc, convo.previousConversant, effect.s, effect.i);
        }
    }

    public virtual void DoSingleEffect(string fcall, string arg1, int arg2)
    {
        foreach (var item in effectListeners)
        {
            item.DoEffect(fcall, convo.previousConversant, arg1, arg2);
        }
    }

    /// <summary>
    /// Please don't use this frequently; it's hella inefficient
    /// </summary>
    /// <returns></returns>
    public Statement GetStatementById(string statementId)
    {
        foreach (List<Statement> listOfStatements in statements.Values)
        {
            foreach (Statement s in listOfStatements)
            {
                if(s.SID.Equals(statementId)) return s;
            }
        }
        Debug.LogWarning("Could not find statement with id: " + statementId);
        return null;
    }

    public abstract void LeaveConversation();
}

public class UnityEventString : UnityEngine.Events.UnityEvent<string> { }