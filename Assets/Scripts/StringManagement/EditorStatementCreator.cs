#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorStatementCreator : EditorWindow
{
    private static readonly GUIContent titleText = new GUIContent("Statement Creator");

    string statementID = "StatementID", animationTrigger = "", textStringKey = "TextStringKey", conversantDirectory = "UNKNOWN";
    int triggerStatementCount = 1;
    List<string> triggerStatementIDs = new List<string>();
    int priority;


    int conditionalCount = 0, effectCount = 0;
    List<Statement.Conditional> conditions = new List<Statement.Conditional>();
    List<Statement.Effect> effects = new List<Statement.Effect>();
    public static readonly string[] comparisonEnums = { "<", "<=", "==", "!=", ">=", ">" };

    #region Oldcode
    /*
    [System.Serializable]
    class ConditionStct
    {
        public ConditionStct(int Item1, string Item2, int Item3) {
            this.Item1 = Item1;
            this.Item2 = Item2;
            this.Item3 = Item3;
        }
        public int Item1;
        public string Item2;
        public int Item3;

        public static implicit operator Statement.Conditional (ConditionStct cs)
        {
            return new Statement.Conditional(cs.Item1, cs.Item2, cs.Item3);
        }

        public static Statement.Conditional Converts (ConditionStct cs)
        {
            return new Statement.Conditional(cs.Item1, cs.Item2, cs.Item3);
        }
    }
    [System.Serializable]
    class EffectStct
    {
        public EffectStct(string Item1, string Item2, int Item3)
        {
            this.Item1 = Item1;
            this.Item2 = Item2;
            this.Item3 = Item3;
        }
        public string Item1;
        public string Item2;
        public int Item3;

        public static implicit operator Statement.Effect(EffectStct es)
        {
            return new Statement.Effect(es.Item1, es.Item2, es.Item3);
        }
        public static Statement.Effect Converts (EffectStct es)
        {
            return new Statement.Effect(es.Item1, es.Item2, es.Item3);
        }

        public static Statement.Effect[] Convert(List<EffectStct> es)
        {
            var retVal = new Statement.Effect[es.Count];
            for (int i = 0; i < es.Count; i++)
            {
                retVal[i] = es[i];
            }
            return retVal;
        }
    }
    */
    #endregion

    [MenuItem("Window/CustomConversation/StatementCreator")]
    static void Init()
    {
        Debug.Log("Open window");
        EditorStatementCreator window = (EditorStatementCreator)EditorWindow.GetWindow(typeof(EditorStatementCreator));
        window.titleContent = titleText;
        window.Show();
    }

    private void OnGUI()
    {
        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
        if(Event.current.type == EventType.DragExited)
        {
            Object[] objs = DragAndDrop.objectReferences;
            if(objs.Length == 1)
            {
                try
                {
                    Statement s =  StatementReader.ParseStatement(objs[0] as TextAsset);
                    if(s != null)
                    {
                        int lastindex=0, secondtolastindex=0, tmp;
                        while (true)
                        {
                            tmp = DragAndDrop.paths[0].IndexOf('/', lastindex + 1);
                            if (tmp == -1) break;
                            secondtolastindex = lastindex;
                            lastindex = tmp;
                            //Debug.Log(lastindex + " " + secondtolastindex);
                        }

                        //Debug.Log(lastindex + " " + secondtolastindex);

                        conversantDirectory = DragAndDrop.paths[0].Substring(secondtolastindex+1,lastindex-secondtolastindex-1);
                        statementID = s.SID;
                        animationTrigger = s.AT;
                        textStringKey = s.TSK;
                        priority = s.p;
                        triggerStatementCount = s.TID.Length;
                        triggerStatementIDs = new List<string>( s.TID );
                        conditionalCount = s.c.Length;
                        conditions = new List<Statement.Conditional>(s.c);
                        effectCount = s.e.Length;
                        effects = new List<Statement.Effect>(s.e);
                    }
                }
                catch
                {
                    Debug.LogWarning("The dragged file is invalid");
                }
            }
            else
            {
                Debug.LogWarning("The dragged file is invalid");
            }
        }

        conversantDirectory = EditorGUILayout.TextField("Conversant Directory", conversantDirectory);
        statementID = EditorGUILayout.TextField("Statement ID", statementID);
        animationTrigger = EditorGUILayout.TextField("Animation Trigger", animationTrigger);
        textStringKey = EditorGUILayout.TextField("Text String Key", textStringKey);
        priority = EditorGUILayout.DelayedIntField("Priority", priority);

        EditorGUILayout.Space();
        triggerStatementCount = EditorGUILayout.DelayedIntField("Triggering Statements", triggerStatementCount);
        while (triggerStatementCount > triggerStatementIDs.Count)
        {
            triggerStatementIDs.Add("");
        }

        if (triggerStatementCount < triggerStatementIDs.Count)
        {
            triggerStatementIDs.RemoveRange(triggerStatementCount, triggerStatementIDs.Count - triggerStatementCount);
        }

        GUILayout.Label("Trigger Statements");
        for (int i = 0; i < triggerStatementCount; i++)
        {
            triggerStatementIDs[i] = EditorGUILayout.TextField($"Trigger{i}", triggerStatementIDs[i]);
        }

        EditorGUILayout.Space();
        conditionalCount = EditorGUILayout.DelayedIntField("Conditional Statement Count", conditionalCount);
        while (conditionalCount > conditions.Count)
        {
            conditions.Add(new Statement.Conditional(0, "", 0));
        }

        if (conditionalCount < conditions.Count)
        {
            conditions.RemoveRange(conditionalCount, conditions.Count - conditionalCount);
        }

        GUILayout.Label("Conditionals");
        for (int i = 0; i < conditionalCount; i++)
        {
            GUILayout.Label($"Conditional {i}");

            EditorGUILayout.BeginHorizontal();
            conditions[i].fc = EditorGUILayout.TextField("Attribute Call", conditions[i].fc);
            conditions[i].enm = EditorGUILayout.Popup(conditions[i].enm, comparisonEnums, GUILayout.MaxWidth(40f));
            conditions[i].cv = EditorGUILayout.IntField(conditions[i].cv);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        effectCount = EditorGUILayout.DelayedIntField("Effect Statement Count", effectCount);
        while (effectCount > effects.Count)
        {
            effects.Add(new Statement.Effect("", "", 0));
        }

        if (conditionalCount < conditions.Count)
        {
            effects.RemoveRange(effectCount, effects.Count - effectCount);
        }

        GUILayout.Label("Effects");
        for (int i = 0; i < effectCount; i++)
        {
            GUILayout.Label($"Effect {i}");

            EditorGUILayout.BeginHorizontal();
            effects[i].fc = EditorGUILayout.TextField("Function", effects[i].fc);
            effects[i].s = EditorGUILayout.TextField("String Argument", effects[i].s);
            effects[i].i = EditorGUILayout.IntField("Quantity", effects[i].i);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Save Statement"))
        {
            SaveStatement();
        }
    }

    private void SaveStatement()
    {
        if (conversantDirectory == null)
        {
            EditorUtility.DisplayDialog("Error", "We could not make the Statement as requested because " +
                "no directory was chosen", "Noted");
        }
        StatementReader.WriteStatement(conversantDirectory, new Statement(statementID, animationTrigger, textStringKey, triggerStatementIDs.GetRange(0,triggerStatementCount).ToArray(), priority, conditions.ToArray(), effects.GetRange(0,effectCount).ToArray()));
    }
}
#endif