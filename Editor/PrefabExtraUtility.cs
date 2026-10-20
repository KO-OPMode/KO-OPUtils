using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KOOP;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace KOOPEditor
{
    /// <summary>
    /// Provides extra utilities for prefabs that complement <see cref="PrefabUtility"/>.
    /// </summary>
    /// <owner>Lazlo</owner>
	public static class PrefabExtraUtility
	{
        /// <summary>
        /// Returns whether the specified game object is a model asset.
        /// </summary>
		public static bool IsModel(GameObject target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

			if (!AssetDatabase.Contains(target) || !AssetDatabase.IsMainAsset(target))
			{
				return false;
			}

			return PrefabUtility.IsPartOfModelPrefab(target);
		}

        /// <summary>
        /// Returns whether the specified game object is a regular prefab asset, i.e., neither a model nor a variant.
        /// </summary>
		public static bool IsRegularPrefab(GameObject target)
		{
            if (target == null) throw new ArgumentNullException(nameof(target));

			if (!AssetDatabase.Contains(target) || !AssetDatabase.IsMainAsset(target))
			{
				return false;
			}

			return PrefabUtility.IsPartOfPrefabAsset(target) && PrefabUtility.GetPrefabAssetType(target) == PrefabAssetType.Regular;
		}

        /// <summary>
        /// Returns whether the specified game object is a prefab variant asset.
        /// </summary>
		public static bool IsPrefabVariant(GameObject target)
		{
            if (target == null) throw new ArgumentNullException(nameof(target));

			if (!AssetDatabase.Contains(target) || !AssetDatabase.IsMainAsset(target))
			{
				return false;
			}

			return PrefabUtility.IsPartOfPrefabAsset(target) && PrefabUtility.GetPrefabAssetType(target) == PrefabAssetType.Variant;
		}

        /// <summary>
        /// Returns whether the specified game object is either a regular prefab or a prefab variant asset, but not a model prefab.
        /// </summary>
		public static bool IsPrefabOrVariant(GameObject target)
		{
            if (target == null) throw new ArgumentNullException(nameof(target));

			if (!AssetDatabase.Contains(target) || !AssetDatabase.IsMainAsset(target))
			{
				return false;
			}

			var prefabType = PrefabUtility.GetPrefabAssetType(target);

			return PrefabUtility.IsPartOfPrefabAsset(target) && (prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Variant);
		}

        /// <summary>
        /// Returns whether the specified game object is any type of valid root prefab asset.
        /// </summary>
        public static bool IsPrefabOrVariantOrModel(GameObject target)
        {
            return IsPrefabOrVariant(target) || IsModel(target);
        }

        /// <summary>
        /// Returns whether the specified game object is a prefab variant of the specified base prefab.
        /// </summary>
		public static bool IsPrefabVariantOf(GameObject target, GameObject @base)
		{
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (@base == null) throw new ArgumentNullException(nameof(@base));
			
			return IsPrefabVariant(target) && PrefabUtility.GetCorrespondingObjectFromSource(target) == @base;
		}

        /// <summary>
        /// Returns whether the specified game object is a prefab instance.
        /// </summary>
        /// <param name="allowVariant">
        /// Unity considers prefab variant assets to be prefab 'instances' of their bases; this case is often misleading,
        /// so it is excluded by default here, but can be optionally included.
        /// </param>
		public static bool IsPrefabInstance(GameObject go, bool allowVariant = false)
		{
            if (go == null) throw new ArgumentNullException(nameof(go));
            
			return PrefabUtility.IsPartOfPrefabInstance(go) && (allowVariant || !IsPrefabVariant(go));
		}

        /// <summary>
        /// Returns whether the object is *derived* from a prefab, meaning its existence is implicit from being part of
        /// a prefab instance, and not from being specifically authored.
        /// In other words, this returns false when:
        /// - the object is not part of a prefab instance
        /// - the object is an added component override, an added game object override, or a component on an added game object override
        /// </summary>
		public static bool IsPrefabDerivative(UnityObject uo)
		{
            if (uo == null) throw new ArgumentNullException(nameof(uo));

			if (!PrefabUtility.IsPartOfPrefabInstance(uo))
			{
				return false;
			}

            if (uo is GameObject gameObject && PrefabUtility.IsAddedGameObjectOverride(gameObject))
            {
                return false;
            }

			if (uo is Component component && 
                (PrefabUtility.IsAddedComponentOverride(component) || PrefabUtility.IsAddedGameObjectOverride(component.gameObject)))
			{
				return false;
			}

			return true;
		}

        /// <summary>
        /// Returns whether the specified object is *authored* at its own level of existence, which is functionally the
        /// opposite of <see cref="IsPrefabDerivative"/>
        /// </summary>
		public static bool IsAuthoredObject(UnityObject uo)
		{
			return !IsPrefabDerivative(uo);
		}

        /// <summary>
        /// Returns whether the specified is instantiated in authoring, that is, whether it's either <see cref="IsAuthoredObject"/>
        /// or a root level instance (e.g. placed directly in a scene).
        /// </summary>
		public static bool IsAuthoredInstance(GameObject go)
		{
			return IsAuthoredObject(go) || (PrefabUtility.IsOutermostPrefabInstanceRoot(go) && !IsPrefabVariant(go));
		}

        /// <summary>
        /// Returns whether a given property name is authored at the level of the specified object.
        /// In other words, will return true if the object itself is authored (<see cref="IsAuthoredObject"/>)
        /// or if the property is a prefab override on the object.
        /// </summary>
		public static bool IsAuthoredProperty(UnityObject uo, string propertyName)
		{
			if (IsAuthoredObject(uo))
			{
				return true;
			}

			// Otherwise, it has to be a prefab override to be considered authored
			var serializedObject = new SerializedObject(uo);
			var serializedProperty = serializedObject.FindPropertyOrThrow(propertyName);
			var isOverride = serializedProperty.prefabOverride;
			serializedProperty.Dispose();
			serializedObject.Dispose();
			return isOverride;
		}

        /// <summary>
        /// Returns the base of the specified prefab variant, or null if the specified object is not a prefab variant. 
        /// </summary>
		public static GameObject GetPrefabVariantBase(GameObject target)
		{
            if (target == null) throw new ArgumentNullException(nameof(target));

			if (!IsPrefabVariant(target))
			{
				return null;
			}

			return PrefabUtility.GetCorrespondingObjectFromSource(target);
		}
        
        
        /// <summary>
        /// Returns the prefab that contains the given object, either
        /// from a prefab instance or a prefab stage if open for editing
        /// </summary>
        public static GameObject GetPrefabAsset(GameObject source)
        {
            if (PrefabUtility.IsPartOfPrefabAsset(source))
            {
                // This object is itself part of a prefab -- return its root object
                return source.transform.root.gameObject;
            }
            
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(source);

            if (stage != null)
            {
                // If our object is the root of the stage,
                // or part of a prefab that's the root of the stage,
                // or not part of any prefab, then our stage's source is the prefab we want
                if (stage.prefabContentsRoot == source || 
                    stage.prefabContentsRoot == prefabRoot || 
                    prefabRoot == null)
                {
                    return AssetDatabase.LoadAssetAtPath<GameObject>(stage.assetPath);
                }
            }

            if (prefabRoot != null)
            {
                // Return the source of the root of our prefab instance root
                return PrefabUtility.GetCorrespondingObjectFromSource(prefabRoot);
            }

            return null;
        }
        
        /// <summary>
        /// Returns if this object is the root of an open prefab state
        /// </summary>
        public static bool IsRootOfPrefabStage(GameObject source)
        {
            var stage = PrefabStageUtility.GetPrefabStage(source);
            return stage != null && stage.prefabContentsRoot == source;
        }
        
        #region Prefab Name Override

        /// <summary>
        /// Reverts the hidden prefab instance name override from the selection.
        /// </summary>
		[MenuItem("GameObject/Prefab/Revert Prefab Name Override")]
		private static void RevertPrefabNameMenuItem(MenuCommand menuCommand)
		{
			if (!menuCommand.ShouldExecuteForMultiSelection())
			{
				return;
			}

			var targets = Selection.transforms.Select(t => t.gameObject).ToArray();
			var progress = 0;

			try
			{
				foreach (var target in targets)
				{
					if (EditorUtility.DisplayCancelableProgressBar("Revert Prefab Name", target.name, (float)progress++ / targets.Length))
					{
						break;
					}

					RevertPrefabName(target);
				}
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}
		}

        /// <summary>
        /// Reverts the hidden prefab instance name override from the selection recursively.
        /// </summary>
		[MenuItem("GameObject/Prefab/Revert Prefab Name Override (Recursive)")]
		private static void RevertPrefabNameRecursiveMenuItem(MenuCommand menuCommand)
		{
			if (!menuCommand.ShouldExecuteForMultiSelection())
			{
				return;
			}

			var targets = Selection.transforms.Select(t => t.gameObject).SelectMany(selector: go => go.GetComponentsInChildren<Transform>(includeInactive: true).Select(selector: t => t.gameObject)).Distinct().ToArray();
			var progress = 0;

			try
			{
				foreach (var target in targets)
				{
					if (EditorUtility.DisplayCancelableProgressBar("Revert Prefab Name", target.name, (float)progress++ / targets.Length))
					{
						break;
					}

					RevertPrefabName(target);
				}
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}
		}

        /// <summary>
        /// Reverts the hidden prefab instance name override from the specified game object.
        /// </summary>
		private static bool RevertPrefabName(GameObject gameObject)
		{
            if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));

			if (!gameObject.IsSceneBound())
			{
				return false;
			}

			if (!PrefabUtility.IsPartOfPrefabInstance(gameObject))
			{
				return false;
			}

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            if (prefabStage != null && gameObject == prefabStage.prefabContentsRoot)
            {
                return false;
            }
            
			var prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);

			if (prefab == null)
			{
				return false;
			}

			Undo.RecordObject(gameObject, "Revert Prefab Name");

			gameObject.name = prefab.name;

			var propertyModifications = new List<PropertyModification>(PrefabUtility.GetPropertyModifications(gameObject));

			for (var i = 0; i < propertyModifications.Count; i++)
			{
				if (propertyModifications[i].propertyPath == "m_Name")
				{
					propertyModifications.RemoveAt(i);
				}
			}

			PrefabUtility.SetPropertyModifications(gameObject, propertyModifications.ToArray());

			return true;
		}
        
        #endregion
	}
}
