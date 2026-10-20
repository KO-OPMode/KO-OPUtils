using System;
using System.Text;
using KOOP;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityInternalAccess.Editor;

namespace KOOPEditor
{
    [CustomPropertyDrawer(typeof(SerializedGuid))]
    public class SerializedGuidPropertyDrawer : PropertyDrawer
    {
        private const string ShowBase64String = "SerializedGuid_ShowBase64";
        
        private static bool ShowBase64
        {
            get => EditorPrefs.GetBool(ShowBase64String);
            set => EditorPrefs.SetBool(ShowBase64String, value);
        }
        
#region UI Toolkit
        
        private const float INDENT = 2f;
        private const float ELEMENT_SPACING = 4f;
        
        private VisualElement _contentRoot;
        private HelpBox _helpBox;
        private TextField _textField;

        public override VisualElement CreatePropertyGUI(SerializedProperty guidProperty)
        {
            var root = new VisualElement
            {
                style =
                {
                    paddingLeft = INDENT,
                }
            };

            // Content section
            _contentRoot = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1f,
                }
            };

            root.Add(_contentRoot);

            var propertyLabel = new Label(guidProperty.displayName)
            {
                style =
                {
                    flexGrow = 0f,
                    alignSelf = Align.Center
                }
            };
            _contentRoot.Add(propertyLabel);

            _textField = new TextField
            {
                style =
                {
                    flexGrow = 1f,
                    marginLeft = ELEMENT_SPACING
                },

                isReadOnly = true
            };
            
            _contentRoot.Add(_textField);

            var toggle = new Toggle("Show base64")
            {
                style =
                {
                    flexGrow = .1f,
                    flexDirection = FlexDirection.RowReverse, // Put the toggle before the label
                    marginLeft = ELEMENT_SPACING
                }
            };

            toggle.RegisterValueChangedCallback(evt =>
            {
                ShowBase64 = evt.newValue;
                RefreshElements(guidProperty);
            });

            _contentRoot.Add(toggle);

            // Warning
            _helpBox = new HelpBox
            {
                style =
                {
                    display = DisplayStyle.None
                }
            };
            
            root.TrackSerializedObjectValue(guidProperty.serializedObject, so => RefreshElements(guidProperty));
            RefreshElements(guidProperty);
            
            return root;
        }

        private void RefreshElements(SerializedProperty guidProperty)
        {
            var bytesProperty = guidProperty.FindPropertyRelative("_bytes").Copy();
            var byteCount = bytesProperty.arraySize;

            if (guidProperty.prefabOverride)
            {
                _contentRoot.style.unityFontStyleAndWeight = FontStyle.Bold;
            }
            else
            {
                _contentRoot.style.unityFontStyleAndWeight = StyleKeyword.Null;
            }

            if (byteCount == 16)
            {
                var serializedGuid = (SerializedGuid)guidProperty.GetValue();
                if (ShowBase64)
                {
                    _textField.value = serializedGuid.ToBase64();
                }
                else
                {
                    _textField.value = serializedGuid.ToString();
                }

                _contentRoot.style.display = DisplayStyle.Flex;
            }
            else
            {
                _helpBox.text = $"Incorrect number of bytes found: {byteCount}";
                _helpBox.messageType = HelpBoxMessageType.Error;
                _contentRoot.style.display = DisplayStyle.None;
            }
        }
        
#endregion
#region IMGUI

        private const int MIN_LABEL_WIDTH = 48;
        private const int BASE64_TOGGLE_WIDTH = 102;
        
        private static readonly GUIContent Base64Content = new GUIContent("Show base64");
        private static readonly GUIStyle BoldTextFieldStyle = new GUIStyle(EditorStyles.textField)
        {
            fontStyle = FontStyle.Bold
        };
        private static readonly GUIStyle BoldToggleStyle = new GUIStyle(EditorStyles.toggle)
        {
            fontStyle = FontStyle.Bold
        };
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        
        public override void OnGUI(Rect fullRect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(fullRect, label, property);
            DrawSerializedGuidDrawer(fullRect, property, label);
            EditorGUI.EndProperty();
        }

        private static void GetRects(Rect fullRect, GUIContent label, out Rect contentRect, out Rect prefixRect, out Rect guidRect, out Rect toggleRect)
        {
            contentRect = EditorGUI.IndentedRect(fullRect);
            
            prefixRect = new Rect(contentRect);
            prefixRect.width = Mathf.Max(EditorStyles.label.CalcSize(label).x, MIN_LABEL_WIDTH);
                
            guidRect = new Rect(contentRect);
            guidRect.x = contentRect.x + prefixRect.width + ELEMENT_SPACING;
            guidRect.width = contentRect.width - prefixRect.width - BASE64_TOGGLE_WIDTH - (2 * ELEMENT_SPACING);
            
            toggleRect = new Rect(contentRect);
            toggleRect.width = BASE64_TOGGLE_WIDTH;
            toggleRect.x = contentRect.x + prefixRect.width + guidRect.width + (2 * ELEMENT_SPACING);
        }

        private void DrawSerializedGuidDrawer(Rect fullRect, SerializedProperty property, GUIContent label)
        {
            var bytesProperty = property.FindPropertyRelative("_bytes");
            var byteCount = bytesProperty.arraySize;
            var indent = EditorGUI.indentLevel;
            var labelWidth = EditorGUIUtility.labelWidth;
            GetRects(fullRect, label, out var contentRect, out var prefixRect, out var guidRect, out var toggleRect);
            EditorGUI.indentLevel = 0;
            
            if (byteCount == 16)
            {
                var serializedGuid = (SerializedGuid) property.GetValue();
                var guid = serializedGuid.Value;

                string guidString;
                if (!ShowBase64)
                {
                    guidString = guid.ToString();
                }
                else
                {
                    var guidBytes = guid.ToByteArray();
                    guidString = Convert.ToBase64String(guidBytes);
                }

                EditorGUI.PrefixLabel(
                    prefixRect, 
                    new GUIContent(property.displayName, property.tooltip), 
                    property.prefabOverride ? EditorStyles.boldLabel : EditorStyles.label);
                
                EditorGUI.SelectableLabel(
                    guidRect, 
                    guidString, 
                    property.prefabOverride ? BoldTextFieldStyle : EditorStyles.textField);
                
                // Always let users toggle whether to see the GUID in Base 64
                var guiWasEnabled = GUI.enabled;
                GUI.enabled = true;
                
                ShowBase64 = EditorGUI.ToggleLeft(
                    toggleRect, 
                    Base64Content, 
                    ShowBase64, 
                    property.prefabOverride ? EditorStyles.boldLabel : EditorStyles.label);
                
                GUI.enabled = guiWasEnabled;
            }
            else
            {
                EditorGUI.HelpBox(contentRect, $"Incorrect number of bytes found: {byteCount}", MessageType.Error);
            }

            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth = labelWidth;
        }

#endregion
    }
}

