#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using KOOP.Attributes;
using KOOP.Extensions;

namespace KOOPEditor.Attributes
{
    [CustomPropertyDrawer(typeof(AnimationParameterAttribute))]
    public class AnimationParameterAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Integer 
                && property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            AnimationParameterAttribute animationParameterAttribute = attribute as AnimationParameterAttribute;
            
            // Get animator component
            Component go = property.serializedObject.targetObject as Component;
            Animator anim = null;
            if (go != null)
            {
                anim = go.GetComponent<Animator>();
                if (anim == null && animationParameterAttribute.findInChildren)
                {
                    anim = go.GetComponentInChildren<Animator>();
                }
            }

            if (anim == null || anim.runtimeAnimatorController == null)
            {
                // todo: add warning here?
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            AnimatorController controller = anim.GetEditorAnimatorController();

            using (new EditorGUI.PropertyScope(position, label, property))
            {
                if (property.propertyType == SerializedPropertyType.Integer)
                {
                    GUIContent[] parameterNames = controller.GetAnimationParameters();
                    int[] parameterHashes = AnimationAttributeUtils.GetHashes(parameterNames);

                    EditorGUI.IntPopup(position, property, parameterNames, parameterHashes);
                }
                else if (property.propertyType == SerializedPropertyType.String)
                {
                    GUIContent[] parameterNames = controller.GetAnimationParameters();
                    int[] parameterHashes = AnimationAttributeUtils.GetHashes(parameterNames);

                    using (var c = new EditorGUI.ChangeCheckScope())
                    {
                        int currentIndex = -1;
                        for (int i = 0; i < parameterNames.Length; ++i)
                        {
                            if (parameterNames[i].text == property.stringValue)
                            {
                                currentIndex = i;
                                break;
                            }
                        }

                        var value = EditorGUI.Popup(position, label, currentIndex, parameterNames);
                        if (c.changed)
                        {
                            property.stringValue = value == -1 ? string.Empty : parameterNames[value].text;
                        }
                    }
                }
            }
        }
    }
}

#endif