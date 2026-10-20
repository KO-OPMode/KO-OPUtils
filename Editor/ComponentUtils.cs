#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace KOOPEditor
{
    public static class ComponentUtils
    {
        /// <summary>
        /// Replaces the given component with a component of a different type by modifying the underlying serialized
        /// script ID used by its component. This should preserve the value of any fields that are shared between
        /// both component types, as well as any compatible references to this component.
        ///
        /// (This essentially reproduces the old trick of changing a component's script using the Debug inspector,
        /// but does it auto-magically via code.)
        /// </summary>
        public static void ReplaceComponentWithType<T>(MonoBehaviour oldComponent) where T : MonoBehaviour
        {
            // Create a new instance of this type so we can use MonoScript.FromMonoBehaviour()
            GameObject tempObject = new GameObject();
            T tempComponent = tempObject.AddComponent<T>();
            var monoScript = MonoScript.FromMonoBehaviour(tempComponent);

            try
            {
                var serializedObject = new SerializedObject(oldComponent);
                var scriptProperty = serializedObject.FindProperty("m_Script");
                scriptProperty.objectReferenceValue = monoScript;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
            finally
            {
                Object.DestroyImmediate(tempObject);
            }
        }
    }
}

#endif