using System;
using UnityEditor;
using UnityEngine;

namespace KOOP.SerializeReferenceExtensions.Editor
{
    public static class ManagedReferenceContextualPropertyMenu
    {
        private const string CopiedPropertyPathKey = "SerializeReferenceExtensions.CopiedPropertyPath";
        private const string ClipboardKey = "SerializeReferenceExtensions.CopyAndPasteProperty";

        private static readonly GUIContent PasteContent = new("Paste Property");
        private static readonly GUIContent NewInstanceContent = new("New Instance");
        private static readonly GUIContent ResetAndNewInstanceContent = new("Reset and New Instance");

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.contextualPropertyMenu += OnContextualPropertyMenu;
        }

        private static void OnContextualPropertyMenu(GenericMenu menu, SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                // NOTE: When the callback function is called, the SerializedProperty is rewritten to the property that was being moused over at the time,
                // so a new SerializedProperty instance must be created.
                var clonedProperty = property.Copy();

                menu.AddItem(new GUIContent($"Copy \"{property.propertyPath}\" property"), false, Copy, clonedProperty);

                var copiedPropertyPath = SessionState.GetString(CopiedPropertyPathKey, string.Empty);
                
                if (!string.IsNullOrEmpty(copiedPropertyPath))
                {
                    menu.AddItem(new GUIContent($"Paste \"{copiedPropertyPath}\" property"), false, Paste, clonedProperty);
                }
                else
                {
                    menu.AddDisabledItem(PasteContent);
                }

                menu.AddSeparator("");

                var hasInstance = clonedProperty.managedReferenceValue != null;
                
                if (hasInstance)
                {
                    menu.AddItem(NewInstanceContent, false, NewInstance, clonedProperty);
                    menu.AddItem(ResetAndNewInstanceContent, false, ResetAndNewInstance, clonedProperty);
                }
                else
                {
                    menu.AddDisabledItem(NewInstanceContent);
                    menu.AddDisabledItem(ResetAndNewInstanceContent);
                }
            }
        }

        private static void Copy(object customData)
        {
            var property = (SerializedProperty)customData;
            var json = JsonUtility.ToJson(property.managedReferenceValue);
            SessionState.SetString(CopiedPropertyPathKey, property.propertyPath);
            SessionState.SetString(ClipboardKey, json);
        }

        private static void Paste(object customData)
        {
            var property = (SerializedProperty)customData;
            var json = SessionState.GetString(ClipboardKey, string.Empty);
            
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            Undo.RecordObject(property.serializedObject.targetObject, "Paste Property");
            JsonUtility.FromJsonOverwrite(json, property.managedReferenceValue);
            property.serializedObject.ApplyModifiedProperties();
        }

        private static void NewInstance(object customData)
        {
            var property = (SerializedProperty)customData;
            var json = JsonUtility.ToJson(property.managedReferenceValue);

            Undo.RecordObject(property.serializedObject.targetObject, "New Instance");
            property.managedReferenceValue = JsonUtility.FromJson(json, property.managedReferenceValue.GetType());
            property.serializedObject.ApplyModifiedProperties();
        }

        private static void ResetAndNewInstance(object customData)
        {
            var property = (SerializedProperty)customData;

            Undo.RecordObject(property.serializedObject.targetObject, "Reset and New Instance");
            property.managedReferenceValue = Activator.CreateInstance(property.managedReferenceValue.GetType());
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}