#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEditor;
using UnityEditor.Animations;
using KOOP.Attributes;
using KOOP.Extensions;

namespace KOOPEditor.Attributes
{
    [CustomPropertyDrawer(typeof(AnimationStateAttribute))]
    public class AnimationStateAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Integer 
                && property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            AnimationStateAttribute animationStateAttribute = attribute as AnimationStateAttribute;
            
            // Get animator component
            Component go = property.serializedObject.targetObject as Component;

            HashSet<AnimatorController> animatorControllers = new();
            if (go != null)
            {
                if (animationStateAttribute!.findInChildren)
                {
                    foreach (Animator animator in go.GetComponentsInChildren<Animator>())
                    {
                        var animatorController = animator.GetEditorAnimatorController();
                        
                        if (animatorController != null)
                        {
                            animatorControllers.Add(animatorController);
                        }
                    }
                }
                else
                {
                    var animatorController = go.GetComponent<Animator>().UnityNullable()?.GetEditorAnimatorController();
                    
                    if (animatorController != null)
                    {
                        animatorControllers.Add(animatorController);
                    }
                }
            }

            if (animatorControllers.Count == 0)
            {
                // todo: add warning here?
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            using (new EditorGUI.PropertyScope(position, label, property))
            {
                List<GUIContent> stateNames = new();
                List<int> stateHashes = new();
                if (property.propertyType == SerializedPropertyType.Integer)
                {
                    foreach (var controller in animatorControllers)
                    {
                        GUIContent[] controllerStateNames = controller.GetAnimationStates();
                        int[] controllerStateHashes = AnimationAttributeUtils.GetHashes(controllerStateNames);
                        for (int i = 0; i < controllerStateNames.Length; i++)
                        {
                            if (animatorControllers.Count > 1)
                                controllerStateNames[i].text = $"{controller.name}/{controllerStateNames[i].text}";
                            stateNames.Add(controllerStateNames[i]);
                            stateHashes.Add(controllerStateHashes[i]);
                        }
                    }

                    EditorGUI.IntPopup(position, property, stateNames.ToArray(), stateHashes.ToArray());
                }
                else if (property.propertyType == SerializedPropertyType.String)
                {
                    foreach (var controller in animatorControllers)
                    {
                        GUIContent[] controllerStateNames = controller.GetAnimationStates();
                        int[] controllerStateHashes = AnimationAttributeUtils.GetHashes(controllerStateNames);
                        
                        for (int i = 0; i < controllerStateNames.Length; i++)
                        {
                            if (animatorControllers.Count > 1)
                                controllerStateNames[i].text = $"{controller.name}/{controllerStateNames[i].text}";
                            stateNames.Add(controllerStateNames[i]);
                            stateHashes.Add(controllerStateHashes[i]);
                        }
                    }

                    using (var c = new EditorGUI.ChangeCheckScope())
                    {
                        int currentIndex = -1;
                        for (int i = 0; i < stateNames.Count; ++i)
                        {
                            if (RemoveControllerPrefix(stateNames[i].text) == property.stringValue)
                            {
                                currentIndex = i;
                                break;
                            }
                        }

                        var value = EditorGUI.Popup(position, label, currentIndex, stateNames.ToArray());
                        if (c.changed)
                        {
                            property.stringValue = value == -1 ? string.Empty : RemoveControllerPrefix(stateNames[value].text);
                        }

                        string RemoveControllerPrefix(string stateDisplayName)
                        {
                            return stateDisplayName.Remove(0, stateDisplayName.LastIndexOf('/') + 1);
                        }
                    }
                }
            }
        }
    }
}

#endif