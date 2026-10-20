#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using KOOP.Attributes;

namespace KOOPEditor.Attributes
{
    [CustomPropertyDrawer(typeof(OnValueChangedAttribute))]
    public class OnValueChangedAttributeDrawer : PropertyDrawer
    {
        private readonly Texture _warningIcon = EditorGUIUtility.IconContent("d_console.warnicon.inactive.sml@2x").image;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height -= EditorGUIUtility.standardVerticalSpacing;

            OnValueChangedAttribute onValueChangedAttribute = (OnValueChangedAttribute)attribute;

            // get the method to invoke
            string methodName = onValueChangedAttribute.CallbackName ?? string.Empty;
            object parent = SerializedPropertyUtils.GetParentObjectOfProperty(property);
            MethodInfo method = parent.GetType().GetMethod(methodName, BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, Type.DefaultBinder, Type.EmptyTypes, null);

            // early return if there is no method found
            if (method == null)
            {
                label.image = _warningIcon;
                label.tooltip = $"<color={(EditorGUIUtility.isProSkin ? "yellow" : "red")}>Uh oh! Can't find the method {methodName}() for [OnValueChanged()]</color>{(string.IsNullOrEmpty(label.tooltip) ? string.Empty : '\n' + label.tooltip)}";
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            // if we can't invoke the method because we're not allowed to in edit mode and are currently in edit mode
            // then don't bother checking for changes
            if (!onValueChangedAttribute.ExecuteInEditMode && !Application.isPlaying)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            // check for array changes if allowed to execute
            bool hasArrayChanged = false;
            int arrayNameStartIndex = property.propertyPath.IndexOf(".Array.data[");
            bool isPartOfAnArray = arrayNameStartIndex != -1;
            if (isPartOfAnArray)
            {
                bool isFirstElementInArray = property.propertyPath[property.propertyPath.IndexOf('[') + 1] == '0';

                // this OnGUI will be called for every element in the array, so we only want it to happen for the first one
                if (isFirstElementInArray)
                {
                    SerializedProperty arrayProperty = property.serializedObject.FindProperty(property.propertyPath[..arrayNameStartIndex]);

                    // if the size or the content hash changes, there's a change!
                    if (onValueChangedAttribute.ArraySize != arrayProperty.arraySize || onValueChangedAttribute.ArrayContentHash != arrayProperty.contentHash)
                    {
                        // only acknowledge that the array has changed if it's changing from something other than the initial value, so that it doesn't fire on recompile
                        if (onValueChangedAttribute.ArrayContentHash != 0 || onValueChangedAttribute.ArraySize != 0)
                            hasArrayChanged = true;
                        onValueChangedAttribute.ArrayContentHash = arrayProperty.contentHash;
                        onValueChangedAttribute.ArraySize = arrayProperty.arraySize;
                    }
                }
            }

            // check if the property itself detects a change
            using (EditorGUI.ChangeCheckScope changeCheck = new())
            {
                EditorGUI.PropertyField(position, property, label);
                
                // only apply the property change check if the property isn't part of an array!
                // elements in an array, or nested arrays (strings count) will detect the change
                // both in here and in the array change detection, but we don't want the method to fire twice
                if (changeCheck.changed && !isPartOfAnArray || hasArrayChanged)
                    method.Invoke(parent, null);
            }
        }
    }
}

#endif