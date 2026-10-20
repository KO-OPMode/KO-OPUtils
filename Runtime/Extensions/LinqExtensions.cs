using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace KOOP.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }

        private static string ToSeparatedString(this IEnumerable enumerable, string separator)
        {
            return string.Join(separator, enumerable.Cast<object>().Select(o => o?.ToString() ?? "(null)").ToArray());
        }

        /// <summary>
        /// Returns the concatenated string version of an enumerable separated by commas.
        /// Useful for debugging.
        /// </summary>
        public static string ToCommaSeparatedString(this IEnumerable enumerable)
        {
            return ToSeparatedString(enumerable, ", ");
        }

        /// <summary>
        /// Returns the concatenated string version of an enumerable separated by new-lines.
        /// Useful for debugging.
        /// </summary>
        public static string ToLineSeparatedString(this IEnumerable enumerable)
        {
            return ToSeparatedString(enumerable, Environment.NewLine);
        }

        /// <summary>
        /// Forces an immediate evaluation of the given source; i.e. if the source is already a collection,
        /// returns itself; otherwise, allocate and return a collection from the source.
        /// </summary>
        public static IReadOnlyCollection<T> ForceEvaluation<T>(this IEnumerable<T> source)
        {
            if (source == null) return null;
            
            if (source is IReadOnlyCollection<T> collection)
            {
                return collection;
            }
            else
            {
                return source.ToArray();
            }
        }

        /// <summary>
        /// Forces an immediate evaluation of the given source to a list; i.e. if the source is already a list,
        /// returns itself; otherwise, allocate and return a list from the source.
        /// </summary>
        public static IReadOnlyList<T> ForceListEvaluation<T>(this IEnumerable<T> source)
        {
            if (source == null) return null;
            
            if (source is IReadOnlyList<T> list)
            {
                return list;
            }
            else
            {
                return source.ToArray();
            }
        }

        /// <summary>
        /// Attempts to find the first element in the source matching a predicate.
        /// </summary>
        public static bool TryFindFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T first)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            foreach (var element in source)
            {
                if (predicate(element))
                {
                    first = element;
                    return true;
                }
            }

            first = default;
            return false;
        }

        /// <summary>
        /// Attempts to find the single element in the source matching a predicate, optionally throwing if multiple are found.
        /// </summary>
        public static bool TryFindSingle<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T single, bool throwOnMultiple)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            single = default;
            var found = false;
            
            foreach (var element in source)
            {
                if (predicate(element))
                {
                    if (!found)
                    {
                        single = element;
                        found = true;
                    }
                    else
                    {
                        if (throwOnMultiple)
                        {
                            throw new InvalidOperationException("More than one element match the provided predicate.");
                        }
                        
                        return false;
                    }
                }
            }

            return found;
        }

        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> sequence) where T : class
        {
            return sequence.Where(obj => obj != null);
        }

        /// <remarks>
        /// Use this overload variant when using a <see cref="T"/> that is not explicitly derived from <see cref="UnityEngine.Object"/>
        /// (such as an interface or plainly <see cref="System.Object"/>) to make sure that we are using Unity's fake-null checking
        /// despite it not being inferrable at compile time.
        /// </remarks> 
        public static IEnumerable<T> NotUnityNull<T>(this IEnumerable<T> sequence) where T : class
        {
            return sequence.Where(obj => !obj.IsUnityNull());
        }
    }
}