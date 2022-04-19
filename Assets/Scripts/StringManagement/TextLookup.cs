using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Text;

/// <summary>
/// This is a static container that holds a dictionary full of localized words and has the capability of saving and loading them as json "LanguageBook"
/// Main Methods are SetLanguage, LoadLanguage, and GetValue
/// </summary>
public static class TextLookup
{
    public static string language = "";
    private const string TEXT_PATTERN = @"<rt>.+</rt>", NUMBER_PATTERN = @"<rd>.+</rd>";

    static Dictionary<string, string> languageLookup = new Dictionary<string, string>();

    private static BoolGameEvent _loadLanguageEvent;

    public static void SetLoadLanguageEvent(BoolGameEvent loadLanguageEvent)
    {
        _loadLanguageEvent = loadLanguageEvent;
        Debug.Log("changed load language event");
    }

    public static bool SetLanguage(string language)
    {
        if (TextLookup.language.CompareTo(language) == 0) return true;
        TextAsset ta;
        Debug.Log(language);

        try
        {
            ta = Resources.Load<TextAsset>(System.IO.Path.Combine("Strings/" + language));
            LanguageBook lb = JsonUtility.FromJson<LanguageBook>(ta.ToString());
            for (int i = 0; i < lb.k.Count; i++)
            {
                languageLookup.Add(lb.k[i], lb.v[i]);
            }
        }
        catch
        {
            Debug.LogError($"Language {language} does not appear to exist in Strings");
            return false;
        }
        _loadLanguageEvent.Raise(true);
        return true;
    }

    public static LanguageBook LoadLanguage(string language)
    {
        TextAsset ta = Resources.Load<TextAsset>(System.IO.Path.Combine("Strings/" + language));
        if(ta != null) { 
            return JsonUtility.FromJson<LanguageBook>(ta.ToString());
        }
        else
        {
            //Debug.Log($"Did not successfully load the {language} language book");
        }
        return new LanguageBook();
    }

    public static string GetValue(string key)
    {
        string retval;
        if (languageLookup.TryGetValue(key, out retval)){
            return CheckReplacements(retval);
        }
        else
        {
            Debug.LogWarning("Could not determine string key");
            return null;
        }
    }

    private static string CheckReplacements(string stringWithTextToReplace)
    {
        StringBuilder sb = new StringBuilder();
        string[] nonMatching = Regex.Split(stringWithTextToReplace,TEXT_PATTERN);
        MatchCollection matches = Regex.Matches(stringWithTextToReplace,TEXT_PATTERN);
        sb.Append(nonMatching[0]);
        for(int i=0; matches.Count > i;i++)
        {
            string mv;
            if (languageLookup.TryGetValue(matches[i].Value.Substring(4,matches[i].Length-9), out mv))
            {
                ;
            }
            else
            {
                mv = "INVALIDTOKEN";
            }
            sb.Append(mv);
            sb.Append(nonMatching[i+1]);
        }
        /*
        nonMatching = Regex.Split(NUMBER_PATTERN, sb.ToString());
        matches = Regex.Matches(NUMBER_PATTERN, sb.ToString());
        sb.Clear();
        sb.Append(nonMatching[0]);
        for (int i = 0; matches.Count > i; i++)
        {
            string mv;
            if (languageLookup.TryGetValue(matches[i].Value.Substring(4, matches[i].Length - 9), out mv))
            {
                ;
            }
            else
            {
                mv = "INVALIDTOKEN";
            }
            sb.Append(mv);
            sb.Append(nonMatching[i + 1]);
        }
        */
        return sb.ToString();
    }


#if UNITY_EDITOR
    public static void SaveLanguage(string language, LanguageBook languageBook)
    {
        if (!System.IO.Directory.Exists("Assets/Resources"))
        {
            UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (!System.IO.Directory.Exists("Assets/Resources/Strings"))
        {
            UnityEditor.AssetDatabase.CreateFolder("Assets/Resources", "Strings");
        }

        System.IO.StreamWriter sw = new System.IO.StreamWriter("Assets/Resources/Strings/" + language + ".json");
        sw.Write(JsonUtility.ToJson(languageBook).ToString());
        sw.Close();
    }
    public static void SaveLanguage(string language, List<string> keys, List<string> values)
    {
        LanguageBook lb = new LanguageBook();
        for (int i = 0; i < keys.Count; i++)
        {
            lb.k.Add(keys[i]);
            lb.v.Add(values[i]);
        }
        SaveLanguage(language, lb);
    }
#endif

    [System.Serializable]
    public class LanguageBook
    {
        public List<string> k = new List<string>();
        public List<string> v = new List<string>();
    }
}