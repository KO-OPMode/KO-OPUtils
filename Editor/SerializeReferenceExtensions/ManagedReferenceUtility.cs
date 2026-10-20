using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace KOOP.SerializeReferenceExtensions.Editor
{
    public static class ManagedReferenceUtility
    {
        public static object SetManagedReference(this SerializedProperty property, Type type)
        {
            object result = null;

            // NOTE: managedReferenceValue getter is available only in Unity 2021.3 or later.
            if ((type != null) && (property.managedReferenceValue != null))
            {
                // Restore an previous values from json.
                var json = JsonUtility.ToJson(property.managedReferenceValue);
                result = JsonUtility.FromJson(json, type);
            }

            if (result == null)
            {
                result = (type != null) ? Activator.CreateInstance(type) : null;
            }

            property.managedReferenceValue = result;
            return result;
        }

        public static Type GetType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            var splitIndex = typeName.IndexOf(' ');
            var assembly = Assembly.Load(typeName.Substring(0, splitIndex));
            return assembly.GetType(typeName.Substring(splitIndex + 1));
        }
    }
}