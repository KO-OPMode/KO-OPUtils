using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SettingsManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using UnityInternalAccess.Editor;

namespace KOOPEditor
{
    /// <summary>
    /// Additional helper methods for constructing editor UIs programatically
    /// </summary>
    /// <owner>jonagill</owner>
    public static class EditorGUIHelpers
    {
        public const float IndentWidth = 15f;
        private const float MinInputWidth = 32f;

        #region UI Toolkit extensions

        public static void Indent(this VisualElement element, int indentCount = 1)
        {
            element.style.paddingLeft = element.style.paddingLeft.value.value + (IndentWidth * indentCount);
        }

        /// <summary>
        /// Create a <see cref="Toggle"/> element that is bound to the value of a <see cref="UserSetting{T}"/> editor preference.
        /// </summary>
        public static Toggle CreatePreferenceToggle(
            string label,
            string tooltip,
            UserSetting<bool> userSetting,
            Action<bool> toggleRefreshed = null,
            bool toggleOnRight = false)
        {
            return CreateToggle(
                label,
                tooltip,
                () => userSetting.value,
                v => userSetting.value = v,
                toggleRefreshed,
                toggleOnRight);
        }

        /// <summary>
        /// Create a <see cref="Toggle"/> element that uses the provided callbacks to get and set its value.
        /// </summary>
        public static Toggle CreateToggle(
            string label,
            string tooltip,
            Func<bool> getToggled,
            Action<bool> setToggled,
            Action<bool> toggleRefreshed = null,
            bool toggleOnRight = false)
        {
            Assert.IsFalse(string.IsNullOrEmpty(label),
                "Toggles created without a label will be instantiated in an undefined state.");

            var toggle = new Toggle()
            {
                text = label,
                tooltip = tooltip
            };

            ConfigureToggle(toggle, getToggled, setToggled, toggleRefreshed, toggleOnRight);

            return toggle;
        }

        private static void ConfigureToggle(
            Toggle toggle,
            Func<bool> getToggled,
            Action<bool> setToggled,
            Action<bool> toggleRefreshed = null,
            bool toggleOnRight = false)
        {
            void RefreshToggleState()
            {
                toggle.value = getToggled();
                toggleRefreshed?.Invoke(toggle.value);
            }

            toggle.Q<VisualElement>(className: Toggle.inputUssClassName).style.flexDirection = toggleOnRight ? FlexDirection.RowReverse :  FlexDirection.Row;

            toggle.RegisterValueChangedCallback(changeEvent =>
            {
                setToggled.Invoke(changeEvent.newValue);
                RefreshToggleState();

                // Stop propagation to fix a bug where un-checking a preference toggle
                // contained in a foldout would also close the foldout
                changeEvent.StopPropagation();
            });

            RefreshToggleState();
        }

        /// <summary>
        /// Create a <see cref="IntegerField"/> element that is bound to the value of a <see cref="UserSetting{T}"/> editor preference.
        /// </summary>
        public static IntegerField CreatePreferenceIntField(
            string label,
            string tooltip,
            UserSetting<int> userSetting,
            Action<int> valueChanged = null,
            int? minValue = null,
            int? maxValue = null)
        {
            return CreateIntField(
                label,
                tooltip,
                () => userSetting.value,
                v => userSetting.value = v,
                valueChanged,
                minValue,
                maxValue);
        }

        /// <summary>
        /// Create a <see cref="IntegerField"/> element that uses the provided callbacks to get and set its value.
        /// </summary>
        public static IntegerField CreateIntField(
            string label,
            string tooltip,
            Func<int> getValue,
            Action<int> setValue,
            Action<int> valueChanged = null,
            int? minValue = null,
            int? maxValue = null)
        {
            var intField = new IntegerField()
            {
                label = label,
                tooltip = tooltip
            };

            var textElement = intField.Q("unity-text-input");
            textElement.style.minWidth = MinInputWidth;

            int OnValueChanged(int value)
            {
                if (minValue.HasValue)
                {
                    value = Mathf.Max(value, minValue.Value);
                }

                if (maxValue.HasValue)
                {
                    value = Mathf.Min(value, maxValue.Value);
                }

                intField.value = value;
                valueChanged?.Invoke(intField.value);

                return value;
            }

            intField.RegisterValueChangedCallback(changeEvent =>
            {
                var value = OnValueChanged(changeEvent.newValue);
                setValue.Invoke(value);
            });

            OnValueChanged(getValue());

            return intField;
        }

