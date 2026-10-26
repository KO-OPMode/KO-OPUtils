using UnityEngine;

namespace KOOP.Extensions
{
    public static class ScreenExtensions
    {
        public static bool Matches(this Resolution a, Resolution b)
        {
            return a.width == b.width && 
                a.height == b.height && 
                Mathf.Approximately((float) a.refreshRateRatio.value, (float) b.refreshRateRatio.value);
        }

        public static bool TryGetCurrentResolutionIndex(out Resolution currentResolution, out int index)
        {
            index = -1;
            currentResolution = Screen.currentResolution;
            var resolutions = Screen.resolutions;
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (currentResolution.Matches(resolutions[i]))
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetClosestSupportedResolution(Vector2Int dimensions, out Resolution supportedResolution)
        {
            // Get our current resolutions -- this copies the array from the engine,
            // but we shouldn't call it enough to matter too much
            var resolutions = Screen.resolutions;

            supportedResolution = default;
            int minDistance = Int32.MaxValue;
            bool anyFound = false;

            // Do a simplified version of the best-fit checks performed by Screen.SetResolution()
            // See FindClosestResolutionForExclusiveFullscreen() in WinScreenSetup.cpp in the engine code
            foreach (var resolution in resolutions)
            {
                int distance = Mathf.Abs(dimensions.x - resolution.width) + Mathf.Abs(dimensions.y - resolution.height);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    supportedResolution = resolution;
                    anyFound = true;
                }
            }

            return anyFound;
        }
    }
}