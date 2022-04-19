using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class UIDialogueManager : MonoBehaviour
{
    public Transform responseArea;
    public TextMeshProUGUI promptArea;
    public GameObject responseObject;
    public Conversation conversation;


    public void SetDialogue(Statement promptStatement, List<Statement> replies)
    {
        promptArea.text = TextLookup.GetValue(promptStatement.SID);

        ClearAllResponses();
        foreach (Statement s in replies)
        {
            var response = GameObject.Instantiate(responseObject, responseArea);
            response.GetComponent<UIDialogueResponseManager>().ShowStatement(s, conversation);
        }
        Canvas.ForceUpdateCanvases();
    }


    private void ClearAllResponses()
    {
        foreach (Transform t in responseArea)
        {
            Destroy(t.gameObject);
        }
    }
}

