using UnityEngine;

namespace KOOP.Extensions
{
    public static class FloatExtensions
    {
        /// <summary>
        /// Returns the fractional component of the value.
        /// </summary>
        public static float Frac(float x)
        {
            return x - Mathf.Floor(x);
        }

        /// <summary>
        /// Returns the fractional component of the value.
        /// </summary>
        /// <param name="keepSign">Should the value's sign be included? Eg. -6.9 returns -0.9 if true.</param>
        public static float Frac(this float value, bool keepSign = true)
        {
            return keepSign ? Frac(value) : Mathf.Abs(Frac(value));
        }

        /// <summary>
        /// Rounds a value to an integer whose absolute value is greater than or equal to the <paramref name="value"/>.
        /// </summary>
        /// <remarks>
        /// This differs from <see cref="Mathf.Ceil"/> which would round -0.5 up to 0 instead of down to -1.
        /// This does mean that returning a value of 0 is only possible with an initial value of 0.
        /// </remarks>
        public static float MagnitudeCeil(this float value)
        {
            if (value >= 0)
                return Mathf.Ceil(value);
            return -Mathf.Ceil(-value);
        }

        /// <summary>
        /// Rounds a value to an integer whose absolute value is less than or equal to the <paramref name="value"/>.
        /// </summary>
        /// <remarks>
        /// This differs from <see cref="Mathf.Floor"/> which would round -0.5 down to -1 instead of up to 0.
        /// This does mean that any value between -1 and 1 (exclusive) will return 0.
        /// </remarks>
        public static float MagnitudeFloor(this float value)
        {
            if (value >= 0)
                return Mathf.Floor(value);
            return -Mathf.Floor(-value);
        }

        public static bool Between(this float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        public static bool Between(this double value, double min, double max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Maps the given value from its normalized position in the source range to the corresponding position in the destination range.
        /// </summary>
        public static float Remap(this float value, float srcMin, float srcMax, float dstMin, float dstMax)
        {
            var t = Mathf.InverseLerp(srcMin, srcMax, value);
            return Mathf.Lerp(dstMin, dstMax, t);
        }

        /// <summary>
        /// Maps the given value from its normalized position in the source range to the corresponding position in the destination range.
        /// </summary>
        public static float Remap(this float value, Vector2 src, Vector2 dst)
        {
            var t = Mathf.InverseLerp(src.x, src.y, value);
            return Mathf.Lerp(dst.x, dst.y, t);
        }

        /// <summary>
        /// Returns a difference between two angles comprised between -180f and 180f (comparing 5 and 355 will return -10 instead of 350)
        /// </summary>
        public static float AngleDifference(float angle1, float angle2)
        {
            float diff = (angle2 - angle1 + 180f) % 360f - 180f;
            return diff < -180f ? diff + 360f : diff;
        }

        public static float ShortestLoopingDiff(float current, float target, float max)
        {
            float full = max;
            float half = max / 2f;

            float diff = (target - current + half) % full - half;
            return diff < -half ? diff + full : diff;
        }

        /// <summary>
        /// Can lerp towards a targetvalue, but if the difference between the two is below snap, it'll automatically match target value
        /// </summary>
        /// <param name="value">current float value</param>
        /// <param name="target">value to reach</param>
        /// <param name="lerp">lerp ratio</param>
        /// <param name="snap">snap threshold to automatically match target</param>
        /// <returns></returns>
        public static float SmoothSnapLerp(this float value, float target, float lerp, float snap)
        {
            if (Mathf.Abs(value - target) < snap)
                value = target;
            else
                value += (target - value) * Mathf.Clamp01(lerp);

            return value;
        }

        /// <summary>
        /// Clamp a value between the bounds provided by a Vector2
        /// </summary>
        /// <param name="value">value to be clamped</param>
        /// <param name="clampRange">Range for clamping, where x is Min and y is Max </param>
        /// <returns></returns>
        public static float Clamp(this float value, Vector2 clampRange)
        {
            return Mathf.Clamp(value, clampRange.x, clampRange.y);
        }

        /// <summary>
        /// Rounds a value to a nearest multiple.
        /// </summary>
        public static float RoundToMultiple(this float value, float multiple)
        {
            return Mathf.Round(value / multiple) * multiple;
        }

		/// <summary>
		/// Converts a distance in world-units to a simplified string label with rounded values
		/// </summary>
		public static string ToSpaceDistanceString(this float value)
		{
			if (value < 100)
			{
				//Ex: 72.7 => 73m
				return $"{Mathf.Round(value)}m";
			}
			else if (value < 1000)
			{
				//Ex: 936.2 => 940m
				return $"{Mathf.Round(value / 10) * 10}m";
			}
			else
			{
				//Ex: 7430.2 => 7.4km
				return $"{(value / 1000):0.0}km";
			}
		}
	}
}