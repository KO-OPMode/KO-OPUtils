#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using KOOP;
using KOOPEditor.Extensions;

namespace KOOPEditor
{
    [CustomEditor(typeof(KeyboundEvent))]
    public class KeyboundEventEditor : Editor
    {
        private SerializedProperty Key;
            
        private void OnEnable()
        {
            Key = serializedObject.FindProperty(nameof(Key));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            
            serializedObject.DrawScriptField();

            if (Key.enumValueIndex == (int)KeyCode.None)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Keybound Event requires a key! Otherwise this event will never fire :(", MessageType.Warning);
            }

            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();                                                                                                                                                                             
        }
    }
}

#endif