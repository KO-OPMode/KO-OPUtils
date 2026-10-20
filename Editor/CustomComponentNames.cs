#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System.Collections;
using System.Collections.Generic;

namespace KOOPEditor
{
    /// <summary>
    /// Allows for a component to draw a custom name.
    /// </summary>
    public interface IDrawCustomName
    {
        string GetCustomName();
    }

    /// <summary>
    /// Handles the drawing of custom names for components.</para>
    /// This lets components have individual names related to their content, rather than all being named the same thing,
    /// helping the Inspector generally be a lot more readable.
    /// </summary>
    [InitializeOnLoad]
    static class CustomComponentNames
    {
        // Reflection stuff
        private static readonly FieldInfo inspectorElementEditorField;

        private const string fieldName_allInspectors = "m_AllInspectors";
        private static readonly FieldInfo field_allInspectors;
        private const string typeName_propertyEditor = "UnityEditor.PropertyEditor"; 
        private static readonly Type type_propertyEditor;
        private const string typeName_inspectorWindow = "UnityEditor.InspectorWindow"; 
        private static readonly Type type_inspectorWindow;

        private const string typeName_editorElement = "EditorElement"; 
        private const string token_headerEndsWith = "Header"; 
        private const string fieldName_editor = "m_Editor"; 

        private static bool _shouldUpdatePropertyEditors = true;

        private class ComponentHeaderWrapper
        {
            readonly IMGUIContainer headerElement;
            readonly IDrawCustomName customName;
            readonly Component component;
            readonly Action defaultOnGUIHandler;

            public Component Component => component;

            public ComponentHeaderWrapper(IMGUIContainer headerElement, Component component, IDrawCustomName customName)
            {
                this.headerElement = headerElement;
                this.component = component;
                this.customName = customName;
                defaultOnGUIHandler = headerElement.onGUIHandler;
            }

            public void OnGUIHandler()
            {
                if (component == null)
                {
                    defaultOnGUIHandler?.Invoke();
                    return;
                }

                Rect headerRect = headerElement.contentRect;
                bool headerIsSelected = headerElement.focusController.focusedElement == headerElement;
                bool isFocused = headerRect.Contains(Event.current.mousePosition);

                headerRect.y += 1f;
                headerRect.height -= 4f;
                headerRect.x += 59f;
                headerRect.width -= 120f;

                defaultOnGUIHandler?.Invoke();
                EditorGUI.DrawRect(headerRect, isFocused ? hoverColor : backgroundColor);
                GUI.Label(headerRect, customName.GetCustomName(), TextStyle);
            }

            Color backgroundColor = EditorGUIUtility.isProSkin
            ? new Color32(62, 62, 62, 255)
            : new Color32(203, 203, 203, 255);

            Color hoverColor = EditorGUIUtility.isProSkin
            ? new Color32(71, 71, 71, 255)
            : new Color32(214, 214, 214, 255);

            static GUIStyle _style = null;
            static GUIStyle TextStyle
            {
                get
                {
                    if (_style == null)
                    {
                        _style = new GUIStyle(EditorStyles.label);
                        _style.richText = true;
                    }

                    return _style;
                }
            }
        }

        static CustomComponentNames()
        {
            Editor.finishedDefaultHeaderGUI -= OnFinishedDrawingHeaderGUI;
            Editor.finishedDefaultHeaderGUI += OnFinishedDrawingHeaderGUI;

            // initialize reflection
            inspectorElementEditorField = typeof(InspectorElement).GetField(fieldName_editor, BindingFlags.Instance | BindingFlags.NonPublic);
        
            // get unity editor assembly
            var unityEditorAssembly = typeof(EditorWindow).Assembly;
            
            type_propertyEditor = unityEditorAssembly.GetType(typeName_propertyEditor);
            type_inspectorWindow = unityEditorAssembly.GetType(typeName_inspectorWindow);
            if (type_inspectorWindow == null)
            {
                return;
            }

            field_allInspectors = type_inspectorWindow.GetField(fieldName_allInspectors, BindingFlags.Static | BindingFlags.NonPublic);
            if (field_allInspectors == null)
            {
                return;
            }

            ReloadInspectors();
        }

