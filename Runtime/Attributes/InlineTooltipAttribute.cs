using UnityEngine;

public class InlineTooltipAttribute : PropertyAttribute
{
    public InlineTooltipAttribute(string tooltip)
    {
        this.tooltip = tooltip;
    }

    public string tooltip;
}