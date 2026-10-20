#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace KOOP.Extensions
{
    public static class EditorPrefsExtensions
    {
        /// <summary>
        /// Get the persistent debug value of a float
        /// </summary>
        public static float GetDebugFloat(string key)
        {
#if UNITY_EDITOR
            return EditorPrefs.GetFloat(key, 0f);
#else   
            return 0f;
#endif
        }

        /// <summary>
        /// Set the persistent debug value of a float
        /// </summary>
        public static void SetDebugFloat(string key, float value)
        {
#if UNITY_EDITOR
            EditorPrefs.SetFloat(key, value);
#else   
            //Do nothing
#endif
        }

        /// <summary>
        /// Get the persistent debug value of a boolean
        /// </summary>
        public static bool GetDebugBool(string key)
        {
#if UNITY_EDITOR
            return EditorPrefs.GetInt(key, 0) == 1;
#else   
            return false;
#endif
        }

        /// <summary>
        /// Set the persistent debug value of a boolean
        /// </summary>
        public static void SetDebugBool(string key, bool value)
        {
#if UNITY_EDITOR
            EditorPrefs.SetInt(key, value ? 1 : 0);
#else   
            //Do nothing
#endif
        }

        /// <summary>
        /// Get the persistent debug value of an integer
        /// </summary>
        public static int GetDebugInt(string key)
        {
#if UNITY_EDITOR
            return EditorPrefs.GetInt(key, -1);
#else   
            return -1;
#endif
        }

        /// <summary>
        /// Set the persistent debug value of an integer
        /// </summary>
        public static void SetDebugInt(string key, int value)
        {
#if UNITY_EDITOR
            EditorPrefs.SetInt(key, value);
#else   
            //Do nothing
#endif
        }

        /// <summary>
        /// Get the persistent debug value of a Vector3
        /// </summary>
        public static Vector3 GetDebugVector3(string key)
        {
#if UNITY_EDITOR
            return new Vector3(
                EditorPrefs.GetFloat(key + "X", 0),
                EditorPrefs.GetFloat(key + "Y", 0),
                EditorPrefs.GetFloat(key + "Z", 0)
            );
#else
            return Vector3.zero;
#endif
        }

        /// <summary>
        /// Set the persistent debug value of a Vector3
        /// </summary>
        public static void SetDebugVector3(string key, Vector3 value)
        {
#if UNITY_EDITOR
            EditorPrefs.SetFloat(key + "X", value.x);
            EditorPrefs.SetFloat(key + "Y", value.y);
            EditorPrefs.SetFloat(key + "Z", value.z);
#else   
            //Do nothing
#endif
        }
    }
}