        private static void ReloadInspectors()
        {
            var inspectorList = field_allInspectors.GetValue(null) as IList;
            _allInspectors.Clear();
            foreach (var elem in inspectorList)
            {
                _allInspectors.Add(elem as EditorWindow);
            }
        }

        private static void OnFinishedDrawingHeaderGUI(Editor editor)
        {
            if (editor == null) return;

            if (editor.target is GameObject)
            {
                // Handle InspectorWindow
                InjectAfterGameObjectEditor(editor);
                return;
            }

            if (editor is IDrawCustomName customNameHandler && editor.target is Component component)
            {
                InjectAfterComponentEditor(editor, component, customNameHandler);
            }
        }

        private static void InjectAfterGameObjectEditor(Editor gameObjectEditor)
        {
            if (!TryGetComponentHeaderElements(gameObjectEditor, out var editors, out var headers))
            {
                return;
            }

            for (int i = 0; i < editors.Length; ++i)
            {            
                var editor = editors[i]; var header = headers[i];

                var onGUIHandler = header.onGUIHandler;
                if (onGUIHandler.Method is MethodInfo onGUI && onGUI.Name == nameof(ComponentHeaderWrapper.OnGUIHandler))
                {
                    continue;
                }

                var component = editor.target as Component;
                if (component == null || header is null || !(editor is IDrawCustomName customNameHandler))
                {
                    continue;
                }

                var wrapper = new ComponentHeaderWrapper(header, component, customNameHandler);
                header.onGUIHandler = wrapper.OnGUIHandler;
            }
        }

        private static void InjectAfterComponentEditor(Editor componentEditor, Component component, IDrawCustomName customNameHandler)
        {
            if (!TryGetComponentHeaderElement(componentEditor, out var foundEditor, out var header))
            {
                return;
            }
            
            var onGUIHandler = header.onGUIHandler;
            if (onGUIHandler.Method is MethodInfo onGUI && onGUI.Name == nameof(ComponentHeaderWrapper.OnGUIHandler))
            {
                return;
            }

            var wrapper = new ComponentHeaderWrapper(header, component, customNameHandler);
            header.onGUIHandler = wrapper.OnGUIHandler;
        }

        private static bool TryGetComponentHeaderElements(Editor gameObjectEditor, out Editor[] editors, out IMGUIContainer[] headers)
        {
            editors = default; headers = default;
            var inspector = GetGameObjectEditorWindow(gameObjectEditor);
            if (inspector == null)
            {
                return false;
            }

            List<Editor> editorList = new(); List<IMGUIContainer> headerList = new();
            if (!TryGetComponentHeaderElements(inspector.rootVisualElement, editorList, headerList))
            {
                return false;
            }

            editors = editorList.ToArray();
            headers = headerList.ToArray();
            return true;
        }
        
        private static EditorWindow GetGameObjectEditorWindow(Editor gameObjectEditor)
        {
            foreach (var window in EditorWindows)
            {
                if (ContainsPropertyEditor(window.rootVisualElement, gameObjectEditor))
                {
                    return window;
                }
            }

            return null;
        }
        
        private static EditorWindow GetComponentPropertyEditor(Editor componentEditor)
        {
            for(int i = PropertyEditors.Count - 1; i >= 0; i--)
            {
                var propertyEditor = PropertyEditors[i];
                if (propertyEditor == null)
                {
                    PropertyEditors.RemoveAt(i);
                    continue;
                }
                
                if (ContainsPropertyEditor(propertyEditor.rootVisualElement, componentEditor))
                {
                    return propertyEditor;
                }
            }

            return null;
        }

        private static List<EditorWindow> _propertyEditors = new();
        private static List<EditorWindow> PropertyEditors
        {
            get
            {
                if (_shouldUpdatePropertyEditors)
                {
                    _shouldUpdatePropertyEditors = false;
                    UpdateAllPropertyEditors();
                }
                else
                {
                    var focusedWindow = EditorWindow.focusedWindow;
                    if (focusedWindow != null && focusedWindow.GetType() == type_propertyEditor && !_propertyEditors.Contains(focusedWindow))
                    {
                        _propertyEditors.Add(focusedWindow);
                    }

                    var mouseOverWindow = EditorWindow.mouseOverWindow;
                    if (mouseOverWindow != null && mouseOverWindow.GetType() == type_propertyEditor && !_propertyEditors.Contains(mouseOverWindow))
                    {
                        _propertyEditors.Add(mouseOverWindow);
                    }
                }

                return _propertyEditors;
            }
        }
        
