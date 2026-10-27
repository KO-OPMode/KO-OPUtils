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

        public static Vector2Int Dimensions(this Resolution resolution)
        {
            return new Vector2Int(resolution.width, resolution.height);
        }

        /// <summary>
        /// The current resolution of the game window.
        /// This should generally be used instead of <see cref="Screen.currentResolution"/>, which confusingly
        /// returns the monitor resolution in windowed mode.
        /// </summary>
        public static Vector2Int CurrentDimensions()
        {
            return new Vector2Int(Screen.width, Screen.height);
        }
        
        /// <summary>
        /// Returns which of the supported dimensions are closest in overall size to the proposed dimensions
        /// by performing a simplified version of the matching logic Unity performs in Screen.SetResolution()
        /// </summary>
        public static bool TryGetClosestDimensionsIndex(Vector2Int dimensions, IReadOnlyList<Vector2Int> supportedDimensions, out Vector2Int closestDimensions, out int index)
        {
            index = -1;
            closestDimensions = default;
            int minDistance = Int32.MaxValue;

            // Do a simplified version of the best-fit checks performed by Screen.SetResolution()
            // See FindClosestResolutionForExclusiveFullscreen() in WinScreenSetup.cpp in the engine code
            for (int i = 0; i < supportedDimensions.Count; i++)
            {
                var possibleDimensions = supportedDimensions[i];
                int distance = Mathf.Abs(dimensions.x - possibleDimensions.x) + Mathf.Abs(dimensions.y - possibleDimensions.y);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestDimensions = possibleDimensions;
                    index = i;
                }
            }

            return index >= 0;
        }
    }
}