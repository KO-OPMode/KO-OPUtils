using System;
using UnityEngine;

namespace KOOP.SerializeReferenceExtensions
{
    /// <summary>
    /// Attribute to specify the type of the field serialized by the SerializeReference attribute in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class SubclassSelectorAttribute : PropertyAttribute
    {
        public bool AllowNull { get; set; } = true;
        
        public bool UseToStringAsLabel { get; set; }

        public bool ShowDropdown { get; set; } = true;
    }
}