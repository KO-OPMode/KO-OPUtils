using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KOOP;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using static KOOPEditor.PrefabExtraUtility;

namespace KOOPEditor
{
    /// <summary>
    /// Editor operations and menu items to create or detach a prefab variant or base.
    /// </summary>
    public static class PrefabVariantWrangler
    {
        #region Create Prefab Variant

        public static GameObject CreatePrefabVariant(GameObject prefabOrModelBase, string variantPath)
        {
            var path = AssetDatabase.GetAssetPath(prefabOrModelBase);
            
            if (!IsPrefabOrVariantOrModel(prefabOrModelBase))
            {
                throw new ArgumentException("Base must be prefab or variant or model.", nameof(prefabOrModelBase));
            }
            
            // Note: We can't use LoadPrefabContents, because it will fail for models
            // Simply instantiate and destroy immediate instead
            var root = (GameObject)PrefabUtility.InstantiatePrefab(prefabOrModelBase); // Not: PrefabUtility.LoadPrefabContents(path);
            PrefabUtility.SaveAsPrefabAssetAndConnect(root, variantPath, InteractionMode.AutomatedAction);
            UnityObject.DestroyImmediate(root); // Not: PrefabUtility.UnloadPrefabContents(root);

            var variant = AssetDatabase.LoadAssetAtPath<GameObject>(variantPath);
            return variant;
        }
        
        #endregion
        
        
        #region Create Prefab Base

        // Turns a prefab or variant into a variant of itself while preserving all its references,
        // and creates another "base" prefab to host its former contents
        // Based on this talk: https://youtu.be/J9WFcPbxOhQ?t=1667

        private static bool IsValidCreatePrefabBaseTarget(GameObject target)
        {
            return IsPrefabOrVariant(target);
        }

        private static IEnumerable<GameObject> GetCreatePrefabBaseTargets()
        {
            return Selection.objects.OfType<GameObject>().Where(IsValidCreatePrefabBaseTarget);
        }

        [MenuItem("Assets/Prefab/Create Prefab Base", isValidateFunction: true)]
        public static bool IsCreatePrefabBaseMenuItemEnabled()
        {
            return GetCreatePrefabBaseTargets().Any();
        }

        [MenuItem("Assets/Prefab/Create Prefab Base")]
        public static void ExecuteCreatePrefabBaseMenuItem()
        {
            if (!EditorUtility.DisplayDialog("Create Prefab Base",
                                             "Are you sure you want to create a prefab base for the selected prefabs?\n\n" +
                                             "THIS IS NOT UNDOABLE.",
                                             "Create", "Cancel"))
            {
                return;
            }

            foreach (var target in GetCreatePrefabBaseTargets())
            {
                CreatePrefabBase(target, baseSuffix: " Base", variantSuffix: null, out var @base, out var variant);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void CreatePrefabBase(GameObject prefabOrVariant, string baseSuffix, string variantSuffix, out GameObject @base, out GameObject variant)
        {
            if (prefabOrVariant == null) throw new ArgumentNullException(nameof(prefabOrVariant));

            if (!IsPrefabOrVariant(prefabOrVariant))
            {
                throw new ArgumentException("Must be prefab or variant.", nameof(prefabOrVariant));
            }

            if (string.IsNullOrWhiteSpace(baseSuffix))
            {
                baseSuffix = null;
            }

            if (string.IsNullOrWhiteSpace(variantSuffix))
            {
                variantSuffix = null;
            }

            if (baseSuffix?.ToUpperInvariant() == variantSuffix?.ToUpperInvariant())
            {
                throw new ArgumentException("Base suffix must be different than variant suffix.");
            }

            var path = AssetDatabase.GetAssetPath(prefabOrVariant);
            var basePath = AssetDatabase.GenerateUniqueAssetPath(PathUtility.AppendFileName(path, baseSuffix));
            var variantPath = AssetDatabase.GenerateUniqueAssetPath(PathUtility.AppendFileName(path, variantSuffix));

            var root = PrefabUtility.LoadPrefabContents(path);
            PrefabUtility.SaveAsPrefabAssetAndConnect(root, basePath, InteractionMode.AutomatedAction);
            PrefabUtility.SaveAsPrefabAsset(root, path);

            if (baseSuffix == null)
            {
                AssetDatabase.RenameAsset(path, Path.GetFileNameWithoutExtension(variantPath));
                var basePathTry2 = AssetDatabase.GenerateUniqueAssetPath(path);
                AssetDatabase.RenameAsset(basePath, Path.GetFileNameWithoutExtension(basePathTry2));
                basePath = basePathTry2;
            }
            else if (variantSuffix == null) { }
            else
            {
                AssetDatabase.RenameAsset(path, Path.GetFileNameWithoutExtension(variantPath));
            }

            PrefabUtility.UnloadPrefabContents(root);

            @base = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);

            variant = AssetDatabase.LoadAssetAtPath<GameObject>(variantPath);
        }

        #endregion


        #region Detach Prefab Base

        // The reverse: turns a variant into a regular prefab
        // Based on this talk: https://youtu.be/J9WFcPbxOhQ?t=1765

        private static bool IsValidDetachPrefabBaseTarget(GameObject target)
        {
            return IsPrefabVariant(target);
        }

        private static IEnumerable<GameObject> GetDetachPrefabBaseTargets()
        {
            return Selection.objects.OfType<GameObject>().Where(IsValidDetachPrefabBaseTarget);
        }

        [MenuItem("Assets/Prefab/Detach Prefab Base", isValidateFunction: true)]
        public static bool IsDetachPrefabBaseMenuItemEnabled()
        {
            return GetDetachPrefabBaseTargets().Any();
        }

        [MenuItem("Assets/Prefab/Detach Prefab Base")]
        public static void ExecuteDetachPrefabBaseMenuItem()
        {
            if (!EditorUtility.DisplayDialog("Detach Prefab Base",
                                             "Are you sure you want to detach the prefab base from the selected prefabs?\n\n" +
                                             "THIS IS NOT UNDOABLE.",
                                             "Detach", "Cancel"))
            {
                return;
            }

            foreach (var target in GetDetachPrefabBaseTargets())
            {
                DetachPrefabBase(target);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void DetachPrefabBase(GameObject variant)
        {
            if (variant == null) throw new ArgumentNullException(nameof(variant));

            if (!IsPrefabVariant(variant))
            {
                throw new ArgumentException("Must be a prefab variant.", nameof(variant));
            }

            var path = AssetDatabase.GetAssetPath(variant);
            var root = PrefabUtility.LoadPrefabContents(path);
            PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        #endregion
    }
}