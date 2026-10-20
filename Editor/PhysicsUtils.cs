using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KOOPEditor
{
    public class PhysicsUtils
    {
        public static bool TryGetPhysicsSceneOfCurrentStage(out PhysicsScene physicsScene)
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null && prefabStage.scene.IsValid())
            {
                physicsScene = prefabStage.scene.GetPhysicsScene();
                return physicsScene.IsValid();
            }
            else
            {
                for (var i = 0; i < SceneManager.loadedSceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (scene.IsValid())
                    {
                        physicsScene = scene.GetPhysicsScene();
                        if (physicsScene.IsValid())
                        {
                            return true;
                        }
                    }
                }
            }

            physicsScene = default;
            return false;
        }
    }
}
