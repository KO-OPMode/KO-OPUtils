#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace KOOPEditor
{
    public static class SerializedPropertyUtils
    {
        public static T GetTargetObjectOfProperty<T>(SerializedProperty prop)
        {
            return (T)GetTargetObjectOfProperty(prop);
        }

        public static object GetParentObjectOfProperty(SerializedProperty prop)
        {
            if (prop == null)
            {
                return null;
            }

            object obj = prop.serializedObject.targetObject;
            string[] propertyElements = ElementsFromProperty(prop);
            // Skip the last one, as that would give us the property itself, not its target object
            for (int i = 0; i < propertyElements.Length - 1; i++)
            {
                string element = propertyElements[i];

                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    obj = GetValue_Imp(obj, elementName);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);

                    if (i == propertyElements.Length - 2)
                    {
                        break;
                    }
                }
            }

            if (propertyElements[propertyElements.Length - 1].Contains("["))
            {
                string element = propertyElements[propertyElements.Length - 1];
                var elementName = element.Substring(0, element.IndexOf("["));
            }

            return obj;
        }

        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            if (prop == null) 
            {
                return null;
            }

            object obj = prop.serializedObject.targetObject;
            foreach (var element in ElementsFromProperty(prop))
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }

            return obj;
        }

        private static string[] ElementsFromProperty(SerializedProperty prop)
        {
            /** Get full path */
            var path = PropertyPathToPath(prop.propertyPath);

            return path.Split('.');
        }

        private static string PropertyPathToPath(string propertyPath)
        {
            return propertyPath.Replace(".Array.data[", "[");
        }
        
        private static string PropertyPathToFieldName(SerializedProperty prop)
        {
            string[] parts = ElementsFromProperty(prop);

            if (parts.Length == 0)
            {
                return string.Empty;
            }
            else
            {
                string finalPart = parts[parts.Length-1];

                if (finalPart.Contains("["))
                {
                    return finalPart.Substring(0, finalPart.IndexOf("["));
                }
                else
                {
                    return finalPart;
                }
            }
        }

        public static FieldInfo GetFieldInfo(SerializedProperty prop)
        {
            if (prop == null) 
            {
                return null;
            }
            
            object source = GetParentObjectOfProperty(prop);
            string subpropertyName = PropertyPathToFieldName(prop);
            
            FieldInfo fieldInfo = GetFieldInfo(source, subpropertyName);

            return fieldInfo;
        }

        private static FieldInfo GetFieldInfo(object source, string name)
        {
            if (source == null)
            {
                return null;
            }

            var type = source.GetType();
            while (type != null)
            {
                if (type.IsGenericType)
                {
                    type = type.GetGenericArguments()[0];
                }

                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                {
                    return f;
                }
                
                type = type.BaseType;
            }

            return null;
        }

        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }

        public static SerializedProperty FindPropertyOrThrow(this SerializedObject serializedObject, string propertyPath)
        {
            var property = serializedObject.FindProperty(propertyPath);
            
            if (property == null)
            {
                throw new Exception($"Failed to find property at path '{propertyPath}' on {serializedObject}");
            }

            return property;
        }
    }
}

#endif