using System.Linq;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace KOOPEditor
{
    /// <summary>
    /// Provides utilities for dealing with Unity Editor shortcuts.
    /// </summary>
    /// <owner>Lazlo</owner>
    public static class ShortcutUtility
    {
        /// <summary>
        /// Returns whether the action key is pressed on the selected event. 
        /// </summary>
        /// <returns>
        /// On OSX, this returns <see cref="Event.command"/>; on other platforms, it returns <see cref="Event.control"/>
        /// </returns>
        public static bool GetAction(this Event e)
        {
#if UNITY_EDITOR_OSX
            return e.command;
#else
            return e.control;
#endif
        }
        
        /// <summary>
        /// Checks whether a given UI event matches a key combination for the specified shortcut modifiers. 
        /// </summary>
        public static bool MatchesKeyCombination(this Event e, KeyCombination keyCombination, ShortcutModifiers modifiersChecks)
        {
            return e.keyCode == keyCombination.keyCode &&
                   (!modifiersChecks.HasFlag(ShortcutModifiers.Alt) || e.alt == keyCombination.alt) &&
                   (!modifiersChecks.HasFlag(ShortcutModifiers.Shift) || e.shift == keyCombination.shift) &&
                   (!modifiersChecks.HasFlag(ShortcutModifiers.Action) || e.GetAction() == keyCombination.action);
        }

        /// <summary>
        /// Attempts to return the single key combination for a given shortcut binding.
        /// If zero or multiple combinations are associated to that binding, returns false.
        /// </summary>
        public static bool TryGetSingleKeyCombination(this ShortcutBinding shortcutBinding, out KeyCombination keyCombination)
        {
            if (shortcutBinding.keyCombinationSequence.Count() != 1)
            {
                keyCombination = default;
                return false;
            }

            keyCombination = shortcutBinding.keyCombinationSequence.Single();
            return true;
        }
    }
}