using UnityEditor;
using UnityEngine;

namespace KK.Bootstrap.Editor
{
    [CustomEditor(typeof(TypingTextProviderFactory))]
    public class TypingTextProviderFactoryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_textLibrary"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_fragmentOptions"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_timeoutSeconds"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Отладка", EditorStyles.boldLabel);
            var useDebugText = serializedObject.FindProperty("_useDebugText");
            EditorGUILayout.PropertyField(useDebugText);

            if (useDebugText.boolValue)
            {
                // Почему-то если не делать GUIContent.none, то в инспекторе рисуется 2 лейбла проперти,
                // один из которых налезает на соседнее поле. Так что пока убрал лейбл
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_debugText"), GUIContent.none);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
