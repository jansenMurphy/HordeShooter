using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Must implement (bool, int) CheckConditional(string getInfo)
/// Used to verify Conversant's conversation conditionals
/// The bool returns whether or not it was a valid question
/// </summary>
public abstract class ConditionalListener : MonoBehaviour {
    //This should return true if the question is irrelevant
    abstract public bool CheckConditional(Statement.Conditional conditional);
    /// <summary>
    /// Give the conditional listener a reference to the conversant through which it can yoink appropriate object data
    /// </summary>
    /// <param name="connectedObject">Object to yoink data from</param>
    abstract public void Setup(GameObject connectedObject);
}
/// <summary>
/// Must implement DoEffect(string functionCall, Conversant c, string arg1, int arg2)
/// Used to enact a statement's effects
/// </summary>
public abstract class EffectListener : MonoBehaviour
{
    /// <summary>
    /// Do things based on statements
    /// </summary>
    /// <param name="functionCall">Mutator name</param>
    /// <param name="conversant">Previous speaker</param>
    /// <param name="arg1">What thing to change</param>
    /// <param name="arg2">How much to change it by</param>
    abstract public void DoEffect(string functionCall, Conversant conversant, string arg1, int arg2);
}
