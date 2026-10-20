#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace KOOPEditor
{
    public static class EditorWindowUtils
    {
        public static bool TryGetWindow<T>(out T window, bool focusIfFound = false) where T : EditorWindow
        {
            T[] array = Resources.FindObjectsOfTypeAll(typeof(T)) as T[];
            window = (array.Length != 0) ? array[0] : null;
            if (window != null)
            {
                if (focusIfFound)
                {
                    window.Focus();
                }
            }

            return window != null;
        }
    }
}

#endif