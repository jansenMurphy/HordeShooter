using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Statements are serializable objects that allow for dynamic conversations. StatementID is unique to each statement. When a statement is queried, each conditional is checked on the Conversant that has the statement. If all conditionals return true then the UI or conversation should show the string referred to by the teststringkey.
/// </summary>
[System.Serializable]
public class Statement : IComparer<Statement>
{
    //Because these are going to be serialized as JSON, they need to be short and hence unreadable
    /// <summary>
    /// Statement ID that lets a statement be called or addressed
    /// </summary>
    public string SID;
    /// <summary>
    /// Animation Trigger that the Conversant may use to react to something.
    /// </summary>
    public string AT;
    /// <summary>
    /// TextString Key that references a value in a dictionary, determined by language.
    /// </summary>
    //A Text String key that references a specific line of text in a dictionary. This isn't just the text because multiple statements may need to reference the same text and
    public string TSK;
    /// <summary>
    /// Trigger Statement IDs: The possible statements that may trigger this as a response
    /// </summary>
    public string[] TID;
    /// <summary>
    /// Statement's priority: The highest priority statement is automatically chosen by NPCs as a dialogue response
    /// </summary>
    public int p;
    /// <summary>
    /// Statement's conditionals: What things have to be true in order for the Statement to be a valid response
    /// </summary>
    public Conditional[] c;
    /// <summary>
    /// Statmenent's effects: What things happen to the character after this statement is triggered
    /// </summary>
    public Effect[] e;
    /// <summary>
    /// Statement's speaking conversant: This is not saved with the statement; it's just called with the person
    /// </summary>
    public Conversant conversant;

    public Statement(string statementID, string animationTrigger, string textStringKey, string[] triggerStatementIDs, int priority, Conditional[] conditionals, Effect[] effects)
    {
        this.SID = statementID;
        this.AT = animationTrigger;
        this.TSK = textStringKey;
        this.TID = triggerStatementIDs;
        this.p = priority;
        this.c = conditionals;
        this.e = effects;
    }

    [System.Serializable]
    public class Effect
    {
        /// <summary>
        /// Function Call
        /// </summary>
        public string fc;
        /// <summary>
        /// Effect's function call's argument 1 that is a string
        /// </summary>
        public string s;
        /// <summary>
        /// Effect's function call's argument 2 that is an int
        /// </summary>
        public int i;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fcall">The string of the function to call</param>
        /// <param name="arg1">Argument 1 of the function, usually a variable name</param>
        /// <param name="arg2">Argument 2 of the function, usually an amount</param>
        public Effect(string fcall, string arg1, int arg2)
        {
            this.fc = fcall;
            this.s = arg1;
            this.i = arg2;
        }
    }
    [System.Serializable]
    public class Conditional
    {
        public enum Comparison { Less, LessEqual, Equal, NotEqual, GreaterEqual, Greater };

        /// <summary>
        /// Comparison Enum < <= == != >= >
        /// </summary>
        public int enm;//The Comparison enum
        /// <summary>
        /// The information getter funcion call
        /// </summary>
        public string fc;
        /// <summary>
        /// The comparison value the fc value is compared against 
        /// </summary>
        public int cv;//Comparison value

        public Conditional(int comparisonEnum, string getInfoCall, int compValue)
        {
            this.enm = comparisonEnum;
            this.fc = getInfoCall;
            this.cv = compValue;
        }

        public bool CompareCheck(int val)
        {
            switch ((Comparison)enm)
            {
                case Comparison.Less:
                    return val < cv;
                case Comparison.LessEqual:
                    return val <= cv;
                case Comparison.Equal:
                    return val == cv;
                case Comparison.NotEqual:
                    return val != cv;
                case Comparison.GreaterEqual:
                    return val >= cv;
                case Comparison.Greater:
                    return val > cv;
                default:
                    Debug.LogWarning("Cannot properly compare");
                    return false;
            }
        }
    }
    public int Compare(Statement x, Statement y)
    {
        if(x.p < y.p)
        {
            return -1;
        }else if (x.p > y.p)
        {
            return 1;
        }
        if(x.c.Length < y.c.Length)
        {
            return 1;
        }else if(x.c.Length < y.c.Length)
        {
            return -1;
        }
        if (x.e.Length < y.e.Length)
        {
            return 1;
        }
        else if (x.e.Length < y.e.Length)
        {
            return -1;
        }
        return 0;
    }
}