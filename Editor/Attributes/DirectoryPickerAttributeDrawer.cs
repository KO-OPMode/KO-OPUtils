#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

using KOOP.Attributes;

namespace KOOPEditor.Attributes
{
    [CustomPropertyDrawer(typeof(DirectoryPickerAttribute))]
    public class DirectoryPickerAttributeDrawer : PropertyDrawer
    {
        const float buttonWidth = 25f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            bool buttonPressed = false;
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                position.width -= buttonWidth;

                EditorGUI.PropertyField(position, property);

                position.x += position.width;
                position.width = buttonWidth;

                buttonPressed = GUI.Button(position, "...");
            }

            if (buttonPressed)
            {
                string opt = EditorUtility.OpenFolderPanel("Pick directory", "", "");
                if (opt != string.Empty)
                {
                    property.stringValue = AbsoluteToAssetPath(opt);
                }
            }
        }

        public static string AbsoluteToAssetPath(string absolutePath)
        {
            if (absolutePath.StartsWith(Application.dataPath))
            {
                absolutePath = absolutePath.Substring(Application.dataPath.Length+1);
            }

            return absolutePath;
        }
    }
}

#endif