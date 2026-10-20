
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using KOOP;

// TODO: General cleanup

namespace KOOPEditor
{
    [CustomPropertyDrawer(typeof(SerializableType))]
    public class SerializableTypePropertyDrawer : PropertyDrawer
    {
        private const string _propName_assemblyQualifiedName = "_assemblyQualifiedName";
        private const float _openTypeWindowButtonWidth = 16f;
        private const float _dropdownMinWidth = 128f;

        private List<System.Type> _cachedTypes = null;
        private FieldInfo _serializedFieldInfo = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            OnGUIMultiple(position, property, label, EditorGUI.showMixedValue);
        }
            
        public void OnGUIMultiple(Rect position, SerializedProperty property, GUIContent label, bool showMixed)
        {
            if (_serializedFieldInfo == null)
            {
                _serializedFieldInfo = SerializedPropertyUtils.GetFieldInfo(property);
            }

            if (_cachedTypes == null)
            {
                _cachedTypes = CacheTypes(_serializedFieldInfo);
            }

            /** Pre-initialize */
            EditorStyles.label.richText = true;

            using (new EditorGUI.PropertyScope(position, label, property))
            {
                Rect fieldRect = new(position);
                fieldRect.width = Mathf.Max(fieldRect.width, _dropdownMinWidth);
                
                /** Draw prefix label */
                position = EditorGUI.PrefixLabel(position, label);
                
                position.width -= _openTypeWindowButtonWidth;

                string assemblyQualifiedName = property.FindPropertyRelative(_propName_assemblyQualifiedName).stringValue;
                string name = SerializableType.AssemblyQualifiedToTypeName(assemblyQualifiedName);
                System.Type type = System.Type.GetType(assemblyQualifiedName);

                bool hasType = type != null;

                if (hasType)
                {
                    position.width -= _openTypeWindowButtonWidth;
                }

                if (string.IsNullOrEmpty(assemblyQualifiedName))
                {
                    name = "<color=grey>None";    
                }
                else if (!hasType)
                {
                    name += " <color=red>(Invalid type)";
                }

                EditorGUI.LabelField(position, name, LabelTypeStyle);
                GUI.color = Color.white;

                position.x += position.width;
                position.width = _openTypeWindowButtonWidth;

                if (GUI.Button(position, EditorGUIUtility.IconContent("_Menu"), hasType ? EditorStyles.miniButtonMid : EditorStyles.miniButtonRight))
                {
                    SerialTypePopupWindow popupWindow = new SerialTypePopupWindow(_cachedTypes.ToArray(), fieldRect.width);
                    popupWindow.onClose += (int index) => 
                    {
                        if (index < 0|| index >= _cachedTypes.Count) return;

                        property.serializedObject.UpdateIfRequiredOrScript();

                        property.FindPropertyRelative(_propName_assemblyQualifiedName).stringValue = _cachedTypes[index].AssemblyQualifiedName;

                        property.serializedObject.ApplyModifiedProperties();
                    };

                    PopupWindow.Show(fieldRect, popupWindow);
                }

                if (hasType)
                {
                    position.x += position.width;
                    
                    if (GUI.Button(position, "x", EditorStyles.miniButtonRight))
                    {
                        property.FindPropertyRelative(_propName_assemblyQualifiedName).stringValue = string.Empty;
                    }
                }
            }
        }

        private GUIStyle _labelTypeStyle;
        private GUIStyle LabelTypeStyle
        {
            get
            {
                if (_labelTypeStyle == null)
                {
                    _labelTypeStyle = new(EditorStyles.textField);
                    _labelTypeStyle.richText = true;
                }

                return _labelTypeStyle;
            }
        }

        private List<System.Type> CacheTypes(FieldInfo fieldInfo)
        {
            SubclassesOfTypeAttribute subclasses = fieldInfo.GetCustomAttribute<SubclassesOfTypeAttribute>();
            if (subclasses == null)
            {
                // Defaults to subclasses of MonoBehaviour
                subclasses = new(typeof(MonoBehaviour));
            }

            List<System.Type> typeList = new(TypeCache.GetTypesDerivedFrom(subclasses.SubclassesOf));
            typeList.Sort((x, y) => x.Name.CompareTo(y.Name));
            return typeList;
        }

        private class SerialTypePopupWindow : PopupWindowContent
        {
            public delegate void OnCloseCallback(int index);
            public OnCloseCallback onClose;
            
            private const float toolbarHeight = 21f;

            private Vector2 _scrollPosition = Vector2.zero;
            private Vector2 _windowSize = Vector2.zero;
            private int _selectedIndex = 0;

