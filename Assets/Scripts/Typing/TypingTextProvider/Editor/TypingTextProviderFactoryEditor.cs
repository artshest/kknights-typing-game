#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;
using static UnityEngine.InputManagerEntry;

[CustomEditor(typeof(TypingTextProviderFactory))]
public class TypingTextProviderFactoryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var factory = (TypingTextProviderFactory)target;

        var type = serializedObject.FindProperty("_type");
        EditorGUILayout.PropertyField(type);

        var enumValue = (TypingTextProviderFactory.ProviderType)type.enumValueIndex;
        switch (enumValue)
        {
            case TypingTextProviderFactory.ProviderType.PredefinedText:
                
                // Почему-то если не делать GUIContent.none, то в инспекторе рисуется 2 лейбла проперти, 
                // один из которых налезает на проперти type. Так что пока убрал лейбл
                // TODO: разобраться почему так!

                EditorGUILayout.PropertyField(serializedObject.FindProperty("_predefinedText"), GUIContent.none);
                break;
            case TypingTextProviderFactory.ProviderType.PredefinedArticleURL:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_articleUrlApi"));
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}

#endif