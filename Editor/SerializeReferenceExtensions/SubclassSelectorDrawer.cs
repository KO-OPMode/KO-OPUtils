using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityInternalAccess.Editor;

namespace KOOP.SerializeReferenceExtensions.Editor
{
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        private const int MaxTypePopupLineCount = 13;

        private static readonly GUIContent NullDisplayNameContent = new(TypeSelectorUtility.NullDisplayName);
        private static readonly GUIContent IsNotManagedReferenceLabel = new("The property type is not a managed reference.");

        private readonly Dictionary<string, GUIContent> _typeToNameContent = new();

        private SerializedProperty targetProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                // Render label first to avoid label overlap for lists
                var foldoutLabelRect = new Rect(position);
                foldoutLabelRect.height = EditorGUIUtility.singleLineHeight;

                // NOTE: IndentedRect should be disabled as it causes extra indentation.
                //foldoutLabelRect = EditorGUI.IndentedRect(foldoutLabelRect);
                var popupPosition = EditorGUI.PrefixLabel(foldoutLabelRect, label);

                // Override the label text with the ToString() of the managed reference.
                var subclassSelectorAttribute = (SubclassSelectorAttribute)attribute;
                
                if (subclassSelectorAttribute.UseToStringAsLabel && !property.hasMultipleDifferentValues)
                {
                    var managedReferenceValue = property.managedReferenceValue;
                    
                    if (managedReferenceValue != null)
                    {
                        label.text = managedReferenceValue.ToString();
                    }
                }

                // Draw the subclass selector popup.
                if (subclassSelectorAttribute.ShowDropdown)
                {
                    if (EditorGUI.DropdownButton(popupPosition, GetTypeName(property), FocusType.Keyboard))
                    {
                        var popup = GetTypePopup(property);
                        targetProperty = property;
                        popup.Show(popupPosition);
                    }
                }
                // Auto set null value to defined property type if it does not allow null 
                else if (!subclassSelectorAttribute.AllowNull && property.managedReferenceValue == null)
                {
                    var defaultType = ManagedReferenceUtility.GetType(property.managedReferenceFieldTypename);
                    foreach (var targetObject in property.serializedObject.targetObjects)
                    {
                        var individualObject = new SerializedObject(targetObject);
                        var individualProperty = individualObject.FindProperty(property.propertyPath);
                        var obj = individualProperty.SetManagedReference(defaultType);
                        individualProperty.isExpanded = (obj != null);

                        individualObject.ApplyModifiedProperties();
                        individualObject.Update();
                    }
                }

