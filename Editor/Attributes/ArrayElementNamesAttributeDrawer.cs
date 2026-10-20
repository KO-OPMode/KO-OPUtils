#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using KOOP.Attributes;
using UnityInternalAccess.Editor;

namespace KOOPEditor.Attributes
{
    /// <summary>
    /// @owner Noé<br/>
    /// The property drawer for <see cref="ArrayElementNamesAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(ArrayElementNamesAttribute))]
    public class ArrayElementNamesAttributeDrawer : PropertyDrawer
    {
        private readonly Texture _warningTexture = EditorGUIUtility.IconContent("Warning").image;

        private void UpdateLabel(SerializedProperty property, ref GUIContent label)
        {
            string originalText = label.text;
            
            // try and find the method that was given to the attribute
            string methodName = ((ArrayElementNamesAttribute)attribute).NamingMethodName;
            object parent = SerializedPropertyUtils.GetParentObjectOfProperty(property);
            MethodInfo method = parent.GetType().GetMethod(methodName, BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, Type.DefaultBinder, new[] { typeof(int) }, null);
            
            if (method == null)
            {
                // show the warning symbol and override the tooltip to describe the issue
                label.image = _warningTexture;
                label.tooltip = $"Can't find method {methodName}(int) :(";
            }
            else
            {
                int index = int.Parse(originalText.Substring(originalText.LastIndexOf(' ')));
                label.text = method.Invoke(parent, new object[] { index }) as string;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            UpdateLabel(property, ref label);
            return DefaultPropertyDrawer.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            UpdateLabel(property, ref label);
            using (new EditorGUI.PropertyScope (position, label, property))
            {
                // shrink the height by vertical spacing cause apparently it doesn't do this for us
                position.height -= EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(position, property, label, includeChildren: true);
            }
        }
    }
}

#endif