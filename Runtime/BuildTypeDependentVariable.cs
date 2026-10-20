
using UnityEngine;

namespace KOOP
{
    /// <summary>
    /// Simple helper class that lets you have build-type dependent variables.
    /// Specifically: Editor, Development Build, Release Build
    /// </summary>
    [System.Serializable]
    public class BuildTypeDependent<T>
    {
        public T Editor = default;
        public T DevelopmentBuild = default;
        public T ReleaseBuild = default;

        public static implicit operator T(BuildTypeDependent<T> flag)
        {
            #if UNITY_EDITOR
                return flag.Editor;
            #elif DEVELOPMENT_BUILD
                return flag.DevelopmentBuild;
            #else
                return flag.ReleaseBuild;
            #endif
        }
    }
}