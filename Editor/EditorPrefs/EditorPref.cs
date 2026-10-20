#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace KOOPEditor
{
    [System.Serializable]
    public class EditorPref<T>
    {
        public EditorPref(string key)
        {
            this.key = key;
        }

        public EditorPref(string key, bool perProject)
        {
            this.key = key;
            if (perProject)
            {
                this.key = AppendProjectTitleToKey(key);
            }
        }

        string key;

        public void Set(T value)
        {
            System.Type type = typeof(T);
            SetValue(key, type, value);
        }

        public T Get() { return Get(default(T)); }
        public T Get(T defaultValue)
        {
            if (!EditorPrefs.HasKey(key))
            {
                return defaultValue;
            }

            return (T)GetValue(key, typeof(T), defaultValue);
        }

        void SetValue(string key, System.Type type, object value)
        {
            if (type == typeof(bool))
            {
                EditorPrefs.SetBool(key, (bool)value);
            }
            else if (type == typeof(int))
            {
                EditorPrefs.SetInt(key, (int)value);
            }
            else if (type == typeof(float))
            {
                EditorPrefs.SetFloat(key, (float)value);
            }
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                SetAsset(key, value as UnityEngine.Object);
            }
            else
            {
                EditorPrefs.SetString(key, value.ToString());
            }
        }

        object GetValue(string key, System.Type type, object defaultValue)
        {
            if (type == typeof(bool))
            {
                return EditorPrefs.GetBool(key, (bool)defaultValue);
            }
            else if (type == typeof(int))
            {
                return EditorPrefs.GetInt(key, (int)defaultValue);
            }
            else if (type == typeof(float))
            {
                return EditorPrefs.GetFloat(key, (float)defaultValue);
            }
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                return GetAsset(key);
            }
            else
            {
                return EditorPrefs.GetString(key, defaultValue == null ? string.Empty : defaultValue.ToString());
            }
        }
        
        static string AppendProjectTitleToKey(string key)
        {
            return $"{GetProjectName()}_{key}";
        }

        static void SetAsset(string key, Object value) { SetAsset(key, GetAssetPath(value)); }
        static void SetAsset(string key, string assetPath)
        {
            EditorPrefs.SetString(key, assetPath);
        }

        static Object GetAsset(string key)
        {
            string assetPath = EditorPrefs.GetString(key);
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            return AssetDatabase.LoadMainAssetAtPath(assetPath);
        }

        static string GetAssetPath(Object obj)
        {
            if (obj is GameObject go)
            {
                if (string.IsNullOrEmpty(go.scene.name))
                {
                    return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
                }
            }
            
            return AssetDatabase.GetAssetPath(obj);
        }

        static string GetProjectName()
        {
            string[] s = Application.dataPath.Split('/');
            string projectName = s[s.Length - 2];
            return projectName;
        }
    }

    [System.Serializable]
    internal class EditorArrayPref<T>
    {
        public EditorArrayPref(string key)
        {
            this.key = key;
        }

        public EditorArrayPref(string key, bool perProject)
        {
            this.key = key;
            if (perProject)
            {
                this.key = AppendProjectTitleToKey(key);
            }
        }

        string key;

        public void Set(T[] value)
        {
            System.Type type = typeof(T);
            SetArray(value);
        }

        public T[] Get()
        {
            if (!EditorPrefs.HasKey(key))
            {
                return null;
            }

            return GetArray();
        }

        void SetValue(string key, System.Type type, object value)
        {
            if (type == typeof(bool))
            {
                EditorPrefs.SetBool(key, (bool)value);
            }
            else if (type == typeof(int))
            {
                EditorPrefs.SetInt(key, (int)value);
            }
            else if (type == typeof(float))
            {
                EditorPrefs.SetFloat(key, (float)value);
            }
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                SetAsset(key, value as UnityEngine.Object);
            }
            else
            {
                EditorPrefs.SetString(key, value.ToString());
            }
        }

        object GetValue(string key, System.Type type, object defaultValue)
        {
            if (type == typeof(bool))
            {
                return EditorPrefs.GetBool(key, (bool)defaultValue);
            }
            else if (type == typeof(int))
            {
                return EditorPrefs.GetInt(key, (int)defaultValue);
            }
            else if (type == typeof(float))
            {
                return EditorPrefs.GetFloat(key, (float)defaultValue);
            }
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                return GetAsset(key);
            }
            else
            {
                return EditorPrefs.GetString(key, defaultValue == null ? string.Empty : defaultValue.ToString());
            }
        }
        
        static string AppendProjectTitleToKey(string key)
        {
            return $"{GetProjectName()}_{key}";
        }

        void SetArray(T[] values)
        {
            if (values == null) return;

            System.Type type = values.GetType().GetElementType();

            int prev = EditorPrefs.GetInt(key, 0);
            /** Set length of array as the default key value */
            EditorPrefs.SetInt(key, values.Length);
            for (int i = 0; i < values.Length; ++i)
            {
                SetValue($"{key}_{i}", type, values[i]);
            }

            if (prev > values.Length)
            {
                for (int i = values.Length; i < prev; ++i)
                {
                    EditorPrefs.DeleteKey($"{key}_{i}");
                }
            }
        }

        T[] GetArray()
        {
            List<T> objs = new List<T>();
            /** Set length of array as the default key value */
            int arrayLength = EditorPrefs.GetInt(key, 0);
            System.Type type = typeof(T);

            for (int i = 0; i < arrayLength; ++i)
            {
                objs.Add((T)GetValue($"{key}_{i}", type, default(T)));
            }

            return objs.ToArray();
        }

        static void SetAsset(string key, Object value) { SetAsset(key, GetAssetPath(value)); }
        static void SetAsset(string key, string assetPath)
        {
            EditorPrefs.SetString(key, assetPath);
        }

        static Object GetAsset(string key)
        {
            string assetPath = EditorPrefs.GetString(key);
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            return AssetDatabase.LoadMainAssetAtPath(assetPath);
        }

        static string GetAssetPath(Object obj)
        {
            if (obj is GameObject go)
            {
                if (string.IsNullOrEmpty(go.scene.name))
                {
                    return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
                }
            }
            
            return AssetDatabase.GetAssetPath(obj);
        }

        static string GetProjectName()
        {
            string[] s = Application.dataPath.Split('/');
            string projectName = s[s.Length - 2];
            return projectName;
        }
    }
}

#endif