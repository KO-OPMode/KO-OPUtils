using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KOOP.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> for iterating over the values of public constants of a given type declared in a type.
        /// </summary>
        public static IEnumerable<T> GetPublicConstantValues<T>(this Type type)
        {
            return type.GetPublicConstants()
                .Where(x => x.FieldType.IsAssignableFrom(typeof(T)))
                .Select(x => (T)x.GetRawConstantValue());
        }

        /// <summary>
        /// Returns an <see cref="IEnumerable{FieldInfo}"/> for iterating over the public constants declared in a type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> GetPublicConstants(this Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(x => x.IsLiteral && !x.IsInitOnly);
        }
    }
}