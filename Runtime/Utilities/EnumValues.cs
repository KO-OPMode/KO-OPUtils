using System;
using System.Collections.Generic;
using System.Linq;

namespace KOOP.Utils
{
    /// <summary>
    /// Caches the result of <see cref="Enum.GetValues"/> in the generic type cache using the type initializer so all subsequent calls are free.
    /// </summary>
    /// <owner>Lazlo</owner>
    public static class EnumValues<T> where T : Enum
    {
        private static readonly T[] _values;

        public static IReadOnlyList<T> Values => _values;

        static EnumValues()
        {
            _values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        }
    }
}