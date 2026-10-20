using UnityEngine;

namespace KOOP.Extensions
{
    public static class DoubleExtensions
    {
        /// <summary>
        /// Maps the given value from its normalized position in the source range to the corresponding position in the destination range.
        /// </summary>
        public static double Remap(this double value, double srcMin, double srcMax, double dstMin, double dstMax)
        {
            //var t = Mathf.InverseLerp(srcMin, srcMax, value);
            //return Mathf.Lerp(dstMin, dstMax, t);
            double t = (value - srcMin) / (srcMax - srcMin);
            return dstMin + (dstMax - dstMin) * value.InverseLerp(srcMin, srcMax);
        }

        /// <summary>
        /// Maps the given value from its normalized position in the source range to the corresponding position in the destination range.
        /// </summary>
        public static double Remap(this double value, Vector2 src, Vector2 dst)
        {
            return value.Remap(src.x, src.y, dst.x, dst.y);
        }

        /// <summary>
        /// Remaps value from 0-1 based on position between A and B
        /// </summary>
        public static double InverseLerp(this double value, double A, double B)
        {
            return (value - A) / (B - A);
        }

        public static double LerpTo(this float value, double A, double B)
        {
            return A + (B - A) * value;
        }
    }
}