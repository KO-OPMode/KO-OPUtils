#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KOOP;
using UnityCollections;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace KOOPEditor
{
    public static class AssetUtils
    {
        /// <summary>
        /// Returns the path to the root directory of the project
        /// </summary>
        public static string ProjectRootPath
        {
            get
            {
                var dataPath = Application.dataPath;
                return dataPath.Substring(0, dataPath.Length - "Assets/".Length);
            }
        }

        /// <summary>
        /// Attempts to find and load the first asset of type <typeparamref name="T"/>.
        /// </summary>
        public static T FindAndLoadFirstAsset<T>() where T : Object =>
            (T)FindAndLoadFirstAsset(string.Empty, string.Empty, typeof(T));

        /// <summary>
        /// Attempts to find and load the first asset of type <paramref name="type"/>.
        /// </summary>
        public static Object FindAndLoadFirstAsset(string search, System.Type type) =>
            FindAndLoadFirstAsset(search, string.Empty, type);

        /// <summary>
        /// Attempts to find and load the first asset of type <paramref name="type"/> in the directory <paramref name="dir"/>.
        /// If <paramref name="dir"/> is null or empty, this will search the entire project.
        /// </summary>
        public static Object FindAndLoadFirstAsset(string search, string dir, System.Type type)
        {
            string fullTypeName = GetSearchableTypeName(type);
            string[] assets = null;
            if (!string.IsNullOrEmpty(dir))
            {
                assets = AssetDatabase.FindAssets($"{search} t:{fullTypeName}", new string[] { dir });
            }
            else
            {
                assets = AssetDatabase.FindAssets($"{search} t:{fullTypeName}");
            }

            if (assets.Length == 0)
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[0]), type);
        }

        /// <summary>
        /// Returns the searchable type name.
        /// </summary>
        private static string GetSearchableTypeName(System.Type type)
        {
            string fullTypeName = type.FullName;
            string[] words = fullTypeName.Split('.');

            // NB: I (max) wrote this a long time ago and I don't remember why you have to do this--
            //     if I had to guess maybe Unity auto-filters out its own namespaces when you're searching types?
            if (fullTypeName.StartsWith("UnityEngine."))
            {
                fullTypeName = words[words.Length - 1];
            }
            else if (fullTypeName.StartsWith("UnityEditor."))
            {
                fullTypeName = words[words.Length - 1];
            }

            return fullTypeName;
        }

        /// <summary>
        /// Changes an absolute path to a Unity asset path (starts with Assets/).
        /// </summary>
        public static string AbsoluteToAssetPath(string absolutePath)
        {
            if (absolutePath.StartsWith(Application.dataPath))
            {
                absolutePath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
            }

            return absolutePath;
        }

        /// <summary>
        /// Changes a Unity asset path (starts with Assets/) to an absolute path.
        /// </summary>
        public static string AssetPathToAbsolute(string assetPath)
        {
            if (assetPath.StartsWith("Assets"))
            {
                assetPath = Application.dataPath + assetPath.Substring("Assets".Length);
            }

            return assetPath;
        }

        /// <summary>
        /// Recursively creates the specified folder hierarchy.
        /// </summary>
        /// <returns>
        /// True if the folder hierarchy for the has been successfully created or if it was already present.
        /// </returns>
        public static bool CreateFolderHierarchy(string folderPath, bool refreshAssetDatabase = true, bool log = false)
        {
            folderPath = PathUtility.NaiveNormalize(folderPath);

            if (!PathUtility.NaiveContains("Assets", folderPath, recursive: true))
            {
                Debug.LogWarning(
                    $"Attempting to create a folder hierarchy for a non-project-relative path; this is not supported." +
                    $"\nPath:{folderPath}");

                return false;
            }

            var pathParts = folderPath.Split(PathUtility.NaiveSeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            var parentFolderPath = pathParts[0];

            var foldersCreated = false;

            try
            {
                AssetDatabase.StartAssetEditing();

                for (var index = 1; index < pathParts.Length; index++)
                {
                    var pathPart = pathParts[index];
                    var levelFolderName = pathPart;
                    var levelFolderPath = parentFolderPath + PathUtility.NaiveSeparatorChar + pathPart;

                    if (!AssetDatabase.IsValidFolder(levelFolderPath))
                    {
                        var newFolderGuid = AssetDatabase.CreateFolder(parentFolderPath, levelFolderName);

                        if (!string.IsNullOrEmpty(newFolderGuid))
                        {
                            foldersCreated = true;
                        }
                        else
                        {
                            Debug.LogError($"Failed to create folder '{levelFolderName}' under '{parentFolderPath}'");
                            return false;
                        }

                        if (log)
                        {
                            Debug.Log($"Created folder: {levelFolderPath}");
                        }
                    }

                    parentFolderPath = levelFolderPath;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            if (refreshAssetDatabase && foldersCreated)
            {
                // NOTE: This shouldn't be necessary, because we use AssetDatabase API already,
                // but I'm leaving it in optionally/default for backward compatibility safety - Lazlo
                AssetDatabase.Refresh();
            }

            return true;
        }

        /// <summary>
        /// Recursively creates parent folders from a given asset path.
        /// Equivalent of calling <see cref="CreateFolderHierarchy"/> on the parent folder of <see cref="assetPath"/>.
        /// </summary>
        /// <remarks>
        /// We have to do this because Unity
        /// requires parent folders to exist before you can create a child folder in that parent!
        /// And doesn't just create the entire directory hierarchy! For some reason!
        /// </remarks>
        /// <returns>
        /// True if the folder hierarchy for the asset path has been successfully created or if it was already present.
        /// </returns>
        public static bool CreateFolderHierarchyFromAssetPath(string assetPath, bool refreshAssetDatabase = true,
            bool log = false)
        {
            return CreateFolderHierarchy(PathUtility.NaiveParent(assetPath), refreshAssetDatabase, log);
        }

        /// <summary>
        /// Returns a list of assets inside the given folder path (including the Assets/ prefix)
        /// </summary>
        /// <param name="searchPattern">Search pattern for filtering results(supports * and ? as wildcards)</param>
        /// <param name="includeSubfolders">Whether to search subfolders recursively or just search the root</param>
        /// <param name="relativeToFolder">If true, results are relative to the provided folder path, not Assets/</param>
        public static List<string> GetAssetsInFolder(
            string folder,
            string searchPattern = null,
            bool includeSubfolders = true,
            bool relativeToFolder = false)
        {
            var resultPaths = new List<string>();

            if (!AssetDatabase.IsValidFolder(folder))
            {
                return resultPaths;
            }

            string projectRootPath = ProjectRootPath;
            string directoryPath = Path.Combine(projectRootPath, folder);

            string[] filePaths = Directory.GetFiles(
                directoryPath,
                !string.IsNullOrEmpty(searchPattern) ? searchPattern : string.Empty,
                includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            foreach (var filePath in filePaths)
            {
                var prefixToRemoveLength = relativeToFolder ? directoryPath.Length + 1 : projectRootPath.Length + 1;

                // Remove the parent folder and the trailing /
                var assetPath = filePath.Remove(0, prefixToRemoveLength);

                // Replace any backslashes with slashes for consistency
                assetPath = assetPath.Replace('\\', '/');

                resultPaths.Add(assetPath);
            }

            return resultPaths;
        }

        /// <summary>
        /// Equivalent of <see cref="AssetDatabase.FindAssets(string, string[])"/>, but in a single-folder and non-recursive,
        /// and returning paths instead of GUIDs.
        /// </summary>
        /// <param name="mainAssetType">
        /// A main asset type to filter results by.
        /// Useful when wanting to avoid this search returning paths for sub-assets that match the filter,
        /// such as passing a "t:Material" filter would return paths of all models that import materials as sub-assets
        /// unless <see cref="mainAssetType"/> is defined as <see cref="Material"/>
        /// </param>
        public static string[] FindAssetPathsNonRecursive(string filter, string folderPath, Type mainAssetType = null)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                return Array.Empty<string>();
            }

            return AssetDatabase.FindAssets(filter, new[] { folderPath })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(assetPath => PathUtility.NaiveContains(folderPath, assetPath, recursive: false))
                .Where(path =>
                    mainAssetType == null || mainAssetType.IsAssignableFrom(AssetDatabase.GetMainAssetTypeAtPath(path)))
                .ToArray();
        }

        /// <summary>
        /// Returns the project-relative paths of selected folders in the project window.
        /// Also supports/includes folders in the left pane of the window.
        /// </summary>
        public static IEnumerable<string> GetSelectedFoldersPath()
        {
            // https://discussions.unity.com/t/selection-activeobject-on-items-on-left-side-of-project-window/664382/6
            var selectedProjectObjects = Selection.GetFiltered<Object>(SelectionMode.Assets);

            foreach (var selectedObject in selectedProjectObjects)
            {
                if (!AssetDatabase.Contains(selectedObject)) continue;

                string path = AssetDatabase.GetAssetPath(selectedObject);

                if (!AssetDatabase.IsValidFolder(path)) continue;

                yield return path;
            }
        }

        /// <summary>
        /// Checks whether a given assets folder is empty.
        /// </summary>
        /// <param name="path">The project-relative path (starting with "Assets/")</param>
        public static bool IsFolderEmpty(string path)
        {
            if (!AssetDatabase.IsValidFolder(path)) throw new ArgumentException($"Invalid folder: {path}");

            return !Directory.EnumerateFileSystemEntries(AssetPathToAbsolute(path)).Any();
        }

        public static string GetAssetNameFromPath(string path)
        {
            path = path.Substring(path.LastIndexOf('/') + 1);
            path = path.Substring(0, path.IndexOf('.'));
            return path;
        }
    }
}

#endif