        /// <summary>
        /// Create an <see cref="EnumField"/> element that is bound to the value of a <see cref="UserSetting{T}"/> editor preference.
        /// </summary>
        public static EnumField CreatePreferenceEnumField<T>(
            string label,
            string tooltip,
            UserSetting<T> userSetting,
            Action<T> valueChanged = null) where T : Enum
        {
            return CreateEnumField(
                label,
                tooltip,
                () => userSetting.value,
                v => userSetting.value = v,
                valueChanged);
        }

        /// <summary>
        /// Create an <see cref="EnumField"/> element that uses the provided callbacks to get and set its value.
        /// </summary>
        public static EnumField CreateEnumField<T>(
            string label,
            string tooltip,
            Func<T> getValue,
            Action<T> setValue,
            Action<T> valueChanged = null) where T : Enum
        {
            var enumField = new EnumField(getValue())
            {
                label = label,
                tooltip = tooltip
            };

            var dropdownElement = enumField.Q(className: "unity-enum-field__input");
            dropdownElement.style.minWidth = MinInputWidth;

            T OnValueChanged(T value)
            {
                enumField.value = value;
                valueChanged?.Invoke(value);

                return value;
            }

            enumField.RegisterValueChangedCallback(changeEvent =>
            {
                var value = OnValueChanged((T)changeEvent.newValue);
                setValue.Invoke(value);
            });

            OnValueChanged(getValue());

            return enumField;
        }
        
        /// <summary>
        /// Create an <see cref="EnumField"/> element that uses the provided non-generic callbacks to get and set its value.
        /// </summary>
        public static EnumField CreateEnumField(
            string label, 
            string tooltip, 
            Func<Enum> getValue,
            Action<Enum> setValue)
        {
            var enumField = new EnumField(label, getValue())
            {
                tooltip = tooltip,
            };
            enumField.RegisterValueChangedCallback(evt => setValue(evt.newValue));
            return enumField;
        }
        
        /// <summary>
        /// Create a <see cref="EnumFlagsField"/> element that is bound to the value of a <see cref="UserSetting{T}"/> editor preference.
        /// </summary>
        public static EnumFlagsField CreatePreferenceEnumFlagsField<T>(
            string label,
            string tooltip,
            UserSetting<T> userSetting,
            Action<T> valueChanged = null) where T : Enum
        {
            return CreateEnumFlagsField(
                label,
                tooltip,
                () => userSetting.value,
                v => userSetting.value = v,
                valueChanged);
        }

        /// <summary>
        /// Create an <see cref="EnumFlagsField"/> element that uses the provided callbacks to get and set its value.
        /// </summary>
        public static EnumFlagsField CreateEnumFlagsField<T>(
            string label,
            string tooltip,
            Func<T> getValue,
            Action<T> setValue,
            Action<T> valueChanged = null) where T : Enum
        {
            var enumField = new EnumFlagsField(getValue())
            {
                label = label,
                tooltip = tooltip
            };

            T OnValueChanged(T value)
            {
                enumField.value = value;
                valueChanged?.Invoke(value);

                return value;
            }

            enumField.RegisterValueChangedCallback(changeEvent =>
            {
                var value = OnValueChanged((T)changeEvent.newValue);
                setValue.Invoke(value);
            });

            OnValueChanged(getValue());

            return enumField;
        }

        /// <summary>
        /// Create a foldout that saves its open/closed state automatically in <see cref="EditorPrefs"/> 
        /// </summary>
        /// <param name="preferenceKeyPrefix">A prefix value to disambiguate our saved state in <see cref="EditorPrefs"/>. Gets combined with label to create our full key.</param>
        /// <param name="label">The visual label for this foldout. Also used to construct our <see cref="EditorPrefs"/> key.</param>
        public static Foldout CreateSavedFoldout(
            string preferenceKeyPrefix,
            string label,
            string tooltip = null,
            bool defaultOpen = false)
        {
            var foldout = new Foldout()
            {
                text = label,
                tooltip = tooltip
            };

            var prefKey = $"{preferenceKeyPrefix}_{label}";

            bool GetIsOpen()
            {
                return EditorPrefs.GetBool(prefKey, defaultOpen);
            }

            void SetIsOpen(bool value)
            {
                EditorPrefs.SetBool(prefKey, value);
            }

            void RefreshFoldoutState()
            {
                foldout.value = GetIsOpen();
            }

            foldout.RegisterValueChangedCallback(changeEvent =>
            {
                SetIsOpen(changeEvent.newValue);
                RefreshFoldoutState();
            });

            RefreshFoldoutState();
            return foldout;
        }

