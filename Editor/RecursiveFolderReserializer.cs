using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace KOOPEditor
{
    /// <summary>
    /// Add assets menu item that allows recursively reserializing all assets within a folder. 
    /// </summary>
    /// <remarks>
    /// Intended to be used after major Unity version upgrades or other significant csets to minimize the VCS noise caused by asset reserialization.
    /// </remarks>
    /// <owner>Lazlo</owner>
    public static class RecursiveFolderReserializer
    {
        private const string MenuPath = "Assets/Reserialize (Recursive)";
        private const int MenuPriority = 50;
        private const string DialogTitle = "Recursive Reserialization";

        [MenuItem(MenuPath, priority: MenuPriority, isValidateFunction: true)]
        private static bool CanReserializeRecursive(MenuCommand menuCommand)
        {
            return GetReserializableAssetGuids().Any();
        }

        [MenuItem(MenuPath, priority: MenuPriority, isValidateFunction: false)]
        private static void ReserializeRecursive(MenuCommand menuCommand)
        {
            var reserializableAssetPaths = GetReserializableAssetGuids().ToArray();

            if (reserializableAssetPaths.Length == 0)
            {
                EditorUtility.DisplayDialog(DialogTitle, $"No asset found to reserialize.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog(DialogTitle,
                                             $"Are you sure you want to reserialize {reserializableAssetPaths.Length} " +
                                             $"assets in the selected folder(s)?",
                                             "Reserialize",
                                             "Cancel"))
            {
                return;
            }

            AssetDatabase.ForceReserializeAssets(reserializableAssetPaths.Select(AssetDatabase.GUIDToAssetPath));
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private static IEnumerable<string> GetReserializableAssetGuids()
        {
            var folderPaths = AssetUtils.GetSelectedFoldersPath().ToArray();
            
            if (folderPaths.Length == 0)
            {
                return Enumerable.Empty<string>();
            }
            
            return AssetDatabase.FindAssets(string.Empty, folderPaths);
        }
    }
}