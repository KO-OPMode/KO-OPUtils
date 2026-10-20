using System;

namespace KOOP.SerializeReferenceExtensions
{
    /// <summary>
    /// An attribute that overrides the name of the type displayed in the type selector.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface,
        AllowMultiple = false, Inherited = false)]
    public sealed class TypeSelectorSettingsAttribute : Attribute
    {
        public string Label { get; }

        public int Order { get; }

        public TypeSelectorSettingsAttribute(string label, int order = 0)
        {
            Label = label;
            Order = order;
        }

        private static readonly char[] Separators = { '/', '\\' };

        public string[] GetSplitLabel()
        {
            return !string.IsNullOrWhiteSpace(Label)
                       ? Label.Split(Separators, StringSplitOptions.RemoveEmptyEntries)
                       : Array.Empty<string>();
        }

        public string GetLabelWithoutPath()
        {
            var splittedDisplayName = GetSplitLabel();
            return (splittedDisplayName.Length != 0) ? splittedDisplayName[^1] : null;
        }
    }
}