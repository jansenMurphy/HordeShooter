using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveAndLoad
{
    #region saving
#if UNITY_EDITOR
    public static void SaveResourceJSON (Object serializableObject, string partialPath, string fileName)
    {
        if (!ResourcePathExists(partialPath))
        {
            CreateResourcePath(partialPath);
        }
        StreamWriter sw = new System.IO.StreamWriter(Path.Combine("Assets/Resources", partialPath, fileName +".json"));
        sw.Write(JsonUtility.ToJson(serializableObject).ToString());
        sw.Close();
    }

    public static void SaveResourceXML(Object serializableObject, string partialPath, string fileName)
    {
        if (!ResourcePathExists(partialPath))
        {
            CreateResourcePath(partialPath);
        }
        FileStream fs = new FileStream(Path.Combine("Assets/Resources", partialPath, fileName + ".json"), FileMode.Create);
        System.Xml.Serialization.XmlSerializer xmls = new System.Xml.Serialization.XmlSerializer(typeof(Object));
        xmls.Serialize(fs, serializableObject);
        fs.Close();
    }
#endif

    public static void SavePersistentDataJSON(Object serializableObject, string partialPath, string fileName)
    {
        if (!PersistentPathExists(partialPath))
        {
            CreatePersistentPath(partialPath);
        }
        StreamWriter sw = new StreamWriter(Path.Combine(Application.persistentDataPath, partialPath, fileName +".json"));
        sw.Write(JsonUtility.ToJson(serializableObject).ToString());
        sw.Close();
    }

    public static void SavePersistentDataXML(Object serializableObject, string partialPath, string fileName)
    {
        if (!PersistentPathExists(partialPath))
        {
            CreatePersistentPath(partialPath);
        }
        FileStream fs = new FileStream(Path.Combine(Application.persistentDataPath, partialPath, fileName + ".json"), FileMode.Create);
        System.Xml.Serialization.XmlSerializer xmls = new System.Xml.Serialization.XmlSerializer(typeof(Object));
        xmls.Serialize(fs, serializableObject);
        fs.Close();
    }
    #endregion

    #region loading
    public static T LoadResourceJSON<T>(string path) where T : UnityEngine.Object
    {
        return JsonUtility.FromJson<T>(Resources.Load<TextAsset>(path).ToString());
    }

    public static T LoadResourceXML<T>(string path) where T : UnityEngine.Object
    {
        System.Xml.Serialization.XmlSerializer xmls = new System.Xml.Serialization.XmlSerializer(typeof(T));
        T retval;
        using (Stream reader = new FileStream(Path.Combine("Assets/Resources",path), FileMode.Open))
        {     
            retval = (T)xmls.Deserialize(reader);
        }
        return retval;
    }

    public static T LoadPersistentDataJSON<T>(string path) where T : UnityEngine.Object
    {
        return JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.persistentDataPath,path)));

    }

    public static T LoadPersistentDataXML<T>(string path) where T : UnityEngine.Object
    {
        System.Xml.Serialization.XmlSerializer xmls = new System.Xml.Serialization.XmlSerializer(typeof(T));
        T retval;
        using (Stream reader = new FileStream(Path.Combine(Application.persistentDataPath, path), FileMode.Open))
        {
            retval = (T)xmls.Deserialize(reader);
        }
        return retval;
    }
    #endregion

    #region helperFunctions


#if UNITY_EDITOR
    private static bool ResourcePathExists(string partialPath)
    {
        return File.Exists(Path.Combine("Assets/Resources", partialPath));
    }
    private static void CreateResourcePath(string path)
    {
        if (!Directory.Exists("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder("Assets/Resources");
        string[] pathParts = path.Split('/');
        for (int i = 0; i < pathParts.Length; i++)
        {
            if (!Directory.Exists(Path.Combine(sb.ToString(), pathParts[i])))
            {
                AssetDatabase.CreateFolder(sb.ToString(), pathParts[i]);
            }
            sb.Append('/');
            sb.Append(pathParts[i]);
        }
    }
#endif

    private static bool PersistentPathExists(string partialPath)
    {
        return File.Exists(Path.Combine(Application.persistentDataPath, partialPath));
    }

    public static void CreatePersistentPath(string path)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder(Application.persistentDataPath);
        string[] pathParts = path.Split('/');
        for (int i = 0; i < pathParts.Length; i++)
        {
            sb.Append('/');
            sb.Append(pathParts[i]);
            if (!Directory.Exists(sb.ToString()))
            {
                Directory.CreateDirectory(sb.ToString());
            }
        }
    }

    #endregion
}
