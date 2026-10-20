using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityInternalAccess.Editor;

namespace KOOP.SerializeReferenceExtensions.Editor
{
    public static class SerializedPropertyExtensions
    {
        public static IEnumerable<SerializedProperty> GetChildProperties(this SerializedProperty parent, int depth = 1)
        {
            parent = parent.Copy();

            var depthOfParent = parent.depth;
            var enumerator = parent.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current is not SerializedProperty childProperty)
                {
                    continue;
                }

                if (childProperty.depth > (depthOfParent + depth))
                {
                    continue;
                }

                yield return childProperty.Copy();
            }
        }

        public static IEnumerable<TAttribute> GetAttributesInParent<TAttribute>(this SerializedProperty property) where TAttribute : Attribute
        {
            while (property != null)
            {
                foreach (var attribute in property.GetFieldInfo(out _).GetCustomAttributes(typeof(TAttribute), inherit: true).Cast<TAttribute>())
                {
                    yield return attribute;
                }

                property = property.GetParentProperty();
            }
        }
    }
}