        /// <summary>
        /// Create a foldout that saves its open/closed state automatically in <see cref="SessionState"/>
        /// Shares its open/closed state with all instances of this property, so e.g. it will remain open/closed
        /// as you select different assets in the Inspector. 
        /// </summary>
        public static Foldout CreateSavedFoldoutForProperty(
            SerializedProperty serializedProperty,
            string label = null,
            string tooltip = null,
            bool defaultOpen = false)
        {
            var foldout = new Foldout()
            {
                text = label ?? serializedProperty.displayName,
                tooltip = tooltip ?? serializedProperty.tooltip
            };

            string typeName; 
            if (serializedProperty.serializedObject.targetObject != null)
            {
                typeName = serializedProperty.serializedObject.targetObject.GetType().FullName;
            }
            else
            {
                typeName = "INVALID";
                Debug.LogError($"No type found for serialized property {serializedProperty.propertyPath}");
            }
            
            string prefKey = $"{typeName}_{serializedProperty.propertyPath}";

            // Properties are more transient than global prefs -- save object-specific data
            // in SessionState rather than cluttering up EditorPrefs
            bool GetIsOpen()
            {
                return SessionState.GetBool(prefKey, defaultOpen);
            }

            void SetIsOpen(bool value)
            {
                SessionState.SetBool(prefKey, value);
            }

            void RefreshFoldoutState()
            {
                foldout.value = GetIsOpen();
            }

            foldout.RegisterValueChangedCallback(changeEvent =>
            {
                SetIsOpen(changeEvent.newValue);
                RefreshFoldoutState();
            });

            RefreshFoldoutState();
            return foldout;
        }

        /// <summary>
        /// Create a foldout that saves its open/closed state automatically in <see cref="SessionState"/>
        /// Its open/closed state is specific to the target object, so the open/closed state will not be shared
        /// when selecting different assets in the Inspector.
        /// </summary>
        public static Foldout CreateSavedFoldoutForObject(
            SerializedObject serializedObject,
            string label,
            string tooltip = null,
            bool defaultOpen = false)
        {
            var foldout = new Foldout()
            {
                text = label,
                tooltip = tooltip ?? string.Empty
            };

#if UNITY_6000_4_OR_NEWER
            var prefKey = $"{serializedObject.targetObject.GetEntityId()}_{label}";
#else
            var prefKey = $"{serializedObject.targetObject.GetInstanceID()}_{label}";
#endif

            // Objects are more transient than global prefs -- save object-specific data
            // in SessionState rather than cluttering up EditorPrefs
            bool GetIsOpen()
            {
                return SessionState.GetBool(prefKey, defaultOpen);
            }

            void SetIsOpen(bool value)
            {
                SessionState.SetBool(prefKey, value);
            }

            void RefreshFoldoutState()
            {
                foldout.value = GetIsOpen();
            }

            foldout.RegisterValueChangedCallback(changeEvent =>
            {
                SetIsOpen(changeEvent.newValue);
                RefreshFoldoutState();
            });

            RefreshFoldoutState();
            return foldout;
        }

        /// <summary>
        /// Creates a box element that insets all of its contents.
        /// </summary>
        public static VisualElement CreateBoxElement(string label = null, bool boldLabel = false)
        {
            const float BoxMargin = 3f;
            const float BoxPadding = 2f;
            const float HeaderMargin = 2f;

            var container = new VisualElement
            {
                style =
                {
                    marginTop = BoxMargin,
                    marginBottom = 0, // Don't double-up top and bottom margins
                    marginLeft = BoxMargin,
                    marginRight = BoxMargin,
                    paddingTop = BoxPadding,
                    paddingBottom = BoxPadding,
                    paddingLeft = BoxPadding,
                    paddingRight = BoxPadding,
                }
            };

            if (!string.IsNullOrEmpty(label))
            {
                var labelElement = new Label(label)
                {
                    style =
                    {
                        marginBottom = HeaderMargin,
                        unityFontStyleAndWeight = boldLabel ? FontStyle.Bold : FontStyle.Normal
                    }
                };
                container.Add(labelElement);
            }

            container.AddToClassList("unity-box");
            return container;
        }

