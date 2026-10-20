#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace KOOPEditor.Extensions
{
    public static class SerializedObjectExtensions
    {
        private const string _scriptPropertyName = "m_Script";

        public static void DrawScriptField(this SerializedObject target)
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.PropertyField(target.FindProperty(_scriptPropertyName));
            }
        }
    }
}

#endif