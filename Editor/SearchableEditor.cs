#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace KOOPEditor
{
    /// <summary>
    /// Base class for component inspectors that allow the user to search through their properties.
    /// NOTE: Does not current support Unity's new Visual Toolkit / UIElements API, so any properties
    /// that don't specify a legacy OnGUI() method in their property drawer won't render correctly. 
    /// </summary>
    public abstract class SearchableEditor : Editor
    {
        private static GUIStyle _searchHintStyle;
        protected static GUIStyle SearchHintStyle
        {
            get
            {
                if (_searchHintStyle == null)
                {
                    _searchHintStyle = new GUIStyle(EditorStyles.label);
                    _searchHintStyle.fontSize = 10;
                    _searchHintStyle.normal.textColor = Color.gray;
                }

                return _searchHintStyle;
            }
        }
        
        protected string search = string.Empty;
        private SerializedProperty scriptProperty; const string scriptPropertyName = "m_Script";
        
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            
            DrawScriptField();

            DrawSearchBar();

            DrawProperties();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawScriptField()
        {
            if (scriptProperty == null)
            {
                scriptProperty = serializedObject.FindProperty(scriptPropertyName);
            }

            using (new EditorGUI.DisabledGroupScope(true)) EditorGUILayout.PropertyField(scriptProperty);
        }

        protected virtual void DrawSearchBar()
        {
            EditorGUILayout.Space();

            var rect = EditorGUILayout.GetControlRect();
            search = EditorGUI.TextField(rect, search, EditorStyles.toolbarSearchField);
            if (string.IsNullOrEmpty(search))
            {
                const float searchBarPadding = 12f;

                rect.y -= 1f;
                rect.width -= searchBarPadding * 2f;
                rect.x += searchBarPadding;
                EditorGUI.LabelField(rect, "Search...", SearchHintStyle);
            }

            EditorGUILayout.Space();
        }

        protected virtual void DrawProperties()
        {
            if (scriptProperty == null)
            {
                scriptProperty = serializedObject.FindProperty(scriptPropertyName);
            }

            if (string.IsNullOrEmpty(search))
            {
                DrawPropertiesExcluding(serializedObject, scriptPropertyName);
            }
            else
            {
                SerializedProperty prop = scriptProperty.Copy();
                while (prop.NextVisible(SearchInChildren))
                {
                    if (IsPropertyIncludedInSearch(prop, search))
                    {
                        EditorGUILayout.PropertyField(prop);
                    }
                }
            }
        }

		protected virtual bool SearchInChildren => true;

        protected virtual bool IsPropertyIncludedInSearch(SerializedProperty property, string search)
        {
            if (string.IsNullOrEmpty(search)) return true;

            string[] words = search.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                if (!property.name.ToLower().Contains(word))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

#endif