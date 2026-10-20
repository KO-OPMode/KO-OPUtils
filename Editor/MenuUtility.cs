using UnityEditor;

namespace KOOPEditor
{
    /// <summary>
    /// Various utilities for dealing with Unity editor menus.
    /// </summary>
    /// <owner>Lazlo</owner>
    public static class MenuUtility
    {
        // MenuItem for GameObject/ is called repeatedly for every object in the selection, which we want to prevent
        // so we can do batch operations, e.g. displaying a progress bar
        // https://discussions.unity.com/t/how-to-execute-menuitem-for-multiple-objects-once/91492/5
        // https://issuetracker.unity3d.com/issues/customeditor-multiple-fuction-call-from-custom-menuitem-when-calling-from-hierarchy
        public static bool ShouldExecuteForMultiSelection(this MenuCommand menuCommand)
        {
            if (menuCommand.context == null)
            {
                // This was called from the top-level Unity GameObject/ menu, not a contextual selection, so allow it through
                return true;
            }
            else
            {
                // This was called contextually from a right-click GameObject/ menu, so context will be assigned but it will be called multiple times
                // Confirm that it's on the main selected object before allowing the call through, only once

                // NOTE: This won't work if the operation modifies the selection!
                // There is a suggested hack in the thread to use Time.unscaledTime (not even EditorApplication.timeSinceStartup) to check,
                // but it feels even worse than this caveat.

                return menuCommand.context == Selection.activeObject;
            }
        }
    }
}