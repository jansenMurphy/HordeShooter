#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorStringCreator : EditorWindow
{
    private static readonly GUIContent titleText = new GUIContent("String Editor");

    List<string> languages = new List<string>() { "KEYS" };
    private string[] languageList;
    List<List<string>> strings = new List<List<string>>() { new List<string>() };
    Vector2 scrollPos = Vector2.zero;

    [MenuItem("Window/CustomConversation/StringEditor")]
    static void Init()
    {
        EditorStringCreator window = (EditorStringCreator)EditorWindow.GetWindow(typeof(EditorStringCreator));
        window.titleContent = titleText;

        GetLanguageList(window);
        LoadJSON(window);

        window.Show();
    }
    private void OnGUI()
    {
        if (GUILayout.Button("Add a language"))
        {
            GenericMenu languageMenu = new GenericMenu();
            for(int i=0; i < languageList.Length; i++)
            {
                languageMenu.AddItem(new GUIContent(languageList[i]), false, OnAddLanguage, languageList[i]);
            }
            languageMenu.ShowAsContext();
        }
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < languages.Count; i++)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(languages[i]);
            for (int j = 0; j < strings[0].Count; j++)
            {
                strings[i][j] = EditorGUILayout.TextArea(strings[i][j],GUILayout.Height(50f));
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Add Key")){
            OnAddKey();
        }
        if(GUILayout.Button("Save All"))
        {
            Save();
        }

        EditorGUILayout.EndHorizontal();
    }

    private static void GetLanguageList(EditorStringCreator esc)
    {
        var cultures = System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.AllCultures);
        esc.languageList = new string[cultures.Length];
        for (int i = 0; i < cultures.Length; i++)
        {
            esc.languageList[i] = cultures[i].Name;
        }
    }

    private void OnAddLanguage(object lang)
    {
        if (!languages.Contains((string)lang))
        {
            languages.Add((string)lang);
            strings.Add(new List<string>());
            for (int i = 0, index=strings.Count-1; i < strings[0].Count; i++)
            {
                strings[index].Add("");
            }
        }
        else
        {
            Debug.Log("You already have that language");
        }
    }
    private void OnAddKey()
    {
        for (int i = 0; i < languages.Count; i++)
        {
            strings[i].Add("");
            scrollPos = new Vector2(0, Mathf.Infinity);
        }
    }
    private void Save()
    {
        for (int j = 0; j < strings[0].Count; j++)
        {
            bool works = false;
            for (int i = 1; i < languages.Count; i++)
            {
                if (!strings[i][j].Equals(""))
                {
                    works = true;
                }
            }
            if (!works)
            {
                for (int i = 0; i < languages.Count; i++)
                {
                    strings[i].RemoveAt(j);
                }
                j--;
            }
        }

        for (int i = 1; i < languages.Count; i++)
        {
            TextLookup.SaveLanguage(languages[i], strings[0], strings[i]);
        }
    }

    private static void LoadJSON(EditorStringCreator esc) {
        bool assignedKeys = false;
        int knownLanguages = 0;
        foreach (string item in esc.languageList)
        {
            var lb = TextLookup.LoadLanguage(item);
            if (lb.v.Count == 0) continue;
            esc.languages.Add(item);
            esc.strings.Add(new List<string>());
            knownLanguages++;
            for (int i = 0; i < lb.v.Count; i++)
            {
                esc.strings[knownLanguages].Add(lb.v[i]);
            }
            if (!assignedKeys)
            {
                for (int i = 0; i < lb.k.Count; i++)
                {
                    esc.strings[0].Add(lb.k[i]);
                }
                assignedKeys = true;
            }
        }
    }

}
#endif