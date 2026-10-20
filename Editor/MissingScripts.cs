#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using KOOP.Extensions;

using Object = UnityEngine.Object;

namespace KOOPEditor
{
    /// <summary>
    /// Adapted from <a href="https://forum.unity.com/threads/made-you-an-amazing-find-missing-scripts-script.1463357/">this</a>
    /// Unity forum thread.
    /// </summary>
    public class MissingScripts : EditorWindow
    {
        private readonly List<string> _missingPrefabAssets = new();
        private readonly List<string> _objectsWithMissingScriptsInAllScenes = new();
        private readonly List<GameObject> _objectsWithMissingScriptsInCurrentScene = new();

        private Vector2 scrollPos;

        [MenuItem("Tools/Find missing scripts")]
        public static void ShowWindow()
        {
            GetWindow(typeof(MissingScripts));
        }

        private void OnGUI()
        {
            GUILayout.Label("Find Missing Scripts", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Clear all results"))
            {
                _missingPrefabAssets.Clear();
                _objectsWithMissingScriptsInAllScenes.Clear();
                _objectsWithMissingScriptsInCurrentScene.Clear();
            }

            var myBoxStyle = new GUIStyle(GUI.skin.box)
            {
                normal =
                {
                    background = MakeTex(2, 2, new Color(0.6f, 0.6f, 0.6f, 0.5f))
                }
            };

            using (var sv = new GUILayout.ScrollViewScope(scrollPos))
            {
                scrollPos = sv.scrollPosition;

                // --------------------------------------------------------------------- scene block
                using (new GUILayout.VerticalScope(myBoxStyle))
                {
                    if (GUILayout.Button("Find in current scene"))
                    {
                        FindMissingScriptsInCurrentScene();
                    }
                    GUI.enabled = _objectsWithMissingScriptsInCurrentScene.Count > 0;
                    if (GUILayout.Button("Copy to clipboard"))
                    {
                        GUIUtility.systemCopyBuffer = GetClipboardString(_objectsWithMissingScriptsInCurrentScene);
                    }
                    GUI.enabled = true;

                    GUILayout.Label("Results (Open Scenes):", EditorStyles.boldLabel);
                    for (var i = _objectsWithMissingScriptsInCurrentScene.Count - 1; i >= 0; --i)
                    {
                        if (!_objectsWithMissingScriptsInCurrentScene[i])
                        {
                            _objectsWithMissingScriptsInCurrentScene.RemoveAt(i);
                        }
                    }

                    foreach (var go in _objectsWithMissingScriptsInCurrentScene.Where(go => GUILayout.Button(go.name)))
                    {
                        EditorGUIUtility.PingObject(go);
                    }
                }

                GUILayout.Space(20);

                // --------------------------------------------------------------------- assets block
                using (new GUILayout.VerticalScope(myBoxStyle))
                {
                    if (GUILayout.Button("Find in assets"))
                    {
                        FindMissingScriptsInAssets();
                    }
                    GUI.enabled = _missingPrefabAssets.Count > 0;
                    if (GUILayout.Button("Copy to clipboard"))
                    {
                        GUIUtility.systemCopyBuffer = GetClipboardString(_missingPrefabAssets);
                    }
                    GUI.enabled = true;

                    GUILayout.Label("Results (Assets):", EditorStyles.boldLabel);
                    foreach (var path in _missingPrefabAssets.Where(path => GUILayout.Button(path)))
                    {
                        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(path));
                    }
                }

                GUILayout.Space(20);

                // --------------------------------------------------------------------- all scenes block
                using (new GUILayout.VerticalScope(myBoxStyle))
                {
                    if (GUILayout.Button("Find in all scenes"))
                    {
                        FindMissingScriptsInAllScenes();
                    }
                    GUI.enabled = _objectsWithMissingScriptsInAllScenes.Count > 0;
                    if (GUILayout.Button("Copy to clipboard"))
                    {
                        GUIUtility.systemCopyBuffer = GetClipboardString(_objectsWithMissingScriptsInAllScenes);
                    }
                    GUI.enabled = true;

                    GUILayout.Label("Results (All scenes):", EditorStyles.boldLabel);
                    foreach (var objPath in _objectsWithMissingScriptsInAllScenes)
                    {
                        GUILayout.TextField(objPath);
                    }
                }
            }
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            var pix = new Color[width * height];
            for (var i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }

            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void FindMissingScriptsInCurrentScene()
        {
            _objectsWithMissingScriptsInCurrentScene.Clear();
            var allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var go in allObjects)
            {
                if (go.transform.parent == null) // Only start with root objects
                {
                    FindMissingScriptsInGameObjectAndChildren(go);
                }
            }
        }

