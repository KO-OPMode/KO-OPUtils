#if UNITY_EDITOR

using System.Reflection;
using UnityEngine;
using UnityEditor;
using KOOP.Attributes;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace KOOPEditor.Attributes
{
    [CustomPropertyDrawer(typeof(ButtonAttribute))]
    public class ButtonAttributeDrawer : PropertyDrawer
    {
        private const float SPACING = 2f;

        private GUIContent _buttonContent = new();
        
        private MethodInfo _cachedMethodInfo = null;
        private ButtonAttribute ButtonAttribute => (ButtonAttribute) attribute;
        private float ButtonHeight => GUI.skin.button.CalcHeight(GUIContent.none, 0);
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true) + ButtonHeight + SPACING;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float buttonHeight = ButtonHeight;
            float propertyHeight = position.height - ButtonHeight - SPACING;
            float buttonOffset = ButtonAttribute.DrawBelowProperty ? propertyHeight + SPACING : 0;
            float propertyOffset = ButtonAttribute.DrawBelowProperty ? 0 : buttonHeight + SPACING;

            _buttonContent.text = ButtonAttribute.Label;
            _buttonContent.tooltip = ButtonAttribute.Tooltip;
            if (GUI.Button(new Rect(position.x, position.y + buttonOffset, position.width, buttonHeight), _buttonContent))
            {
                OnClick(property);
                GUIUtility.ExitGUI();
            }

            EditorGUI.PropertyField(new Rect(position.x, position.y + propertyOffset, position.width, propertyHeight), property, label, true);
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            var button = new Button(() => OnClick(property));
            button.text = ButtonAttribute.Label;
            button.tooltip = ButtonAttribute.Tooltip;
            
            var spacer = new VisualElement();
            spacer.style.height = SPACING;
            
            var propertyField = new PropertyField(property);

            if (ButtonAttribute.DrawBelowProperty)
            {
                root.Add(propertyField);
                root.Add(spacer);
                root.Add(button);
            }
            else
            {
                root.Add(button);
                root.Add(spacer);
                root.Add(propertyField);
            }
            
            return root;
        }

        private void OnClick(SerializedProperty property)
        {
            System.Type targetType = property.serializedObject.targetObject.GetType();
            string methodName = ButtonAttribute.MethodName;

            if (_cachedMethodInfo == null)
            {
                _cachedMethodInfo = targetType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            }

            if (_cachedMethodInfo != null)
            {
                foreach (var target in property.serializedObject.targetObjects)
                {
                    _cachedMethodInfo.Invoke(target, ButtonAttribute.Parameters);
                }
            }
            else
            {
                Debug.LogWarning($"{nameof(ButtonAttribute)}: Unable to find method {methodName} in {targetType.Name}");
            }
        }
    }
}

#endif