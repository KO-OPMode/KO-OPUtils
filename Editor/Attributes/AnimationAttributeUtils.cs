#if UNITY_EDITOR

using UnityEngine;
using UnityEditor.Animations;
using System.Collections;
using System.Collections.Generic;
using KOOP.Attributes;

namespace KOOPEditor.Attributes
{
    internal static class AnimationAttributeUtils
    {
        public static GUIContent[] GetAnimationStates(this AnimatorController controller)
        {
            List<GUIContent> states = new List<GUIContent>();
            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                foreach (ChildAnimatorState state in layer.stateMachine.states)
                {
                    states.Add(new GUIContent(state.state.name));
                }
            }

            return states.ToArray();
        }

        public static GUIContent[] GetAnimationParameters(this AnimatorController controller)
        {
            List<GUIContent> parameters = new List<GUIContent>();
            foreach (AnimatorControllerParameter parameter in controller.parameters)
            {
                parameters.Add(new GUIContent(parameter.name));
            }

            return parameters.ToArray();
        }

        public static GUIContent[] GetAnimatorControllerLayers(this AnimatorController controller)
        {
            List<GUIContent> layers = new List<GUIContent>();
            foreach (var layer in controller.layers)
            {
                layers.Add(new GUIContent(layer.name));
            }

            return layers.ToArray();
        }

        public static GUIContent[] GetAnimationParametersWithTypes(this AnimatorController controller)
        {
            List<GUIContent> states = new List<GUIContent>();
            foreach (AnimatorControllerParameter parameter in controller.parameters)
            {
                states.Add(new GUIContent($"{parameter.name} <color=#666666>{parameter.type.ToString()}</color>"));
            }

            return states.ToArray();
        }

        public static int[] GetHashes(GUIContent[] strings)
        {
            int[] hashes = new int[strings.Length];
            for (int i = 0; i < strings.Length; ++i)
            {
                hashes[i] = Animator.StringToHash(strings[i].text);
            }

            return hashes;
        }
    }
}

#endif