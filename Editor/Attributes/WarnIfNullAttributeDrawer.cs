#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using KOOP.Attributes;

namespace KOOPEditor.Attributes
{
    [CustomPropertyDrawer(typeof(WarnIfNullAttribute))]
    public class WarnIfNullAttributeDrawer : PropertyDrawer
    {
        private const float upperWarningSpacing = 10f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            // If we have an editor override, we don't need to worry about anything else!
            if (!property.objectReferenceValue)
            {
                float helpboxHeight = CalcHelpboxHeight((attribute as WarnIfNullAttribute).WarningMessage);
                height += helpboxHeight
                    + EditorGUIUtility.standardVerticalSpacing 
                    + upperWarningSpacing;
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorStyles.helpBox.richText = true;

            if (property.objectReferenceValue == null)
            {
                position = DrawHelpbox(position, (attribute as WarnIfNullAttribute).WarningMessage);
            }

            position = EditorGUI.PrefixLabel(position, label);
            
            EditorGUI.PropertyField(position, property, GUIContent.none);
        }

        private Rect DrawHelpbox(Rect position, string message)
        {
            Rect initialRect = new(position);

            position.width = initialRect.width - (EditorGUIUtility.labelWidth + EditorGUIUtility.standardVerticalSpacing);
            position.x += EditorGUIUtility.labelWidth + EditorGUIUtility.standardVerticalSpacing;
            position.y += upperWarningSpacing;

            float helpboxHeight = CalcHelpboxHeight(message);
            position.height = helpboxHeight;

            using (new EditorGUIUtility.IconSizeScope(new(16f, 16f)))
            {
                EditorGUI.HelpBox(position, message, MessageType.Warning);
            }

            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

            position.height = initialRect.height - (position.y - initialRect.y);
            position.x = initialRect.x;
            position.width = initialRect.width;

            return position;
        }

        private static float CalcHelpboxHeight(string message)
        {
            return EditorStyles.helpBox.CalcHeight(new GUIContent(message), viewWidth);
        }

        private static float viewWidth => EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - EditorGUIUtility.standardVerticalSpacing - 40f;
    }
}

#endif