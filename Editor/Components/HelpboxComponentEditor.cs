#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using KOOP;

namespace KOOPEditor
{
    [CustomEditor(typeof(HelpboxComponent))]
    public class HelpboxComponentEditor : Editor
    {
        private static class Styles
        {
            public static GUIStyle TextAreaStyle;

            static Styles()
            {
                TextAreaStyle = new GUIStyle(EditorStyles.textArea)
                {
                    wordWrap = true,
                };
            }
        }
        
        private bool _isEditing;
        private string _text;
        
        public override void OnInspectorGUI()
        {
            HelpboxComponent helpbox = target as HelpboxComponent;

            if (!_isEditing)
            {
                if (string.IsNullOrWhiteSpace(helpbox.Content))
                {
                    EditorGUILayout.LabelField("No help text set!");
                    if (GUILayout.Button("Start Editing"))
                    {
                        StartEditing(helpbox);
                    }
                }
                else
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorStyles.helpBox.richText = true;
                        EditorGUILayout.HelpBox(helpbox.Content, MessageType.Info);

                        var iconContent = EditorGUIUtility.IconContent("editicon.sml");
                        iconContent.tooltip = "Edit help text";
                        if (GUILayout.Button(iconContent, EditorStyles.iconButton, GUILayout.Width(EditorGUIUtility.singleLineHeight)))
                        {
                            StartEditing(helpbox);
                        }   
                    }
                }
            }
            else
            {
                _text = EditorGUILayout.TextArea(_text, Styles.TextAreaStyle, GUILayout.MinHeight(EditorGUIUtility.singleLineHeight * 3f));

                if (GUILayout.Button("Finish Editing"))
                {
                    FinishEditing(helpbox);
                }
            }
        }

        private void StartEditing(HelpboxComponent helpbox)
        {
            _text = helpbox.Content;
            _isEditing = true;
        }

        private void FinishEditing(HelpboxComponent helpbox)
        {
            helpbox.Content = _text;
            EditorUtility.SetDirty(helpbox);
            
            _isEditing = false;
            _text = string.Empty;
        }
    }
}

#endif