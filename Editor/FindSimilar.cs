#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace KOOPEditor
{
    public static class FindSimilar
    {
        private static EditorWindow _projectWindow;
        private static MethodInfo _setSearchMethod; private const string _setSearchMethodName = "SetSearch";

        [MenuItem("Assets/Find Similar (Name)", false, 100)]
        private static void FindSimilarItemsName(MenuCommand command)
        {
            if (!Selection.activeObject) return;

            string assetName = Selection.activeObject.name;
            SetProjectSearch(assetName);
        }

        [MenuItem("Assets/Find Similar (Name)", true, 100)]
        private static bool FindSimilarItemsName_Validate(MenuCommand command)
        {
            if (!Selection.activeObject) return false;
            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(Selection.activeObject))) return false;

            return true;
        }

        [MenuItem("Assets/Find Similar (Name, Remove Prefix)", false, 101)]
        private static void FindSimilarItemsNameRemovePrefix(MenuCommand command)
        {
            if (!Selection.activeObject) return;

            string assetName = Selection.activeObject.name;

            if (assetName.Contains('_'))
            {
                // has prefix, probably
                assetName = assetName.Substring(assetName.IndexOf('_') + 1);
            }

            SetProjectSearch(assetName);
        }

        [MenuItem("Assets/Find Similar (Name, Remove Prefix)", true, 101)]
        private static bool FindSimilarItemsNameRemovePrefix_Validate(MenuCommand command)
        {
            if (!Selection.activeObject) return false;
            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(Selection.activeObject))) return false;

            return true;
        }

        [MenuItem("Assets/Find Similar (Asset Type)", false, 102)]
        private static void FindSimilarItemsAssetType(MenuCommand command)
        {
            if (!Selection.activeObject) return;
            SetProjectSearch($"t:{Selection.activeObject.GetType().Name}");
        }

        [MenuItem("Assets/Find Similar (Asset Type)", true, 102)]
        private static bool FindSimilarItemsAssetType_Validate(MenuCommand command)
        {
            if (!Selection.activeObject) return false;
            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(Selection.activeObject))) return false;

            return true;
        }
        
        private static void FindProjectWindow()
        {
            if (_projectWindow) return;

            EditorWindow[] windows = (EditorWindow[])Resources.FindObjectsOfTypeAll(typeof(EditorWindow));
            foreach (EditorWindow window in windows)
            {
                if (window.GetType().ToString() == "UnityEditor.ProjectBrowser")
                {
                    _projectWindow = window;
                    return;
                }
            }
        }
        
        private static void SetProjectSearch(string filter)
        {
            FindProjectWindow();
            if (!_projectWindow) return;

            if (_setSearchMethod == null)
            {
                _setSearchMethod = _projectWindow.GetType().GetMethod(_setSearchMethodName, new[] { typeof(string) });
            }
            
            _setSearchMethod.Invoke(_projectWindow, new object[] { filter });
        }
    }
}

#endif