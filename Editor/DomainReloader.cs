#if UNITY_EDITOR

using UnityEditor;

namespace KOOPEditor
{
    public static class DomainReloader
    {
        [MenuItem("Assets/Reload Domain %&r", false, 1000)]
        public static void ReloadDomain()
        {
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }
    }
}

#endif