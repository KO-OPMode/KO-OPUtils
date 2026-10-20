#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using KOOP.Attributes;
using KOOP.Extensions;

namespace KOOPEditor.Attributes
{
    [CustomPropertyDrawer(typeof(AnimatorControllerLayerAttribute))]
    public class AnimatorControllerLayerAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Integer 
                && property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            AnimatorControllerLayerAttribute animatorControllerLayerAttribute = attribute as AnimatorControllerLayerAttribute;
            
            // Get animator component
            Component go = property.serializedObject.targetObject as Component;
            Animator anim = null;
            if (go != null)
            {
                anim = go.GetComponent<Animator>();
                if (anim == null && animatorControllerLayerAttribute.findInChildren)
                {
                    anim = go.GetComponentInChildren<Animator>();
                }
            }

            if (anim == null || anim.GetEditorAnimatorController() == null)
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
                    GUIContent[] layerNames = controller.GetAnimatorControllerLayers();

                    using (var c = new EditorGUI.ChangeCheckScope())
                    {
                        var value = EditorGUI.Popup(position, label, property.intValue, layerNames);
                        if (c.changed)
                        {
                            property.intValue = value;
                        }
                    }
                }
                else if (property.propertyType == SerializedPropertyType.String)
                {
                    GUIContent[] layerNames = controller.GetAnimatorControllerLayers();
                    int[] layerHashes = AnimationAttributeUtils.GetHashes(layerNames);

                    using (var c = new EditorGUI.ChangeCheckScope())
                    {
                        int currentIndex = -1;
                        for (int i = 0; i < layerNames.Length; ++i)
                        {
                            if (layerNames[i].text == property.stringValue)
                            {
                                currentIndex = i;
                                break;
                            }
                        }

                        var value = EditorGUI.Popup(position, label, currentIndex, layerNames);
                        if (c.changed)
                        {
                            property.stringValue = value == -1 ? string.Empty : layerNames[value].text;
                        }
                    }
                }
            }
        }
    }
}

#endif