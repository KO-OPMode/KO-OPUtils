using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace KOOP.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class SeparatorAttribute : PropertyAttribute
    {
        private readonly Color baseColor = new Color(0f, 0f, 0f, 0.2f);
        private readonly float baseHeight = 1f;
        private readonly float baseWidthPadding = 1f;

        public SeparatorAttribute()
        {
            this.color = baseColor;
            this.height = baseHeight;
            this.widthPadding = baseWidthPadding;
        }

        public SeparatorAttribute(Color color)
        {
            this.color = color;
            this.height = baseHeight;
            this.widthPadding = baseWidthPadding;
        }

        public Color color;
        public float height;
        public float widthPadding;
    }
}