        public static VisualElement CreateRowElement()
        {
            return new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap
                }
            };
        }

        public static VisualElement CreateSpaceElement(int size = 8)
        {
            return new VisualElement
            {
                style =
                {
                    flexGrow = 0f,
                    height = size,
                    width = size
                }
            };
        }

        public static VisualElement CreateFlexibleSpaceElement()
        {
            return new VisualElement
            {
                style =
                {
                    flexGrow = 99f
                }
            };
        }

        /// <summary>
        /// Creates a <see cref="VisualElement"/> for an array property, and invokes
        /// the provided callback when an element is added to the array.
        /// </summary>
        public static VisualElement CreateArrayPropertyFieldWithCallbacks(
            SerializedProperty property,
            string label,
            Action<int, object> elementCreatedCallback)
        {
            var field = new PropertyField(property, label);
            if (property.isArray)
            {
                var cachedSize = property.arraySize;

                field.TrackPropertyValue(property, callback =>
                {
                    if (property.arraySize != cachedSize)
                    {
                        for (var i = cachedSize; i < property.arraySize; i++)
                        {
                            var indexProperty = property.GetArrayElementAtIndex(i);
                            var value = indexProperty.GetValue();
                            elementCreatedCallback?.Invoke(i, value);
                        }

                        cachedSize = property.arraySize;
                    }
                });
            }

            return field;
        }

        /// <summary>
        /// Overload of <see cref="BindingExtensions.TrackPropertyValue"/> that can invoke the callback
        /// immediately upon binding to save some boilerplate.
        /// </summary>
        public static void TrackPropertyValue(
            this VisualElement element,
            SerializedProperty property,
            Action<SerializedProperty> callback,
            bool invokeImmediately)
        {
            element.TrackPropertyValue(property, callback);
            if (invokeImmediately)
            {
                callback?.Invoke(property);
            }
        }

        /// <summary>
        /// Overload of <see cref="BindingExtensions.TrackSerializedObjectValue"/> that can invoke the callback
        /// immediately upon binding to save some boilerplate.
        /// </summary>
        public static void TrackSerializedObjectValue(
            this VisualElement element,
            SerializedObject obj,
            Action<SerializedObject> callback,
            bool invokeImmediately)
        {
            element.TrackSerializedObjectValue(obj, callback);
            if (invokeImmediately)
            {
                callback?.Invoke(obj);
            }
        }
        
        /// <summary>
        /// Invokes a callback when the given property's SerializedObject changes just like the standard
        /// <see cref="BindingExtensions.TrackPropertyValue"/>, but validates that the property still exists first.
        /// This prevents errors from being thrown when attempting to access a property in an array element
        /// that has just been destroyed.
        /// </summary>
        public static void TrackSerializedObjectValueForProperty(
            this VisualElement visualElement,
            SerializedProperty property,
            Action<SerializedObject> callback)
        {
            var cachedPropertyPath = property.propertyPath;
            visualElement.TrackSerializedObjectValue(property.serializedObject, serializedObject =>
            {
                if (serializedObject.FindProperty(cachedPropertyPath) != null)
                {
                    callback?.Invoke(serializedObject);
                }
            });
        }


        /// <summary>
        /// Overload of <see cref="TrackSerializedObjectValueForProperty"/> that can invoke the callback
        /// immediately upon binding to save some boilerplate.
        /// </summary>
        public static void TrackSerializedObjectValueForProperty(
            this VisualElement element,
            SerializedProperty property,
            Action<SerializedObject> callback,
            bool invokeImmediately)
        {
            element.TrackSerializedObjectValueForProperty(property, callback);
            if (invokeImmediately)
            {
                callback?.Invoke(property.serializedObject);
            }
        }

        /// <summary>
        /// Configures this <see cref="VisualElement"/> to show or hide itself whenever the given property changes.
        /// </summary>
        public static void TrackPropertyForConditionalDisplay(
            this VisualElement element,
            SerializedProperty property,
            Func<SerializedProperty, bool> shouldDisplayCallback)
        {
            void UpdateDisplay(SerializedProperty prop)
            {
                var shouldDisplay = shouldDisplayCallback == null || shouldDisplayCallback.Invoke(prop);
                element.style.display = shouldDisplay ? DisplayStyle.Flex : DisplayStyle.None;
            }

            element.TrackPropertyValue(property, UpdateDisplay);
            UpdateDisplay(property);
        }

        /// <summary>
        /// Configures this <see cref="VisualElement"/> to show or hide itself whenever any property
        /// on the given <see cref="SerializedObject"/> changes.
        /// </summary>
        public static void TrackObjectForConditionalDisplay(
            this VisualElement element,
            SerializedObject serializedObject,
            Func<SerializedObject, bool> shouldDisplayCallback)
        {
            void UpdateDisplay(SerializedObject obj)
            {
                var shouldDisplay = shouldDisplayCallback == null || shouldDisplayCallback.Invoke(obj);
                element.style.display = shouldDisplay ? DisplayStyle.Flex : DisplayStyle.None;
            }

            element.TrackSerializedObjectValue(serializedObject, UpdateDisplay);
            UpdateDisplay(serializedObject);
        }

        /// <summary>
        /// Adds UI Elements all properties of this object other than the specified properties.
        /// Heavily based on InspectorElement.FillDefaultInspector() and Editor.DrawPropertiesExcluding()
        /// </summary>
        public static void FillDefaultInspectorExcluding(
            VisualElement container,
            SerializedObject serializedObject,
            Editor editor,
            params SerializedProperty[] excludeProperties)
        {
            FillDefaultInspectorInternal(container, serializedObject, editor, null, excludeProperties);
        }

        /// <summary>
        /// Adds UI Elements all properties of this object up until the specified property.
        /// Heavily based on InspectorElement.FillDefaultInspector() and Editor.DrawPropertiesExcluding()
        /// </summary>
        public static void FillDefaultInspectorUntil(
            VisualElement container,
            SerializedObject serializedObject,
            Editor editor,
            SerializedProperty breakOnProperty)
        {
            FillDefaultInspectorInternal(container, serializedObject, editor, breakOnProperty, null);
        }

        private static void FillDefaultInspectorInternal(
            VisualElement container,
            SerializedObject serializedObject,
            Editor editor,
            SerializedProperty breakOnProperty,
            SerializedProperty[] excludeProperties)
        {
            bool IsAnyMonoBehaviourTargetPartOfPrefabInstance(Editor editor)
            {
                if (!(editor.target is MonoBehaviour))
                    return false;
                foreach (UnityEngine.Object target in editor.targets)
                {
                    if (PrefabUtility.IsPartOfNonAssetPrefabInstance(target))
                    {
                        return true;
                    }
                }

                return false;
            }

            if (serializedObject == null)
                return;

            bool isPartOfPrefabInstance = (UnityEngine.Object)editor != (UnityEngine.Object)null &&
                                          IsAnyMonoBehaviourTargetPartOfPrefabInstance(editor);
            SerializedProperty iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
            {
                do
                {
                    if (iterator.propertyPath == breakOnProperty?.propertyPath)
                    {
                        break;
                    }

                    if (excludeProperties != null &&
                        excludeProperties.Any(p => iterator.propertyPath == p?.propertyPath))
                    {
                        continue;
                    }

                    PropertyField child = new PropertyField(iterator);
                    child.name = "PropertyField:" + iterator.propertyPath;
                    if (iterator.propertyPath == "m_Script" &&
                        ((serializedObject.targetObject != (UnityEngine.Object)null
                             ? 1
                             : (iterator.objectReferenceValue != (UnityEngine.Object)null ? 1 : 0)) |
                         (isPartOfPrefabInstance ? 1 : 0)) != 0)
                        child.SetEnabled(false);
                    container.Add((VisualElement)child);
                } while (iterator.NextVisible(false));
            }
        }

        #endregion
    }
}