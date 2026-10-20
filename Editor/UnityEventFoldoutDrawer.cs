
#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

using UnityEditorInternal;

namespace KOOPEditor
{
    [CustomPropertyDrawer(typeof(UnityEventBase), true)]
    class UnityEventFoldoutDrawer : UnityEventDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) 
            {
                return EditorGUIUtility.singleLineHeight;
            }

            return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect foldoutRect = new Rect(position);
            foldoutRect.height = EditorGUIUtility.singleLineHeight;

            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
            if (property.isExpanded)
            {
                position.y += EditorGUIUtility.singleLineHeight;
                position.height -= EditorGUIUtility.singleLineHeight;
                using (new EditorGUI.IndentLevelScope())
                {
                    base.OnGUI(position, property, label);
                }
            }
        }
    }
}

#endif