#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using KOOP.Attributes;

namespace KOOPEditor.Attributes
{
    [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxAttributeDrawer : DecoratorDrawer
    {
        // Cache data we're not allowed to access outside of OnGUI()
        private GUIStyle _helpBoxStyle = null;
        private float _currentViewWidth = 0f;
        
        public override float GetHeight() 
        {
            var helpBoxAttribute = attribute as HelpBoxAttribute;
            if (helpBoxAttribute == null) return base.GetHeight();
            if (_helpBoxStyle == null) return base.GetHeight();

            var guiContent = new GUIContent(helpBoxAttribute.text);
            var calculatedHeight = _helpBoxStyle.CalcHeight(guiContent, _currentViewWidth);

            return Mathf.Max(40f, calculatedHeight + 4);
        }
        
        public override void OnGUI(Rect position) 
        {
            if (_helpBoxStyle == null && GUI.skin != null)
            {
                _helpBoxStyle = GUI.skin.GetStyle("helpbox");
            }
            
            _currentViewWidth = EditorGUIUtility.currentViewWidth;
            
            var helpBoxAttribute = attribute as HelpBoxAttribute;
            if (helpBoxAttribute == null) return;
            EditorGUI.HelpBox(position, helpBoxAttribute.text, GetMessageType(helpBoxAttribute.messageType));
        }
        
        private MessageType GetMessageType(HelpBoxMessageType helpBoxMessageType) 
        {
            switch (helpBoxMessageType) {
                default:
                case HelpBoxMessageType.None: return MessageType.None;
                case HelpBoxMessageType.Info: return MessageType.Info;
                case HelpBoxMessageType.Warning: return MessageType.Warning;
                case HelpBoxMessageType.Error: return MessageType.Error;
            }
        }
    }
}

#endif