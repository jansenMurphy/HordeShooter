using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDialogueResponseManager : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Button btn;
    private Conversation convo;
    Statement linkedStatement;
    private void Start()
    {
        btn.onClick.AddListener(Say);
    }
    public void ShowStatement(Statement statement, Conversation convo)
    {
        linkedStatement = statement;
        this.convo = convo;
        //Debug.Log(statement.TSK + TextLookup.GetValue(statement.TSK));
        text.text = 
            statement.conversant.GetConversantName() +
            " : " + TextLookup.GetValue(statement.TSK);
        text.color = statement.conversant.GetColor();
    }
    public void Say()
    {
        linkedStatement.conversant.Say(linkedStatement, convo);
    }
}
