using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace KOOP.SerializeReferenceExtensions.Editor
{
    public class AdvancedTypePopupItem : AdvancedDropdownItem
    {
        public Type Type { get; }

        public AdvancedTypePopupItem(Type type, string name) : base(name)
        {
            Type = type;
        }
    }

    /// <summary>
    /// A type popup with a fuzzy finder.
    /// </summary>
    public class AdvancedTypePopup : AdvancedDropdown
    {
        private const int MaxNamespaceNestingDepth = 16;

        public static void AddTo(AdvancedDropdownItem root, IEnumerable<Type> types, bool allowNull)
        {
            var itemCount = 0;

            if (allowNull)
            {
                // Add null item.
                var nullItem = new AdvancedTypePopupItem(null, TypeSelectorUtility.NullDisplayName)
                {
                    id = itemCount++
                };

                root.AddChild(nullItem);
            }

            var typeArray = types.OrderByType().ToArray();

            // Single namespace if the root has one namespace and the nest is unbranched.
            var isSingleNamespace = true;
            var namespaces = new string[MaxNamespaceNestingDepth];

            foreach (var type in typeArray)
            {
                var splittedTypePath = TypeSelectorUtility.GetSplitTypePath(type);

                if (splittedTypePath.Length <= 1)
                {
                    continue;
                }

                // If they explicitly want sub category, let them do.
                if (TypeSelectorUtility.GetTypeSelectorSettingsAttribute(type) != null)
                {
                    isSingleNamespace = false;
                    break;
                }

                for (var k = 0; (splittedTypePath.Length - 1) > k; k++)
                {
                    var ns = namespaces[k];

                    if (ns == null)
                    {
                        namespaces[k] = splittedTypePath[k];
                    }
                    else if (ns != splittedTypePath[k])
                    {
                        isSingleNamespace = false;
                        break;
                    }
                }

                if (!isSingleNamespace)
                {
                    break;
                }
            }

            // Add type items.
            foreach (var type in typeArray)
            {
                var splittedTypePath = TypeSelectorUtility.GetSplitTypePath(type);

                if (splittedTypePath.Length == 0)
                {
                    continue;
                }

                var parent = root;

                // Add namespace items.
                if (!isSingleNamespace)
                {
                    for (var k = 0; (splittedTypePath.Length - 1) > k; k++)
                    {
                        var foundItem = GetItem(parent, splittedTypePath[k]);

                        if (foundItem != null)
                        {
                            parent = foundItem;
                        }
                        else
                        {
                            var newItem = new AdvancedDropdownItem(splittedTypePath[k])
                            {
                                id = itemCount++
                            };
                            parent.AddChild(newItem);
                            parent = newItem;
                        }
                    }
                }

                // Add type item.
                var item = new AdvancedTypePopupItem(type, ObjectNames.NicifyVariableName(splittedTypePath[splittedTypePath.Length - 1]))
                {
                    id = itemCount++
                };
                
                parent.AddChild(item);
            }
        }

        private static AdvancedDropdownItem GetItem(AdvancedDropdownItem parent, string name)
        {
#if UNITY_6000_5_OR_NEWER
            foreach (var item in parent.childList)
#else
            foreach (var item in parent.children)
#endif
            {
                if (item.name == name)
                {
                    return item;
                }
            }

            return null;
        }

        private static readonly float HeaderHeight = 34f;

        private bool allowNull;
        private Type[] types;

        public event Action<AdvancedTypePopupItem> OnItemSelected;

        public AdvancedTypePopup(IEnumerable<Type> types, bool allowNull, int maxLineCount, AdvancedDropdownState state) : base(state)
        {
            this.types = types.ToArray();
            this.allowNull = allowNull;
            minimumSize = new Vector2(minimumSize.x, EditorGUIUtility.singleLineHeight * maxLineCount + HeaderHeight);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Select Type");
            AddTo(root, types, allowNull);
            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);
            
            if (item is AdvancedTypePopupItem typePopupItem)
            {
                OnItemSelected?.Invoke(typePopupItem);
            }
        }
    }
}