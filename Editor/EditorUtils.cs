using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BulkEditor;
using KOOP.Extensions;
using UnityCollections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR

namespace KOOPEditor
{
    public static class EditorUtils
    {
        /// <summary>
        /// Dirties the component and any scene / prefab that it might be a part of
        /// This handles cases where just directly calling EditorUtility.SetDirty()
        /// by itself can end up not doing anything, such as dirtying an object from OnValidate()
        /// while opening a scene or prefab
        /// </summary>
        public static void SetComponentAndSceneDirty(Component component)
        {
            if (component == null) return;

            if (component.gameObject.scene.isLoaded)
            {
                // The scene is loaded -- dirtying us directly should be fine
                EditorUtility.SetDirty(component);
                return;
            }

            // If our scene isn't loaded yet, Unity is going to clear the dirty flag on the scene
            // after it finishes loading. This means we have to defer dirtying until after the scene loads
            PrefabStage objectStage = PrefabStageUtility.GetPrefabStage(component.gameObject);
            if (objectStage != null)
            {
                // Our scene is actually a prefab stage, so we have to subscribe to the right callback
                // to dirty the component once we've finished opening its stage
                void OnPrefabStageOpened(PrefabStage openedStage)
                {
                    if (openedStage == objectStage)
                    {
                        if (component != null)
                        {
                            EditorUtility.SetDirty(component);
                        }
                        PrefabStage.prefabStageOpened -= OnPrefabStageOpened;
                    }
                }
                
                PrefabStage.prefabStageOpened += OnPrefabStageOpened;
            }
            else
            {
                var objectScene = component.gameObject.scene;
                void DirtyOnSceneOpened(UnityEngine.SceneManagement.Scene openedScene, OpenSceneMode mode)
                {
                    if (openedScene == objectScene)
                    {
                        if (component != null)
                        {
                            EditorUtility.SetDirty(component);
                        }

                        EditorSceneManager.sceneOpened -= DirtyOnSceneOpened;
                    }
                }
                
                EditorSceneManager.sceneOpened += DirtyOnSceneOpened;
            }
        }

        private class SceneSetupData
        {
            public struct SelectionState
            {
                public string SceneName { get; set; }
                public string ObjectPath { get; set; }
            }
            
            public SceneSetup[] SceneSetups { get; set; }
            public string PrefabStagePath { get; set;}
            public SelectionState[] SelectionStates { get; set; }
        }
        
        /// <summary>
        /// Save our current set of open scenes to Unity's SessionState dictionary
        /// </summary>
        /// <param name="includePrefabStage">If true, we will also save the current top-level open prefab stage (if any)</param>
        public static void SaveSceneSetupToSessionState(string key, bool includePrefabStage = false, bool includeSelection = false)
        {
            // Serialize our current scene setup and save to JSON so we can restore it after the build completes
            var setupData = new SceneSetupData();
            
            setupData.SceneSetups = EditorSceneManager.GetSceneManagerSetup();
            if (setupData.SceneSetups .Length == 0)
            {
                Debug.LogWarning("No open scenes detected; editor will end up in empty scene after the build.");
            }

            if (includePrefabStage)
            {
                var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null)
                {
                    setupData.PrefabStagePath = prefabStage.assetPath;
                }
            }

            if (includeSelection)
            {
                setupData.SelectionStates = Selection.gameObjects
                    .Where(go => !EditorUtility.IsPersistent(go))
                    .Select(go => new SceneSetupData.SelectionState()
                    {
                        SceneName = go.scene.name,
                        ObjectPath = go.GetPathName()
                    }).ToArray();
            }
            
            string setupJson = JsonSerializer.Serialize(setupData);
            SessionState.SetString(key, setupJson);
        }
        
        /// <summary>
        /// Restore a previous scene setup (open scenes and prefab stages) from Unity's SessionState dictionary 
        /// </summary>
        /// <param name="clear">If true, we will automatically delete the setup from the SessionState dictionary after restoring it</param>
        public static bool TryRestoreSceneSetupFromSessionState(string key, bool clear = true)
        {
            // Load our scene setup from JSON and restore it
            string setupJson = SessionState.GetString(key, "");
            if (string.IsNullOrEmpty(setupJson))
            {
                return false;
            }

            var setupData = JsonSerializer.Deserialize<SceneSetupData>(setupJson);
            if (setupData.SceneSetups.Length > 0)
            {
                EditorSceneManager.RestoreSceneManagerSetup(setupData.SceneSetups);
            }

            if (!string.IsNullOrEmpty(setupData.PrefabStagePath))
            {
                PrefabStageUtility.OpenPrefab(setupData.PrefabStagePath);
            }

            if (setupData.SelectionStates != null && setupData.SelectionStates.Length > 0)
            {
                using PooledList<Object> objectsToSelect = new();
                foreach (var selectionState in setupData.SelectionStates)
                {
                    // Find the scene our selected object was inside
                    if (TryGetSceneOrStageByName(selectionState.SceneName, out var scene) && 
                        scene.IsValid() && 
                        scene.isLoaded)
                    {
                        var pathTokens = selectionState.ObjectPath.Split('/');
                        var rootObjects = scene.GetRootGameObjects();
                        foreach (var rootObject in rootObjects)
                        {
                            // Find the root object that matches our path
                            if (rootObject.name == pathTokens[0])
                            {
                                var pathFromRoot = string.Join('/', pathTokens.Skip(1));
                                if (string.IsNullOrEmpty(pathFromRoot))
                                {
                                    // The root object is itself our taget
                                    objectsToSelect.Add(rootObject);
                                    break;
                                }
                                
                                // Try to find our object relative to the root using the rest of our path
                                var selectedTransform = rootObject.transform.Find(pathFromRoot);
                                if (selectedTransform != null) 
                                {
                                    objectsToSelect.Add(selectedTransform.gameObject);
                                }
                                
                                break;
                            }
                        }
                    }
                }

                if (objectsToSelect.Count > 0)
                {
                    Selection.objects = objectsToSelect.ToArray();
                }
            }

            if (clear)
            {
                ClearSceneSetupFromSessionState(key);
            }

            return true;
        }

