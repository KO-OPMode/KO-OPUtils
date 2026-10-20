using System;
using KOOP;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityInternalAccess.Editor;

namespace KOOPEditor
{
    [CustomPropertyDrawer(typeof(SerializedDictionary<,>), true)]
    public class SerializedDictionaryDrawer : PropertyDrawer
    {
        public const string ErrorColorHex = "#ff534a";
        private const int _headerIndentWidth = 4;
        private const int _listMaxHeight = 500;
        private static readonly Color _searchHighlightColor = Color.yellow;
        
        private string _searchFilter = string.Empty;
        private bool HasSearchFilter => !string.IsNullOrEmpty(_searchFilter);
        
        public override VisualElement CreatePropertyGUI(SerializedProperty dictionaryProperty)
        {
            var dataProperty = dictionaryProperty.FindPropertyRelative("_serializedData").Copy();

            var root = new VisualElement();
            
            void BindListItem(VisualElement element, int index)
            {
                if (dataProperty == null || index < 0 || index >= dataProperty.arraySize) return;
                
                var propertyField = element as PropertyField;
                var elementProperty = dataProperty.GetArrayElementAtIndex(index);
                if (elementProperty == null) return;
                
                propertyField.BindProperty(elementProperty);
                
                // Keep the foldout and property's expanded values in sync
                var foldout = propertyField.Q<Foldout>();
                if (foldout != null)
                {
                    foldout.value = elementProperty.isExpanded;
                    var updateExpandedCallback = new EventCallback<ChangeEvent<bool>>(evt =>
                    {
                        elementProperty.isExpanded = evt.newValue;
                    });
                    
                    foldout.RegisterValueChangedCallback(updateExpandedCallback);
                    foldout.userData = updateExpandedCallback;
                    
                    // Expand and colorize elements based on whether it passes our filter
                    if (HasSearchFilter)
                    {
                        var keyProperty = elementProperty.FindPropertyRelative("Key");
                        SerializedDictionaryEntryDrawer.TryGetLabelStringFromKeyProperty(keyProperty, out var labelString, out _);
                        
                        bool passesFilter = labelString.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase);
                        
                        var value = elementProperty.FindPropertyRelative("Value")?.GetValue();
                        if (value is ISerializedDictionaryValue serializedDictionaryValue)
                        {
                            passesFilter |= serializedDictionaryValue.MatchesFilter(_searchFilter);
                        }
                        
                        foldout.SetValueWithoutNotify(passesFilter);
                        if (passesFilter)
                        {
                            foldout.style.color = _searchHighlightColor;
                        }
                        else
                        {
                            // Restore inherited color
                            foldout.style.color = StyleKeyword.Null;
                        }
                    }
                }
            }
        
            void UnbindListItem(VisualElement element, int index)
            {
                var propertyField = element as PropertyField;
                var foldout = propertyField.Q<Foldout>();
                if (foldout != null)
                {
                    var updateExpandedCallback = foldout.userData as EventCallback<ChangeEvent<bool>>;
                    if (updateExpandedCallback != null)
                    {
                        foldout.UnregisterValueChangedCallback(updateExpandedCallback);
                    }
                }
            }

            // Heavily referencing the default list view construction in PropertyField.ConfigureListView()
            var listView = new ListView
            {
                bindItem = BindListItem,
                unbindItem = UnbindListItem,
                selectionType = SelectionType.Multiple,
                showAddRemoveFooter = true,
                showBoundCollectionSize = true,
                showFoldoutHeader = true,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showAlternatingRowBackgrounds = AlternatingRowBackground.None,
                style =
                {
                    maxHeight = _listMaxHeight
                },
            };
            
            // Rebuild our list view after removing items, because otherwise we were seeing cases where
            // certain property drawers (such as NetworkPrefabRefs) would maintain state in odd ways
            // after removing elements
            listView.itemsRemoved += (items) => { EditorApplication.delayCall += () =>
            {
                listView.Rebuild();
            }; };

            listView.headerTitle = dictionaryProperty.displayName;
            listView.tooltip = dictionaryProperty.tooltip;
            
            listView.userData = dataProperty;
            listView.bindingPath = dataProperty.propertyPath;
            listView.viewDataKey = "serialized-dict-" + dictionaryProperty.propertyPath;
            listView.name = listView.viewDataKey;

