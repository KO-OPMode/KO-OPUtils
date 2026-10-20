using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KOOP
{
    /// <summary>
    /// Static helper that logs every time we load and unload scenes
    /// </summary>
    /// <owner>jonagill</owner>
    public static class SceneLoadLogger
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod()]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void InitializeOnLoad()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"Scene {scene.name} loaded in {mode} mode at {Time.realtimeSinceStartupAsDouble}s");
        }
        
        private static void OnSceneUnloaded(Scene scene)
        {
            Debug.Log($"Scene {scene.name} unloaded at {Time.realtimeSinceStartupAsDouble}s");
        }
    }
}