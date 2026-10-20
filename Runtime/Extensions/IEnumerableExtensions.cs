using System.Collections.Generic;
using UnityEngine;

namespace KOOP.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Randomly selects an element from the collection.
        /// </summary>
        public static T SelectRandom<T>(this IEnumerable<T> enumerable)
        {
            T chosenElement = default;
            float highestValue = -1;

            foreach (T element in enumerable)
            {
                float randomValue = Random.value;
                if (randomValue > highestValue)
                {
                    chosenElement = element;
                    highestValue = randomValue;
                }
            }

            return chosenElement;
        }
    }
}