using System;
using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace KOOP.Extensions
{
    public static class AnimatorExtensions
    {
        // Rebind reflection fields
        private const string _rebindMethodName = "Rebind";
        private const BindingFlags _rebindBindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        private static MethodInfo _rebindMethod;

        /// <owner>max</owner>
        /// <summary>
        /// Rebinds the animator similar to the existing method <see cref="Animator.Rebind"/>.
        /// The existing method sets <paramref name="writeDefaultsToAnimator"/> to true by default,
        /// causing the animator to "bump" or "hitch" whenever you call it.
        /// Setting <paramref name="writeDefaultsToAnimator"/> to false prevents this.  
        /// <para/>
        /// NOTE: Uses reflection to access a private method on <see cref="Animator"/>. As such this can break if the method signature changes.
        /// </summary>
        public static void Rebind(this Animator target, bool writeDefaultsToAnimator)
        {
            if (_rebindMethod == null)
            {
                // Reflect rebind method
                _rebindMethod = target.GetType().GetMethod(_rebindMethodName, _rebindBindingFlags);
                if (_rebindMethod == null)
                {
                    Debug.LogError($"Attempted to use reflection to grab a method {_rebindMethodName} from the Animator type, but failed to find the method. "
                     + "Unity may have changed something, so this extension needs updating.");
                    return;
                }
            }

            _rebindMethod.Invoke(target, new object[] { writeDefaultsToAnimator });
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Finds the editor animator controller for an animator.
        /// This handles recursing through override controllers to find the root base controller.
        /// </summary>
        public static AnimatorController GetEditorAnimatorController(this Animator animator)
        {
            if (animator == null) throw new ArgumentNullException(nameof(animator));

            var controller = animator.runtimeAnimatorController;

            while (controller is AnimatorOverrideController overrideController)
            {
                controller = overrideController.runtimeAnimatorController;
            }

            return controller as AnimatorController;
        }
        #endif
    }
}