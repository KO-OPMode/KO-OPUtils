#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using KOOP.Attributes;

namespace KOOPEditor.Attributes
{
    [CustomPropertyDrawer(typeof(SeparatorAttribute))]
    public class SeparatorAttributeDecoratorDrawer : DecoratorDrawer
    {
        const float padding = 4f;

        public override float GetHeight()
        {
            return (attribute as SeparatorAttribute).height + (padding * 2f);
        }

        public override void OnGUI(Rect position)
        {
            var separatorAttribute = (attribute as SeparatorAttribute);
            position.height -= (padding * 2f);
            position.y += padding;
            position.width -= (separatorAttribute.widthPadding * 2f);
            position.x += separatorAttribute.widthPadding;

            EditorGUI.DrawRect(position, (attribute as SeparatorAttribute).color);
        }
    }
}

#endif