        private void FindMissingScriptsInAllScenes()
        {
            if (!EditorUtility.DisplayDialog("Search all scenes?",
                "Are you sure you want to search all scenes for missing scripts? This process may take seconds or minutes.",
                "Ok",
                "Cancel"))
            {
                return;
            }

            var ogScenePath = SceneManager.GetActiveScene().path;

            _objectsWithMissingScriptsInAllScenes.Clear();

            void FindMissing(GameObject go, string sceneName)
            {
                var components = go.GetComponents<Component>();
                var hasMissingScript = components.Any(c => c == null);
                if (hasMissingScript)
                {
                    _objectsWithMissingScriptsInAllScenes.Add($"{sceneName}/{go.transform.GetHierarchyPath()}");
                }

                foreach (Transform child in go.transform) // Recursively check children
                {
                    FindMissing(child.gameObject, sceneName);
                }
            }

            var allAssets = AssetDatabase.GetAllAssetPaths();
            foreach (var assetPath in allAssets)
            {
                if (Path.GetExtension(assetPath) == ".unity") // && assetPath.Contains(EditorConstants.ScenesFolder))
                {
                    try
                    {
                        var scene = EditorSceneManager.OpenScene(assetPath);
                        foreach (var rootObj in scene.GetRootGameObjects())
                        {
                            FindMissing(rootObj, scene.name);
                        }
                    }
                    catch (ArgumentException e)
                    {
                        Debug.LogError(e);
                    }
                }
            }

            Debug.Log($"Found {_objectsWithMissingScriptsInAllScenes.Count} missing scripts.");
            EditorSceneManager.OpenScene(ogScenePath);
        }

        private void FindMissingScriptsInGameObjectAndChildren(GameObject go)
        {
            var components = go.GetComponents<Component>();
            var hasMissingScript = components.Any(c => c == null);
            if (hasMissingScript)
            {
                _objectsWithMissingScriptsInCurrentScene.Add(go);
            }

            foreach (Transform child in go.transform) // Recursively check children
            {
                FindMissingScriptsInGameObjectAndChildren(child.gameObject);
            }
        }

        private void FindMissingScriptsInAssets()
        {
            _missingPrefabAssets.Clear();
            var allAssets = AssetDatabase.GetAllAssetPaths();
            foreach (var assetPath in allAssets)
            {
                if (assetPath.StartsWith("Packages/"))
                {
                    continue;
                }

                if (Path.GetExtension(assetPath) == ".prefab")
                {
                    var assetRoot = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (assetRoot==null)
                        Debug.Log($"{assetPath} is busted!");
                    else
                    {
                        var components = assetRoot.GetComponentsInChildren<Component>(true);
                        var hasMissingScript = components.Any(c => c == null);
                        if (hasMissingScript)
                        {
                            _missingPrefabAssets.Add(assetPath);
                        }
                    }
                }
            }
        }

        private static string GetClipboardString<T>(List<T> list)
        {
            var str = "";

            foreach (var s in list)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    str += "\n";
                }
                str += s;
            }

            return str;
        }
        
        /// <summary>
        /// Get the full GameObject hierarchy of a given object in a Unity scene.
        /// </summary>
        /// <param name="obj">the object to examine</param>
        /// <returns>A '/'-separated string, showing the path from a scene's root object, to <paramref name="obj"/>'s location.</returns>
        private static string GetGameObjectPath(GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }
    }
}

#endif