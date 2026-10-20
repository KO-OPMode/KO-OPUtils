#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace KOOPEditor
{
    [CustomEditor(typeof(ComponentEditorProxy))]
    public class ComponentEditorProxyEditor : Editor, IDrawCustomName
    {
        private ComponentEditorProxy _proxy;
        private Editor _proxiedEditor;

        private Component[] _components;
        private string[] _componentNames;

        private int _index = -1;

        private void OnEnable()
        {
            _proxy = target as ComponentEditorProxy;
            _components = null;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            using (var c = new EditorGUI.ChangeCheckScope())
            {
                var value = EditorGUILayout.ObjectField("Object", _proxy.ComponentToProxy, typeof(Component), true);
                if (c.changed)
                {
                    _proxy.ComponentToProxy = (Component)value;
                    _components = null;
                    EditorUtility.SetDirty(_proxy);
                }
            }

            if (_proxy.ComponentToProxy != null)
            {
                if (_components == null)
                {
                    _components = _proxy.ComponentToProxy.gameObject.GetComponents<Component>();
                    _componentNames = new string[_components.Length];

                    for (int i = 0; i < _components.Length; ++i)
                    {
                        _componentNames[i] = _components[i].GetType().Name;
                        if (_components[i] == _proxy.ComponentToProxy)
                        {
                            _index = i;
                        }
                    }
                }

                using (var c = new EditorGUI.ChangeCheckScope())
                {
                    var value = EditorGUILayout.Popup("Component", _index, _componentNames);
                    if (c.changed)
                    {
                        _index = value;
                        _proxy.ComponentToProxy = _components[_index];
                    }
                }

                EditorGUILayout.Space();

                EditorGUILayout.HelpBox($"This component is a proxy for {_proxy.ComponentToProxy.GetType().Name} attached to {_proxy.ComponentToProxy.gameObject.name}. It holds no serialized state of its own and only exists to make editing components on sub-objects easier.", MessageType.Warning);
                if (GUILayout.Button("Go to source"))
                {
                    Selection.activeGameObject = _proxy.ComponentToProxy.gameObject;
                }
                
                EditorGUILayout.Space();

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    Editor.CreateCachedEditor(_proxy.ComponentToProxy, null, ref _proxiedEditor);

                    _proxiedEditor.OnInspectorGUI();
                }
            }
        }

        public string GetCustomName()
        {
            if (_proxy.ComponentToProxy == null)
            {
                return "<b>Component Proxy</b> <color=red>(No type set)";
            }
            
            return $"{_proxy.ComponentToProxy.gameObject.name}.<b>{_proxy.ComponentToProxy.GetType().Name}</b> (Proxy)";
        }
    }
}

#endif