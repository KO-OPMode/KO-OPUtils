#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using KOOP.Attributes;

namespace KOOPEditor.Attributes
{
    [CustomPropertyDrawer(typeof(LayerDropdownAttribute))]
    public class LayerDropdownAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string[] layerNames = new string[32];
            for (int i = 0; i < 32; i++) 
            {
                layerNames[i] = LayerMask.LayerToName(i);
            }

            using (new EditorGUI.PropertyScope(position, label, property))
            {
                if ((attribute as LayerDropdownAttribute).mask)
                {
                    property.intValue = EditorGUI.MaskField(position, label, property.intValue, layerNames);
                }
                else
                {
                    property.intValue = EditorGUI.Popup(position, label.text, property.intValue, layerNames);
                }
            }
        }
    }
}

#endif