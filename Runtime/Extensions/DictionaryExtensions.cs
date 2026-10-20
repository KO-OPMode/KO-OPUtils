using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Random = UnityEngine.Random;

namespace KOOP.Extensions
{
    public static class DictionaryExtensions
    {
        public static void IncrementValue<TKey>(this IDictionary<TKey, int> dictionary, TKey key, int delta = 1)
        {
            if (dictionary.TryGetValue(key, out int value))
            {
                value += delta;
            }
            else
            {
                value = delta;
            }

            dictionary[key] = value;
        }
    }
}