        /// <summary>
        /// Clears a previous saved scene setup (open scenes and prefab stages) from Unity's SessionState dictionary 
        /// </summary>
        public static void ClearSceneSetupFromSessionState(string key)
        {
            SessionState.EraseString(key);
        }

        /// <summary>
        /// Return the open scene or prefab stage that matches the given scene or prefab asset name
        /// </summary>
        public static bool TryGetSceneOrStageByName(string name, out Scene scene)
        {
            scene = SceneManager.GetSceneByName(name);
            if (scene.IsValid())
            {
                return true;
            }
            
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage.scene.name == name)
            {
                scene = stage.scene;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Similar to calling <see cref="StageUtility.GetCurrentStage()"/> and <see cref="Stage.FindComponentsOfType()"/>,
        /// but allows us to re-use the same list of components on each call.
        /// </summary>
        public static void GetComponentsInCurrentStageOrScene<T>(List<T> components, bool includeInactive = false)
        {
            components.Clear();
            
            using PooledList<GameObject> rootObjects = new();
            var currentPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (currentPrefabStage != null) 
            {
                rootObjects.Add(currentPrefabStage.prefabContentsRoot);
            }
            else
            {
                for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                {
                    var scene = EditorSceneManager.GetSceneAt(i);
                    if (scene.IsValid() && scene.isLoaded)
                    {
                        rootObjects.AddRange(scene.GetRootGameObjects());
                    }
                }
            }

            foreach (var rootObject in rootObjects)
            {
                components.AddRange(rootObject.GetComponentsInChildren<T>(includeInactive));
            }
        }

        /// <summary>
        /// Finds all components of the given type in any open stage or scene.
        /// It does this by getting the current stage or scene, and then walking recursively upwards through
        /// all of the other stages that this stage was opened from (such as if we have opened a prefab for editing
        /// in context by clicking on the little arrow in the Hierarchy view).
        /// </summary>
        public static void GetComponentsInAnyOpenStageOrScene<T>(List<T> components, bool includeInactive = false)
        {
            // Get components in our current scene
            GetComponentsInCurrentStageOrScene(components, includeInactive);
            
            var currentPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (currentPrefabStage != null && currentPrefabStage.openedFromInstanceObject != null) 
            {
                AppendComponentsInObjectStageOrSceneRecursive(currentPrefabStage.openedFromInstanceObject, components, includeInactive);
            }
        }

        private static void AppendComponentsInObjectStageOrSceneRecursive<T>(
            GameObject gameObject, 
            List<T> components,
            bool includeInactive)
        {
            if (gameObject == null)
                return;
            
            AppendComponentsInObjectScene(gameObject, components, includeInactive);

            var prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
            if (prefabStage != null)
            {
                AppendComponentsInObjectStageOrSceneRecursive<T>(prefabStage.openedFromInstanceObject, components, includeInactive);
            }
        }
        
        private static void AppendComponentsInObjectScene<T>(GameObject gameObject, List<T> components, bool includeInactive)
        {
            var scene = gameObject.scene;
            if (scene.IsValid() && scene.isLoaded)
            {
                foreach (var rootObject in scene.GetRootGameObjects())
                {
                    components.AddRange(rootObject.GetComponentsInChildren<T>(includeInactive));
                }
            }
        }


        /// <summary>
        /// Searches for the given component this object or any parent.
        /// If this game object is within a PrefabStage, we will also search for the given component
        /// in the hierarchy of the instance that the PrefabStage was opened from.
        /// </summary>
        public static T GetComponentInParentFromWithinPrefabStage<T>(GameObject gameObject)
        {
            if (gameObject == null)
                return default;
            
            var component = gameObject.GetComponentInParent<T>(true);
            if (component != null)
                return component;

            var stage = PrefabStageUtility.GetPrefabStage(gameObject);
            if (stage != null)
            {
                // Recurse in case we are actually through several layers of stages
                return GetComponentInParentFromWithinPrefabStage<T>(stage.openedFromInstanceObject);
            }

            return default;
        }

        /// <inheritdoc cref="GetComponentInParentFromWithinPrefabStage"/>
        public static T EditorGetComponentInParentFromWithinPrefabStage<T>(this GameObject gameObject)
        {
            return GetComponentInParentFromWithinPrefabStage<T>(gameObject);
        }
        
        /// <inheritdoc cref="GetComponentInParentFromWithinPrefabStage"/>
        public static T EditorGetComponentInParentFromWithinPrefabStage<T>(this Component component)
        {
            return GetComponentInParentFromWithinPrefabStage<T>(component.gameObject);
        }

        /// <summary>
        /// Returns whether the specified object lives in the scene.
        /// </summary>
        /// <seealso cref="EditorUtility.IsPersistent"/>
        public static bool IsSceneBound(this UnityEngine.Object uo)
        {
            // This is more accurate than !AssetDatabase.Contains, because this will also exclude 
            // in-memory objects like "new Material()" or "ScriptableObject.Instantiate"
            return !EditorUtility.IsPersistent(uo);
        }
    }
}

#endif