using System;
using System.Collections.Generic;
using System.Linq;

namespace KOOP.Utils
{
    public static class EnumUtils
    {
        /// <summary>
        /// Returns a random value of enum <typeparamref name="T"/>.
        /// </summary>
        public static T Random<T>() where T : Enum
        {
            var values = Values<T>();
            var value = values[UnityEngine.Random.Range(0, values.Count)];
            return value;
        }

        /// <summary>
        /// Returns all values of the specified value type. Cached and efficient.
        /// </summary>
        public static IReadOnlyList<T> Values<T>() where T : Enum
        {
            return EnumValues<T>.Values;
        }
    }
}