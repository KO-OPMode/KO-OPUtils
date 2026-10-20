using UnityEngine;
using System;

namespace KOOP.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Returns the next value of an enum, and wraps around if at the end.
        /// </summary>
        public static T Next<T>(this T target) where T : struct, Enum
        {
            T[] arr = (T[])Enum.GetValues(target.GetType());

            int j = RealModulus((Array.IndexOf<T>(arr, target) + 1), arr.Length);

            return arr[j];
        }
        
        /// <summary>
        /// Returns the previous value of an enum, and wraps around if at the end.
        /// </summary>
        public static T Previous<T>(this T target) where T : struct, Enum
        {
            T[] arr = (T[])Enum.GetValues(target.GetType());

            int j = RealModulus((Array.IndexOf<T>(arr, target) - 1), arr.Length);

            return arr[j];
        }
        
        /// <summary>
        /// Gives the mathematical modulus, i.e., the coset representative of a factor group. In layman's terms, it "loops"
        /// correctly for negative numbers, instead of reversed, and the result is always positive.
        /// </summary>
        private static int RealModulus(int a, int b)
        {
            return ((a % b) + b) % b;
        }
    }
}