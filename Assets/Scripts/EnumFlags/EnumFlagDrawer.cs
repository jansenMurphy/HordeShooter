using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(EnumFlagAttribute))]
public class EnumFlagDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EnumFlagAttribute flagSettings = (EnumFlagAttribute)attribute;
        Enum targetEnum = (Enum)Enum.ToObject(fieldInfo.FieldType, property.intValue);

        string propName = flagSettings.name;

        if (string.IsNullOrEmpty(propName)) propName = ObjectNames.NicifyVariableName(property.name);

        EditorGUI.BeginProperty(position, label, property);
        Enum enumNew = EditorGUI.EnumFlagsField(position, propName, targetEnum);
        property.intValue = (int)Convert.ChangeType(enumNew, targetEnum.GetType());
        EditorGUI.EndProperty();
    }
}