        private static List<EditorWindow> _allInspectors = new();
        private static IEnumerable<EditorWindow> EditorWindows
        {
            get
            {
                foreach (var inspector in _allInspectors)
                {
                    yield return inspector;
                }

                foreach (var propertyEditors in PropertyEditors)
                {
                    yield return propertyEditors;
                }
            }
        }

        private static void UpdateAllPropertyEditors()
        {
            _propertyEditors.Clear();
            _propertyEditors.AddRange(Resources.FindObjectsOfTypeAll(type_propertyEditor) as EditorWindow[]);
        }

        private static bool TryGetComponentHeaderElement(Editor componentEditor, out Editor foundEditor, out IMGUIContainer header)
        {
            foundEditor = null; header = null;
            var propertyEditor = GetComponentPropertyEditor(componentEditor);
            if (propertyEditor == null) return false;

            if (!TryGetFirstComponentHeaderElement(propertyEditor.rootVisualElement, out foundEditor, out header)) return false;

            return true;
        }

        private static bool TryGetFirstComponentHeaderElement(VisualElement parentElement, out Editor editor, out IMGUIContainer header)
        {
            editor = default; header = default;
            if (parentElement == null)
            {
                return false;
            }

            if (parentElement.GetType().Name == typeName_editorElement)
            {
                foreach (var child in parentElement.Children())
                {
                    if (TryGetComponentHeaderElement(child, out var headerOrNull))
                    {
                        header = headerOrNull;
                    }
                    else if (header == null)
                    {
                        continue;
                    }

                    if (child is InspectorElement inspectorElement)
                    {
                        editor = inspectorElementEditorField.GetValue(inspectorElement) as Editor;

                        if (editor != null && editor.target as Component != null)
                        {
                            return true;
                        }
                        
                        break;
                    }
                }
            }

            foreach (var child in parentElement.Children())
            {
                if (TryGetFirstComponentHeaderElement(child, out editor, out header))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetComponentHeaderElement(VisualElement visualElement, out IMGUIContainer header)
        {
            if (visualElement is IMGUIContainer imguiContainer && visualElement.name.EndsWith(token_headerEndsWith, StringComparison.Ordinal))
            {
                header = imguiContainer;
                return true;
            }

            header = default;
            return false;
        }
        

        private static bool ContainsPropertyEditor(VisualElement parentElement, Editor editor)
        {
            if (parentElement == null)
            {
                return false;
            }

            if (parentElement.GetType().Name == typeName_editorElement)
            {
                foreach (var child in parentElement.Children())
                {
                    if (child is InspectorElement inspectorElement)
                    {
                        var childEditor = inspectorElementEditorField.GetValue(inspectorElement) as Editor;
                        if (editor == childEditor)
                        {
                            return true;
                        }
                    
                        if (editor != null && editor.target as GameObject != null)
                        {
                            return false;
                        }
                    }
                }
            }

            foreach(var child in parentElement.Children())
            {
                if (ContainsPropertyEditor(child, editor))
                {
                    return true;
                }
            }

            return false;
        }

        
        private static bool TryGetComponentHeaderElements(VisualElement parentElement, List<Editor> editors, List<IMGUIContainer> headers)
        {
            if (parentElement is null)
            {
                return false;
            }

            if (parentElement.GetType().Name == typeName_editorElement)
            {
                IMGUIContainer header = null;
                foreach(var child in parentElement.Children())
                {
                    if (TryGetComponentHeaderElement(child, out var headerOrNull))
                    {
                        header = headerOrNull;
                    }
                    else if(header is null)
                    {
                        continue;
                    }

                    if (child is InspectorElement inspectorElement)
                    {
                        var editor = inspectorElementEditorField.GetValue(inspectorElement) as Editor;

                        if (editor != null && editor.target as Component != null)
                        {
                            editors.Add(editor);
                            headers.Add(header);
                        }

                        break;
                    }
                }
            }

            foreach (var child in parentElement.Children())
            {
                TryGetComponentHeaderElements(child, editors, headers);
            }

            return true;
        }
    }
}

#endif