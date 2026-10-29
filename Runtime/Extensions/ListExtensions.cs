using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace KOOP.Extensions
{
    public static class ListExtensions
    {
        public static void Move<T>(this List<T> list, int oldIndex, int newIndex)
        {
            var item = list[oldIndex];

            list.RemoveAt(oldIndex);

            if (newIndex > oldIndex) newIndex--;

            list.Insert(newIndex, item);
        }

        /// <summary>
        /// Returns whether <paramref name="index"/> is in range of <paramref name="list"/>.
        /// </summary>
        public static bool IsInRange(this IList list, int index)
        {
            return list.Count > 0 && index >= 0 && index < list.Count;
        }

        /// <summary>
        /// Remove all the elements of the list where the filter function returns true
        /// </summary>
        public static void RemoveWhere<T>(this IList<T> list, Func<T, bool> filter)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (filter(list[i]))
                {
                    list.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// Add element if it does not exist in this collection. Requires an O(n) traversal.
        /// </summary>
        /// <returns>Whether we added the value</returns>
        public static bool AddUnique<T>(this IList<T> list, T value)
        {
            if (!list.Contains(value))
            {
                list.Add(value);
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Adds each element if it does not exist in this collection. Requires an O(n^2) traversal.
        /// </summary>
        /// <returns>The number of values added</returns>
        public static int AddRangeUnique<T>(this IList<T> list, IEnumerable<T> values)
        {
            int added = 0;
            foreach (var value in values)
            {
                if (list.AddUnique(value))
                {
                    added++;
                }
            }

            return added;
        }

        /// <summary>
        /// Returns true if the item exists in the list and false if not
        /// </summary>
        public static bool TryGetIndex<T>(this IReadOnlyList<T> list, T value, out int index)
        {
            if (list is List<T> typedList)
            {
                // Use the official C# IndexOf()
                index = typedList.IndexOf(value);
            }
            else
            {
                // Use our custom IndexOf()
                index = list.IndexOf(value);
            }
            
            return index >= 0;
        }

        /// <summary>
        /// Returns the first index of the item if it exists in the list and -1 if not
        /// </summary>
        public static int IndexOf<T>(this IReadOnlyList<T> list, T value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (value != null)
                {
                    if (value.Equals(list[i]))
                    {
                        return i;
                    }
                }
                else if (list[i] == null)
                {
                    // We're looking for null
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns a string representation of a collection, with each item separated by the delimiter.
        /// </summary>
        /// <param name="name">Prepends the result with `{name}: `</param>
        /// <example>
        /// <code>
        /// var files = files = Directory.GetFiles(...)
        /// Debug.Log(files.AsString(name: "Files"));
        /// </code>
        /// </example>
        public static string AsString<T>(this IList<T> list, string delimiter = ", ", string name = null) 
        {
            var prepend = String.IsNullOrEmpty(name) ? "" : $"{name}: ";
            if (list == null)
                return prepend + "[]";

            string[] items = new string[list.Count];
            for (int i = 0; i < items.Length; i++)
                items[i] = list[i].ToString();

            return $"{prepend}[{string.Join(delimiter, items)}]";
        }

        public static bool ContainsAll<T>(this IEnumerable<T> list, IEnumerable<T> other)
        {
            foreach (var o in other)
            {
                // Use Linq Contains(), which resolves to the more optimized ICollection<T>.Contains() internally
                if (!list.Contains(o))
                {
                    return false;
                }
            }   

            return true;
        }

        public static bool Contains(this IEnumerable<string> strings, string value, StringComparison stringComparison)
        {
            foreach (var s in strings)
            {
                if (s != null && s.Equals(value, stringComparison))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TryFind<T>(this List<T> list, Predicate<T> predicate, out T value)
        {
            return (value = list.Find(predicate)) != null;
        }

        public static T[] ShallowClone<T>(this T[] array)
        {
            if (array == null)
            {
                return null;
            }
            
            T[] result = new T[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = array[i];
            }

            return result;
        }

        /// <summary>
        /// Replaces the content of the specified list.
        /// </summary>
        public static void ReplaceWith<T>(this List<T> list, IEnumerable<T> newContents)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (newContents == null) throw new ArgumentNullException(nameof(newContents));

            list.Clear();
            list.AddRange(newContents);
        }

        /// <summary>
        /// Returns the last element in a list, or throws an error if the list is empty.
        /// </summary>
        public static T Last<T>(this List<T> list)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            return list[list.Count - 1];
        }
        
        /// <summary>
        /// Insert an element to an already sorted list at its sorted position, without having to sort the whole list.
        /// </summary>
        /// <remarks>
        /// This method uses <see cref="List{T}.BinarySearch(T)"/> and relies on the fact that the list was already sorted.
        /// If the list is unsorted to begin with, the insert position is undefined.
        /// </remarks>
        public static void AddSorted<T>(this List<T> sortedList, T item, Comparer<T> comparer = null)
        {
            int index = sortedList.BinarySearch(item, comparer);
            
            if (index < 0) 
            {
                // BinarySearch returns a complement of the index if the item isn't found
                // https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.binarysearch?view=net-10.0#returns
                index = ~index; 
            }
            
            sortedList.Insert(index, item);
        }
    }
}