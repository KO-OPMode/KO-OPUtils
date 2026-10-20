using System;
using System.Collections.Generic;
using System.Linq;

namespace KOOP
{
    /// <summary>
    /// Utility class to access a cached list of all values of a specified enum, without incurring reflection cost. 
    /// </summary>
    public static class EnumValues<T> where T : Enum
    {
        private static readonly T[] _values;

        public static IReadOnlyCollection<T> Values => _values;
        
        static EnumValues()
        {
            _values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        }
    }
}

