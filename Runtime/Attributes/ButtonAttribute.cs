using UnityEngine;

namespace KOOP.Attributes
{
    /// <summary>
    /// Attribute that draws a clickable button in the inspector that invokes a specified method.
    /// </summary>
    [System.AttributeUsage( System.AttributeTargets.Field,  AllowMultiple = true )]
    public class ButtonAttribute : PropertyAttribute
    {
        public readonly string Label;
        public readonly string MethodName;
        public readonly string Tooltip;
        public readonly bool DrawBelowProperty;
        public readonly object[] Parameters;
        
        public ButtonAttribute(string label, string methodName, string tooltip = null, bool drawBelowProperty = false, params object[] parameters) : base(applyToCollection: true)
        {
            Label = label;
            MethodName = methodName;
            Tooltip = tooltip;
            DrawBelowProperty = drawBelowProperty;
            Parameters = parameters;
        }
    }
}
