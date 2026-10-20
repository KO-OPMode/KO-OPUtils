using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace KOOP
{
    [System.Serializable]
    public class PlayerPref<T>
    {
        public PlayerPref(string key)
        {
            this.key = key;
        }

        public PlayerPref(string key, bool perProject)
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
            if (!PlayerPrefs.HasKey(key))
            {
                return defaultValue;
            }

            return (T)GetValue(key, typeof(T), defaultValue);
        }

        void SetValue(string key, System.Type type, object value)
        {
            if (type == typeof(bool))
            {
                SetBool(key, (bool)value);
            }
            else if (type == typeof(int))
            {
                PlayerPrefs.SetInt(key, (int)value);
            }
            else if (type == typeof(float))
            {
                PlayerPrefs.SetFloat(key, (float)value);
            }
            else
            {
                PlayerPrefs.SetString(key, value.ToString());
            }
        }

        object GetValue(string key, System.Type type, object defaultValue)
        {
            if (type == typeof(bool))
            {
                return GetBool(key, (bool)defaultValue);
            }
            else if (type == typeof(int))
            {
                return PlayerPrefs.GetInt(key, (int)defaultValue);
            }
            else if (type == typeof(float))
            {
                return PlayerPrefs.GetFloat(key, (float)defaultValue);
            }
            else
            {
                return PlayerPrefs.GetString(key, defaultValue == null ? string.Empty : defaultValue.ToString());
            }
        }

        bool GetBool(string key, bool defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }
        
        static string AppendProjectTitleToKey(string key)
        {
            return $"{GetProjectName()}_{key}";
        }

        static string GetProjectName()
        {
            string[] s = Application.dataPath.Split('/');
            string projectName = s[s.Length - 2];
            return projectName;
        }
    }
}