            private string[] _typeNames = null;
            private System.Type[] _types = null;

            private string _search = string.Empty;

            private Rect topBarRect = new();
            private Rect listAreaRect = new();

            private bool _firstFrame = true;

            public SerialTypePopupWindow(System.Type[] types, float defaultWidth) 
            {
                _labelStyle = new(EditorStyles.label);
                _labelStyle.richText = true;

                _types = types;
                RefreshTypeNames(types);
                _windowSize = new(defaultWidth, 200f);
            }
            
            private void RefreshTypeNames(System.Type[] types)
            {
                _typeNames = new string[types.Length];

                string color = ColorUtility.ToHtmlStringRGB(_labelStyle.normal.textColor);
                color += "99";

                for (int i = 0; i < _typeNames.Length; ++i)
                {
                    _typeNames[i] = $"{types[i].Name} <color=#{color}>{types[i].Namespace}</color>";
                }
            }

            private void CalculateRects(Rect rect)
            {
                topBarRect = new Rect(0, 0f, rect.width, toolbarHeight);
                listAreaRect = new Rect(0, toolbarHeight, rect.width, rect.height - toolbarHeight);
            }

            public override Vector2 GetWindowSize() => _windowSize;

            public override void OnGUI(Rect rect) 
            {
                if (Event.current.type == EventType.KeyDown)
                {
                    if (Event.current.keyCode == KeyCode.Return)
                    {
                        onClose?.Invoke(_selectedIndex);
                        editorWindow.Close();
                        return;
                    }
                    else if (Event.current.keyCode == KeyCode.UpArrow)
                    {
                        _selectedIndex = PreviousIndex(_selectedIndex);
                    }
                    else if (Event.current.keyCode == KeyCode.DownArrow)
                    {                        
                        _selectedIndex = NextIndex(_selectedIndex);
                    }
                }

                CalculateRects(rect);

                using (new GUILayout.AreaScope(topBarRect))
                {
                    using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
                    {
                        float labelFloatMaxW = EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth + 5; // (EditorGUI.kSpacing);
                        Rect searchbarRect = GUILayoutUtility.GetRect(labelFloatMaxW * 0.2f, labelFloatMaxW * 1.5f, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight, EditorStyles.toolbarSearchField);

                        GUI.SetNextControlName("SerialType_Searchbar");
                        _search = EditorGUI.TextField(searchbarRect, _search, EditorStyles.toolbarSearchField);

                        if (_firstFrame)
                        {
                            _firstFrame = false;
                            GUI.FocusControl("SerialType_Searchbar");
                        }
                    }
                }

                int selected = -1;

                using (new GUILayout.AreaScope(listAreaRect))
                {
                    using (var sv = new EditorGUILayout.ScrollViewScope(_scrollPosition))
                    {
                        _scrollPosition = sv.scrollPosition;

                        bool alternate = false;
                        for (int i = 0; i < _types.Length; ++i)
                        {
                            if (IsIncludedInSearch(i, _search))
                            {
                                Rect r = EditorGUILayout.GetControlRect();
                                alternate = !alternate;

                                if (alternate)
                                {
                                    EditorGUI.DrawRect(r, new Color(1f, 1f, 1f, 0.2f));
                                }

                                GUIStyle style = EditorStyles.label;

                                string label = _typeNames[i];
                                if (r.Contains(Event.current.mousePosition))
                                {
                                    label = "<b>" + label;
                                }

                                if (GUI.Button(r, label, style))
                                {
                                    selected = i;
                                }
                            }
                        }
                    }
                }

                if (selected != -1)
                {
                    onClose?.Invoke(selected);
                    editorWindow.Close();
                }

                editorWindow.Repaint();
            }

            private int PreviousIndex(int index)
            {
                for (int i = index-1; i >= 0; --i)
                {
                    if (IsIncludedInSearch(i, _search))
                    {
                        return i;
                    }
                }

                return index;
            }

            private int NextIndex(int index)
            {
                for (int i = index+1; i < _types.Length; ++i)
                {
                    if (IsIncludedInSearch(i, _search))
                    {
                        return i;
                    }
                }

                return index;
            }

            public bool IsIncludedInSearch(int index, string search)
            {
                if (string.IsNullOrEmpty(search)) return true;

                search = search.ToLower();

                return _types[index].Name.ToLower().Contains(search) || (!string.IsNullOrEmpty(_types[index].Namespace) && _types[index].Namespace.ToLower().Contains(search));
            }

            private GUIStyle _labelStyle;
        }
    }
}

#endif