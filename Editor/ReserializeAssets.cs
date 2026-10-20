#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace KOOPEditor
{
    public static class ReserializeAssets
    {
        [MenuItem("Assets/Reserialize", false, 40)]
        private static void ReserializeAssetsCommand(MenuCommand command)
        {
            List<string> assetPaths = new();
            foreach (var asset in Selection.objects)
            {
                var assetPath = AssetDatabase.GetAssetPath(asset);
                if (string.IsNullOrEmpty(assetPath))
                {
                    continue;
                }

                assetPaths.Add(assetPath);
                AssetDatabase.ForceReserializeAssets(assetPaths, ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata);
            }
        }
    }
}

#endif