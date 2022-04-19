using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// Reads and writes statements to Resources folder.
/// </summary>
public class StatementReader
{
    public static Statement[] ReadStatements(string directoryName, Conversant c)
    {
        TextAsset[] statementTxts;
        try
        {
            statementTxts = Resources.LoadAll<TextAsset>("Conversations/" + directoryName + "/");
            if(statementTxts.Length == 0)
            {
                Debug.LogError("There are no statements in this folder");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Couldn't get statements resource. Folder likely doesn't exist", e.ToString());
            return null;
        }
        return GetSFromTAs(statementTxts, c);
    }


    private static Statement[] GetSFromTAs(TextAsset[] statementTxts, Conversant c)
    {
        Statement[] retvals = new Statement[statementTxts.Length];
        try
        {
            for (int i = 0; i < retvals.Length; i++)
            {
                Statement s = JsonUtility.FromJson<Statement>(statementTxts[i].ToString());
                s.conversant = c;
                retvals[i] = s;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Could not convert Json object to Statement", e);
            return retvals;
        }
        return retvals;
    }
    public static Statement ParseStatement(TextAsset statementTxt, Conversant c)
    {
        try
        {
            Statement retval = JsonUtility.FromJson<Statement>(statementTxt.ToString());
            retval.conversant = c;
            return retval;
        }
        catch(Exception e)
        {
            Console.WriteLine("Could not convert Json object to Statement", e);
            return null;
        }
    }
    public static Statement ParseStatement(TextAsset statementTxt)
    {
        try
        {
            return JsonUtility.FromJson<Statement>(statementTxt.ToString());
        }
        catch (Exception e)
        {
            Console.WriteLine("Could not convert Json object to Statement", e);
            return null;
        }
    }

    private static void FakeCall(string directoryName, Conversant c, Action<Statement[]> callback)
    {
        return;
    }

    /*
    public static string GetString(string stringRef, string language, Action<string> callback)
    {

    }

    public static string GetString(string[] keys, Action<string> callback)
    {

    }
    */

    private IEnumerator ReadString(string[] keys, Action<string> callback)
    {
        yield return new WaitForSeconds(1);
    }


    private static IEnumerator ReadStatements(string directoryName, Conversant c, Action<Statement[]> callback)
    {
        var taHandler = Addressables.LoadAssetsAsync<TextAsset>(System.IO.Path.Combine("Assets_moved/Conversations",directoryName), FakeCall);
        yield return taHandler;
        if(taHandler.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            callback.Invoke(GetSFromTAs((TextAsset[])taHandler.Result,c));
        }
        else
        {
            Debug.LogError("Was unable to properly async load text assets");
        }
        Addressables.Release(taHandler);
    }

    private static void FakeCall(TextAsset ta)
    {

    }

#if UNITY_EDITOR
    public static void WriteStatement(string directoryName, Statement s)
    {
        if (!System.IO.Directory.Exists("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        if(!System.IO.Directory.Exists("Assets/Resources/Conversations"))
        {
            AssetDatabase.CreateFolder("Assets/Resources","Conversations");
        }

        if (!System.IO.Directory.Exists("Assets/Resources/Conversations/" + directoryName))
        {
            AssetDatabase.CreateFolder("Assets/Resources/Conversations", directoryName);
        }

        System.IO.StreamWriter sw = new System.IO.StreamWriter("Assets/Resources/Conversations/" + directoryName + "/" + s.SID + ".json");
        sw.Write(JsonUtility.ToJson(s).ToString());
        sw.Close();
    }
#endif
}
