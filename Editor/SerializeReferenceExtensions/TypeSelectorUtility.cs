using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace KOOP.SerializeReferenceExtensions.Editor
{
    public static class TypeSelectorUtility
    {
        public const string NullDisplayName = "<null>";

        public static TypeSelectorSettingsAttribute GetTypeSelectorSettingsAttribute(Type type)
        {
            return Attribute.GetCustomAttribute(type, typeof(TypeSelectorSettingsAttribute)) as TypeSelectorSettingsAttribute;
        }

        public static string[] GetSplitTypePath(Type type)
        {
            var typeMenu = GetTypeSelectorSettingsAttribute(type);

            if (typeMenu != null)
            {
                return typeMenu.GetSplitLabel();
            }

            var splitIndex = type.FullName.LastIndexOf('.');
            
            if (splitIndex >= 0)
            {
                return new[] { type.FullName.Substring(0, splitIndex), type.FullName.Substring(splitIndex + 1) };
            }
            else
            {
                return new[] { type.Name };
            }
        }

        public static IEnumerable<Type> OrderByType(this IEnumerable<Type> source)
        {
            return source.OrderBy(type =>
            {
                if (type == null)
                {
                    return int.MinValue;
                }

                return GetTypeSelectorSettingsAttribute(type)?.Order ?? 0;
            }).ThenBy(type =>
            {
                if (type == null)
                {
                    return null;
                }

                return GetTypeSelectorSettingsAttribute(type)?.Label ?? type.Name;
            });
        }
    }
}