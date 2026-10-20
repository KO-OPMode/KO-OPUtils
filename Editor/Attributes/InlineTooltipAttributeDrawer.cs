#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using KOOP.Attributes;

namespace KOOPEditor.Attributes
{
    [CustomPropertyDrawer(typeof(InlineTooltipAttribute))]
    public class InlineTooltipAttributeDrawer : PropertyDrawer
    {
        const float padding = 5f;
        const float paddingWhenExpanded = 5f;
        const float indent = 0f;
        float buttonWidth => EditorGUIUtility.singleLineHeight;

        InlineTooltipAttribute tooltip => attribute as InlineTooltipAttribute;
        bool expanded = false;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!expanded)
            {
                return base.GetPropertyHeight(property, label);
            }
            else
            {
                return base.GetPropertyHeight(property, label) + GetTooltipHeight() + (padding * 2f) + (paddingWhenExpanded * 2f);
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                if (expanded)
                {
                    EditorGUI.LabelField(position, string.Empty, BackgroundStyle);
                }

                position.width -= buttonWidth;

                Rect buttonRect = new Rect(position.x + position.width, position.y, buttonWidth, buttonWidth);

                if (expanded)
                {
                    position.x += paddingWhenExpanded;
                    position.y += paddingWhenExpanded;
                    position.width -= paddingWhenExpanded * 2f;
                    position.height -= paddingWhenExpanded * 2f;
                }

                if (GUI.Button(buttonRect, Resources.Load<Texture>("InlineTooltip"), ButtonStyle))
                {
                    expanded = !expanded;
                }

                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, base.GetPropertyHeight(property, label)), property);

                if (expanded)
                {
                    Rect r = new Rect(position.x + indent, position.y + padding + base.GetPropertyHeight(property, label), position.width - indent, GetTooltipHeight());
                    EditorGUI.LabelField(r, tooltip.tooltip, TooltipStyle);
                }
            }
        }

        private float GetTooltipHeight()
        {
            return TooltipStyle.CalcHeight(new GUIContent(tooltip.tooltip), EditorGUIUtility.currentViewWidth - (indent) - (padding * 2f) - (paddingWhenExpanded * 2f));
        }

        GUIStyle _tooltipStyle;
        GUIStyle TooltipStyle
        {
            get
            {
                if (_tooltipStyle == null)
                {
                    _tooltipStyle = new GUIStyle(EditorStyles.label);
                    _tooltipStyle.normal.textColor = new Color(_tooltipStyle.normal.textColor.r, _tooltipStyle.normal.textColor.g, _tooltipStyle.normal.textColor.b, 0.5f);
                    _tooltipStyle.richText = true;
                    _tooltipStyle.wordWrap = true;
                }

                return _tooltipStyle;
            }
        }

        GUIStyle _buttonStyle;
        GUIStyle ButtonStyle
        {
            get
            {
                if (_buttonStyle == null)
                {
                    _buttonStyle = new GUIStyle(EditorStyles.iconButton);
                }

                return _buttonStyle;
            }
        }

        GUIStyle _backgroundStyle;
        GUIStyle BackgroundStyle
        {
            get
            {
                if (_backgroundStyle == null)
                {
                    _backgroundStyle = new GUIStyle(EditorStyles.helpBox);
                }

                return _backgroundStyle;
            }
        }
    }
}

#endif