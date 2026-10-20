using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace KOOPEditor
{
    /// <summary>
    /// Utility for highlighting certain asset names and labels in the editor.
    /// Created as part of the prop migration to help authors avoid obsolete assets, but can be generalized.
    /// </summary>
    /// <owner>Lazlo</owner>
    [InitializeOnLoad]
    public static class EditorHighlightUtility
    {
        private const float Alpha = 0.3f;
        
        // TODO: I tried to create a EditorSetting for this, but the editor GUI code from the settings package
        // does not seem to properly support arbitrary lists nor classes, so in the interest of speed for now
        // I'm defining those as hardcoded constants.
        /// <remarks>
        /// Evaluated in order of priority; put more significant highlights first.
        /// </remarks>
        private static readonly Mapping[] _mappings =
        {
            new("Invalid", new Color(1f, 0f, 0f, Alpha)),
            new("Misconfigured", new Color(1f, 0.5f, 0f, Alpha)),
            new("Obsolete", new Color(1f, 0.5f, 0f, Alpha)),
            new("Deprecated", new Color(1f, 0.5f, 0f, Alpha)),
            new("Warning", new Color(1f, 1f, 0f, Alpha)),
            new("Todo", new Color(1f, 1f, 0f, Alpha)),
            new("ToRework", new Color(1f, 1f, 0f, Alpha)),
            new("ToReExport", new Color(1f, 1f, 0f, Alpha)),
            new("ToMaterialRematch", new Color(1f, 1f, 0f, Alpha)),
            new("Temp", new Color(0.5f, 0.5f, 0.5f, Alpha)),
            new("Temporary", new Color(0.5f, 0.5f, 0.5f, Alpha)),
        };

        private static IReadOnlyList<Mapping> Mappings => _mappings;

        [Serializable]
        private sealed class Mapping
        {
            public bool AppliesToProject = true;
            public bool AppliesToHierarchy = true;
            public string Label;
            public string Name;
            public string Prefix;
            public string Suffix;
            public string Tooltip;
            public Color OverlayColor;

            public Mapping(string identifier, Color overlayColor)
            {
                Label = identifier;
                Name = identifier;
                Prefix = identifier + "_";
                Suffix = "_" + identifier;
                Tooltip = identifier;
                OverlayColor = overlayColor;
            }

            public bool MatchesAsset(UnityObject asset)
            {
                if (asset == null) throw new ArgumentNullException(nameof(asset));
                
                foreach (var label in AssetDatabase.GetLabels(asset))
                {
                    if (string.Equals(label, Label, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }

                return MatchesObject(asset);
            }

            public bool MatchesSceneGameObject(GameObject sceneGameObject)
            {
                if (sceneGameObject == null) throw new ArgumentNullException(nameof(sceneGameObject));

                if (PrefabUtility.IsPartOfPrefabInstance(sceneGameObject))
                {
                    var prefabBase = PrefabUtility.GetCorrespondingObjectFromSource(sceneGameObject);
                    
                    // Could be null when missing/broken prefab
                    if (prefabBase != null && MatchesAsset(prefabBase))
                    {
                        return true;
                    }
                }

                return MatchesObject(sceneGameObject);
            }

            private bool MatchesObject(UnityObject @object)
            {
                if (@object == null) throw new ArgumentNullException(nameof(@object));
                
                if (string.Equals(@object.name, Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
                
                if ((!string.IsNullOrEmpty(Prefix) && @object.name.StartsWith(Prefix, StringComparison.InvariantCultureIgnoreCase)) ||
                    (!string.IsNullOrEmpty(Suffix) && @object.name.EndsWith(Suffix, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return true;
                }

                return false;
            }

            public void Draw(Rect rect)
            {
                EditorGUI.DrawRect(rect, OverlayColor);
                GUI.Label(rect, new GUIContent(string.Empty, $"({Tooltip})"));
            }
        }

        static EditorHighlightUtility()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemGUI;
        }

        private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null) return;
            var mapping = Mappings.FirstOrDefault(m => m.MatchesAsset(asset));
            if (mapping == null) return;
            selectionRect.xMin = 0;
            mapping.Draw(selectionRect);
        }

        private static void OnHierarchyWindowItemGUI(int instanceID, Rect selectionRect)
        {
            var sceneObject = EditorUtility.EntityIdToObject(instanceID);
            if (sceneObject == null) return;
            if (sceneObject is not GameObject sceneGameObject) return;
            var mapping = Mappings.FirstOrDefault(m => m.MatchesSceneGameObject(sceneGameObject));
            if (mapping == null) return;
            selectionRect.xMin = 0;
            selectionRect.xMax += 16;
            mapping.Draw(selectionRect);
        }
    }
}