            // Add custom header within the foldout's default content section
            var content = listView.Q<VisualElement>("unity-content");
            var header = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.RowReverse,
                    marginLeft = _headerIndentWidth,
                }
            };
            content.Insert(0, header);
            
            // Add expand/collapse/sort buttons to header
            void ExpandAll(bool isExpanded)
            {
                for (var i = 0; i < dataProperty.arraySize; i++)
                {
                    var elementProperty = dataProperty.GetArrayElementAtIndex(i);
                    elementProperty.isExpanded = isExpanded;
                }
                
                listView.Rebuild();
            }

            void SortElements()
            {
                if (dictionaryProperty.TryGetValue<ISerializedDictionary>(out var serializedDictionary))
                {
                    Undo.RecordObject(dictionaryProperty.serializedObject.targetObject, $"Sort {dictionaryProperty.displayName}");
                    serializedDictionary.EditorSortSerializedData();
                    dictionaryProperty.serializedObject.Update();
                    EditorUtility.SetDirty(dictionaryProperty.serializedObject.targetObject);
                    listView.Rebuild();
                }
            }
            
            var sortButton = new Button(SortElements)
            {
                text = "Sort",
                tooltip = "Sort the elements of the list"
            };
            header.Add(sortButton);
            
            var expandAllButton = new Button(() => ExpandAll(true))
            {
                text = "Expand all"
            };

            var collapseAllButton = new Button(() => ExpandAll(false))
            {
                text = "Collapse all"
            };

            
            header.Add(collapseAllButton);
            header.Add(expandAllButton);

            // Add search bar to the header
            ToolbarSearchField toolbarSearchField = new()
            {
                style =
                {
                    flexGrow = 1f,
                    flexShrink = 1f
                }
            };

            void HandleSearchChanged()
            {
                _searchFilter = toolbarSearchField.value;
                
                collapseAllButton.enabledSelf = !HasSearchFilter;
                expandAllButton.enabledSelf = !HasSearchFilter;
                
                listView.Rebuild();
            }
            
            toolbarSearchField.RegisterValueChangedCallback(evt => HandleSearchChanged());
            toolbarSearchField.Q<TextField>().isDelayed = true;
            header.Add(toolbarSearchField);
            
            // Add warning box
            var warningBox = new HelpBox(string.Empty, HelpBoxMessageType.Warning);
            warningBox.TrackPropertyValue(dictionaryProperty, RefreshWarningBox);
            
            void RefreshWarningBox(SerializedProperty dictionaryProp)
            {
                if (dictionaryProp.TryGetValue<ISerializedDictionary>(out var serializedDictionary))
                {
                    var hasError = serializedDictionary.TryGetError(out string error);

                    if (hasError)
                    {
                        warningBox.text = error;
                    }

                    warningBox.style.display = hasError ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
            
            RefreshWarningBox(dictionaryProperty);
            
            root.Add(warningBox);
            root.Add(listView);
            
            return root;
        }
    }


    /// <summary>
    /// Custom property drawer for SerializedDictionary entries that, among other things,
    /// lists the entry's key as the title on the foldout element
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializedDictionary<,>.Entry), true)]
    public class SerializedDictionaryEntryDrawer : PropertyDrawer
    {
        public static bool TryGetLabelStringFromKeyProperty(SerializedProperty keyProperty, out string labelString, out bool hasError)
        {
            labelString = null;
            hasError = false;
            
            var keyValue = keyProperty.GetValue();
            if (keyValue != null)
            {
                if (keyValue is ISerializedDictionaryKey element)
                {
                    labelString = element.GetElementLabel();

                    if (element.TryGetError(out var error))
                    {
                        labelString = $"{labelString} ({error})";
                        hasError = true;
                    }
                }
                else
                {
                    labelString = keyValue.ToString();
                }

                return true;
            }

            return false;
        }

        public static bool TryGetLabelSuffixFromValueProperty(SerializedProperty valueProperty, out string labelSuffix, out bool hasError)
        {
            labelSuffix = null;
            hasError = false;
            var value = valueProperty.GetValue();
            if (value != null && value is ISerializedDictionaryValue element)
            {
                labelSuffix = element.GetElementLabelSuffix();

                if (element.TryGetError(out var error))
                {
                    labelSuffix = $"{labelSuffix} {error}";
                    hasError = true;
                }
                
                return !string.IsNullOrEmpty(labelSuffix);
            }

            return false;
        }
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var foldoutField = new Foldout
            {
                text = property.displayName,
                tooltip = property.tooltip,
                style =
                {
                    marginLeft = 16
                }
            };
            
            var keyProperty = property.FindPropertyRelative("Key");
            var keyField = new PropertyField(keyProperty);
            foldoutField.contentContainer.Add(keyField);
            
            var valueProperty = property.FindPropertyRelative("Value");

            // Cache a reference to the first sibling property after we exit the Value block
            var nextProperty = valueProperty.Copy();
            nextProperty.NextVisible(false);
            
            // Step inside the Value block to iterate through all of its properties
            var childProperty = valueProperty.Copy();
            childProperty.NextVisible(true);

            do
            {
                // If we've iterated out of the Value block and into the next sibling, we can stop drawing GUI
                if (childProperty.propertyPath == nextProperty.propertyPath)
                {
                    break;
                }
                
                var childField = new PropertyField(childProperty.Copy());
                foldoutField.contentContainer.Add(childField);
            } while (childProperty.NextVisible(enterChildren: false));
                
            void RefreshElementName(SerializedProperty prop)
            {
                var keyProp = prop.FindPropertyRelative("Key");
                if (keyProp == null)
                {
                    return;
                }
                
                TryGetLabelStringFromKeyProperty(keyProp, out var labelString, out bool hasError);
                if (!string.IsNullOrEmpty(labelString))
                {
                    var valueProp = prop.FindPropertyRelative("Value");
                    if (valueProp != null && TryGetLabelSuffixFromValueProperty(valueProperty, out var labelSuffix, out bool suffixHasError))
                    {
                        labelString = $"{labelString} {labelSuffix}";
                        hasError |= suffixHasError;
                    }

                    if (hasError)
                    {
                        labelString = $"<b><color={SerializedDictionaryDrawer.ErrorColorHex}>{labelString}</color></b>";
                    }
                    
                    foldoutField.text = labelString;
                }
                else
                {
                    foldoutField.text = "<KEY UNSET>";
                }
            }
            
            RefreshElementName(property);
            
            // You'd think propertyField.RegisterValueChangeCallback() would be the way to go here,
            // but it doesn't seem to invoke when child properties get altered.
            foldoutField.TrackPropertyValue(property, RefreshElementName);
            
            return foldoutField;
        }
    }
}