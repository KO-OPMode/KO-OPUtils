using KOOP;
using UnityEditor;
using UnityEngine;

namespace KOOPEditor
{
    /// <owner>Lazlo</owner>
    [CustomPropertyDrawer(typeof(Optional<>), useForChildren: true)]
    public sealed class OptionalDrawer : PropertyDrawer
    {
        private const string HasValueFieldName = "_hasValue";
        private const string ValueFieldName = "_value";
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var hasValueProperty = property.FindPropertyRelative(HasValueFieldName);
            var valueProperty = property.FindPropertyRelative(ValueFieldName);

            if (hasValueProperty.boolValue)
            {
                return EditorGUI.GetPropertyHeight(valueProperty);
            }
            else
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var hasValueProperty = property.FindPropertyRelative(HasValueFieldName);
            var valueProperty = property.FindPropertyRelative(ValueFieldName);
            
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            var hasValueTogglePosition = position;
            hasValueTogglePosition.width = 16;
            hasValueTogglePosition.height = EditorGUIUtility.singleLineHeight;

            var valueControlPosition = position;
            valueControlPosition.xMin += 20;

            EditorGUI.PropertyField(hasValueTogglePosition, hasValueProperty, GUIContent.none);

            if (hasValueProperty.boolValue)
            {
                EditorGUI.PropertyField(valueControlPosition, valueProperty, GUIContent.none);
            }

            EditorGUI.EndProperty();
        }
    }
}