using UnityEngine;
using System;
using System.Collections.Generic;

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

        public static bool TryGetMatchingSupportedResolutionIndex(Resolution resolution, out int index)
        {
            return TryGetMatchingResolutionIndex(Screen.resolutions, resolution, out index);
        }
        
        public static bool TryGetMatchingResolutionIndex(IReadOnlyList<Resolution> resolutions, Resolution resolution, out int index)
        {
            index = -1;
            for (int i = 0; i < resolutions.Count; i++)
            {
                if (resolution.Matches(resolutions[i]))
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }
        
        public static bool TryGetClosestSupportedResolutionIndex(Vector2Int dimensions, out Resolution supportedResolution, out int index)
        {
            return TryGetClosestResolutionIndex(dimensions, Screen.resolutions, out supportedResolution, out index);
        }

        public static bool TryGetClosestResolutionIndex(Vector2Int dimensions, IReadOnlyList<Resolution> resolutions, out Resolution supportedResolution, out int index)
        {
            index = -1;
            supportedResolution = default;
            int minDistance = Int32.MaxValue;

            // Do a simplified version of the best-fit checks performed by Screen.SetResolution()
            // See FindClosestResolutionForExclusiveFullscreen() in WinScreenSetup.cpp in the engine code
            for (int i = 0; i < resolutions.Count; i++)
            {
                var resolution = resolutions[i];
                int distance = Mathf.Abs(dimensions.x - resolution.width) + Mathf.Abs(dimensions.y - resolution.height);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    supportedResolution = resolution;
                    index = i;
                }
            }

            return index >= 0;
        }
    }
}