using UnityEditor;
using UnityEngine;

namespace KK.Bootstrap.Editor
{
    [CustomEditor(typeof(TypingTextProviderFactory))]
    public class TypingTextProviderFactoryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
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
                case TypingTextProviderFactory.ProviderType.PredefinedArticleUrl:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_articleLink"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_fragmentOptions"), true);
                    break;
                case TypingTextProviderFactory.ProviderType.Library:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_textLibrary"));
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