                // Draw the foldout.
                if (!string.IsNullOrEmpty(property.managedReferenceFullTypename))
                {
                    var foldoutRect = new Rect(position);
                    foldoutRect.height = EditorGUIUtility.singleLineHeight;
                    property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none, true);
                }

                // Draw property if expanded.
                if (property.isExpanded)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        // Check if a custom property drawer exists for this type.
                        var customDrawer = GetCustomPropertyDrawer(property);
                        if (customDrawer != null)
                        {
                            // Draw the property with custom property drawer.
                            var indentedRect = position;
                            var foldoutDifference =
                                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                            indentedRect.height = customDrawer.GetPropertyHeight(property, label);
                            indentedRect.y += foldoutDifference;
                            customDrawer.OnGUI(indentedRect, property, label);
                        }
                        else
                        {
                            // Draw the properties of the child elements.
                            // NOTE: In the following code, since the foldout layout isn't working properly, I'll iterate through the properties of the child elements myself.
                            // EditorGUI.PropertyField(position, property, GUIContent.none, true);

                            var childPosition = position;
                            childPosition.y += EditorGUIUtility.singleLineHeight +
                                               EditorGUIUtility.standardVerticalSpacing;
                            foreach (var childProperty in property.GetChildProperties())
                            {
                                var height = EditorGUI.GetPropertyHeight(
                                    childProperty, new GUIContent(childProperty.displayName, childProperty.tooltip),
                                    true);
                                childPosition.height = height;
                                EditorGUI.PropertyField(childPosition, childProperty, true);

                                childPosition.y += height + EditorGUIUtility.standardVerticalSpacing;
                            }
                        }
                    }
                }
            }
            else
            {
                EditorGUI.LabelField(position, label, IsNotManagedReferenceLabel);
            }

            EditorGUI.EndProperty();
        }

        private PropertyDrawer GetCustomPropertyDrawer(SerializedProperty property)
        {
            var propertyType = ManagedReferenceUtility.GetType(property.managedReferenceFullTypename);
            
            if (propertyType != null && PropertyDrawerCache.TryGetPropertyDrawer(propertyType, out var drawer))
            {
                return drawer;
            }

            return null;
        }

        private AdvancedTypePopup GetTypePopup(SerializedProperty property)
        {
            // Cache this string. This property internally call Assembly.GetName, which result in a large allocation.
            var managedReferenceFieldTypename = property.managedReferenceFieldTypename;

            var subclassSelectorAttribute = (SubclassSelectorAttribute)attribute;
            
            TypeTagFilteringUtility.GetCollatedTypeTagFilters(property, out var allowlist, out var blocklist);
            
            var propertyType = ManagedReferenceUtility.GetType(managedReferenceFieldTypename);
            var implementingTypes = TypeImplementationSearch.GetImplementingTypes(propertyType)
                .Where(t => TypeTagFilteringUtility.FilterPredicate(t, allowlist, blocklist));

            var popup = new AdvancedTypePopup(
                implementingTypes,
                subclassSelectorAttribute.AllowNull,
                MaxTypePopupLineCount,
                new AdvancedDropdownState()
            );

            popup.OnItemSelected += item =>
            {
                var type = item.Type;

                // Apply changes to individual serialized objects.
                foreach (var targetObject in targetProperty.serializedObject.targetObjects)
                {
                    var individualObject = new SerializedObject(targetObject);
                    var individualProperty = individualObject.FindProperty(targetProperty.propertyPath);
                    var obj = individualProperty.SetManagedReference(type);
                    individualProperty.isExpanded = (obj != null);

                    individualObject.ApplyModifiedProperties();
                    individualObject.Update();
                }
            };

            return popup;
        }

        private GUIContent GetTypeName(SerializedProperty property)
        {
            // Cache this string.
            var managedReferenceFullTypename = property.managedReferenceFullTypename;

            if (string.IsNullOrEmpty(managedReferenceFullTypename))
            {
                return NullDisplayNameContent;
            }

            if (_typeToNameContent.TryGetValue(managedReferenceFullTypename, out var cachedTypeName))
            {
                return cachedTypeName;
            }

            var type = ManagedReferenceUtility.GetType(managedReferenceFullTypename);
            string typeName = null;

            var typeMenu = TypeSelectorUtility.GetTypeSelectorSettingsAttribute(type);

            if (typeMenu != null)
            {
                typeName = typeMenu.GetLabelWithoutPath();

                if (!string.IsNullOrWhiteSpace(typeName))
                {
                    typeName = ObjectNames.NicifyVariableName(typeName);
                }
            }

            if (string.IsNullOrWhiteSpace(typeName))
            {
                typeName = ObjectNames.NicifyVariableName(type.Name);
            }

            var result = new GUIContent(typeName);
            _typeToNameContent.Add(managedReferenceFullTypename, result);
            return result;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var customDrawer = GetCustomPropertyDrawer(property);

            if (customDrawer != null)
            {
                return property.isExpanded
                           ? EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                             customDrawer.GetPropertyHeight(property, label)
                           : EditorGUIUtility.singleLineHeight;
            }
            else
            {
                return property.isExpanded
                           ? EditorGUI.GetPropertyHeight(property, true)
                           : EditorGUIUtility.singleLineHeight;
            }
        }
    }
}