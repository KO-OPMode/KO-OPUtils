using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KOOP.SerializeReferenceExtensions.Editor
{
    /// <summary>
    /// Provides utilities for filtering type tags defined via <see cref="TypeTagsAttribute" />
    /// for use by <see cref="TypeTagsFilterAttribute" /> in <see cref="SubclassSelectorDrawer" />.
    /// </summary>
    /// <owner>Lazlo</owner>
    public static class TypeTagFilteringUtility
    {
        /// <summary>
        /// The string comparer to use when comparing type tags.
        /// </summary>
        public static StringComparer TagComparer => StringComparer.InvariantCultureIgnoreCase;

        /// <summary>
        /// Returns a collated allowlist and blocklist of type tags combining all those defined
        /// in the <see cref="TypeTagsFilterAttribute" /> of a serialized property and all of its ancestors.
        /// </summary>
        public static void GetCollatedTypeTagFilters(SerializedProperty property, out IReadOnlyCollection<string> allowlist, out IReadOnlyCollection<string> blocklist)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            var allowlistSet = new HashSet<string>(TagComparer);
            var blocklistSet = new HashSet<string>(TagComparer);

            foreach (var filterAttribute in property.GetAttributesInParent<TypeTagsFilterAttribute>())
            {
                if (filterAttribute.Allowlist != null)
                {
                    allowlistSet.UnionWith(filterAttribute.Allowlist);
                }

                if (filterAttribute.Blocklist != null)
                {
                    blocklistSet.UnionWith(filterAttribute.Blocklist);
                }
            }

            allowlist = allowlistSet;
            blocklist = blocklistSet;
        }

        /// <summary>
        /// Returns a collated list of type tags for a given type and all of its ancestors.
        /// </summary>
        public static IReadOnlyCollection<string> GetCollatedTypeTags(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var attributes = type.GetCustomAttributes(typeof(TypeTagsAttribute), inherit: true).Cast<TypeTagsAttribute>();
            var tags = new HashSet<string>(TagComparer);

            foreach (var attribute in attributes)
            {
                if (attribute.Tags != null)
                {
                    tags.UnionWith(attribute.Tags);
                }
            }

            return tags;
        }

        /// <summary>
        /// Returns whether a given type should be filtered in given a allowlist and blocklist of type tags.
        /// </summary>
        public static bool FilterPredicate(Type type, IReadOnlyCollection<string> allowlist, IReadOnlyCollection<string> blocklist)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var tags = GetCollatedTypeTags(type);
            
            if (allowlist != null && allowlist.Count > 0)
            {
                if (!tags.Any(allowlist.Contains))
                {
                    return false;
                }
            }

            if (blocklist != null && blocklist.Count > 0)
            {
                if (tags.Any(blocklist.Contains))
                {
                    return false;
                }
            }

            return true;
        }
    }
}