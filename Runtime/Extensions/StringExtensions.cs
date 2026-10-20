using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KOOP.Extensions
{
    public static class StringExtensions
    {
        private static readonly StringBuilder StringBuilder = new();
        private static readonly char[] NewLineCharacters = { '\n', '\r' };

        public static string RemoveAllWhiteSpace(this string s)
        {
            StringBuilder.Clear();

            int startIndex = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsWhiteSpace(s[i]))
                {
                    if (startIndex != i)
                        StringBuilder.Append(s[startIndex..i]);
                    startIndex = i + 1;
                }
            }

            if (startIndex != s.Length)
                StringBuilder.Append(s[startIndex..]);
            return StringBuilder.ToString();
        }

        /// <summary>
        /// Removes all leading and trailing newlines (both <c>\n</c> and <c>\r</c>) from the string. 
        /// </summary>
        public static string TrimNewLines(this string s)
        {
            return s.Trim(NewLineCharacters);
        }

        public static string[] SplitByLine(this string input, StringSplitOptions options = StringSplitOptions.None)
        {
            return input.Split(new[] { "\r\n", "\r", "\n" }, options);
        }

        /// <summary>
        /// A version of string.Contains() that also returns the detected index
        /// </summary>
        public static bool Contains(this string input, string value, out int index)
        {
            index = input.IndexOf(value, StringComparison.Ordinal);
            return index >= 0;
        }

        /// <summary>
        /// Adds color rich text tags to the string.
        /// </summary>
        /// <param name="text">The string to be wrapped in color tags.</param>
        /// <param name="color">The color to apply to the text.</param>
        public static string Color(this string text, Color color)
        {
            return $"<color={color.HexCodeRGBA()}>{text}</color>";
        }
        
        /// <summary>
        /// Reimplementation of <see cref="UnityEditor.ObjectNames.NicifyVariableName"/> that can be used at runtime.
        /// </summary>
        /// <remarks>Written by user ErnestSurys in the Unity forums here: https://discussions.unity.com/t/nicefy-variable-names-at-runtime/879079/3</remarks>
        public static string NicifyVariableName(this string input)
        {
            StringBuilder result = new(input.Length * 2);
            bool prevIsLetter = false;
            bool prevIsLetterUpper = false;
            bool prevIsDigit = false;
            bool prevIsStartOfWord = false;
            bool prevIsNumberWord = false;

            int firstCharIndex = 0;
            if (input.StartsWith('_'))
                firstCharIndex = 1;
            else if (input.StartsWith("m_"))
                firstCharIndex = 2;

            for (int i = input.Length - 1; i >= firstCharIndex; i--)
            {
                char currentChar = input[i];
                bool currIsLetter = char.IsLetter(currentChar);
                if (i == firstCharIndex && currIsLetter)
                    currentChar = char.ToUpper(currentChar);
                bool currIsLetterUpper = char.IsUpper(currentChar);
                bool currIsDigit = char.IsDigit(currentChar);
                bool currIsSpacer = currentChar == ' ' || currentChar == '_';

                bool addSpace = (currIsLetter && !currIsLetterUpper && prevIsLetterUpper) ||
                    (currIsLetter && prevIsLetterUpper && prevIsStartOfWord) ||
                    (currIsDigit && prevIsStartOfWord) ||
                    (!currIsDigit && prevIsNumberWord) ||
                    (currIsLetter && !currIsLetterUpper && prevIsDigit);

                if (!currIsSpacer && addSpace)
                {
                    result.Insert(0, ' ');
                }

                result.Insert(0, currentChar);
                prevIsStartOfWord = currIsLetter && currIsLetterUpper && prevIsLetter && !prevIsLetterUpper;
                prevIsNumberWord = currIsDigit && prevIsLetter && !prevIsLetterUpper;
                prevIsLetterUpper = currIsLetter && currIsLetterUpper;
                prevIsLetter = currIsLetter;
                prevIsDigit = currIsDigit;
            }

            return result.ToString();
        }

        /// <summary> 
        /// Version of <see cref="String.TrimStart(char)"/> that accepts a string trimmable.
        /// </summary>
        public static string TrimStart(this string source, string value)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            if (!source.StartsWith(value))
            {
                return source;
            }

            return source.Substring(value.Length);
        }
        
        /// <summary> 
        /// Version of <see cref="String.TrimEnd(char)"/> that accepts a string trimmable.
        /// </summary>
        public static string TrimEnd(this string source, string value)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            if (!source.EndsWith(value))
            {
                return source;
            }

            return source.Remove(source.LastIndexOf(value));
        }

        /// <summary>
        /// Returns the part of a string before the first instance a given character,
        /// or the full string if it is not found.
        /// </summary>
        public static string PartBefore(this string s, char c)
        {
            var index = s.IndexOf(c);

            if (index >= 0)
            {
                return s.Substring(0, index);
            }
            else
            {
                return s;
            }
        }

        /// <summary>
        /// Returns the part of a string before the first instance a given string,
        /// or the full string if it is not found.
        /// </summary>
        public static string PartBefore(this string s, string t, StringComparison comparisonType = StringComparison.InvariantCulture)
        {
            var index = s.IndexOf(t, comparisonType);

            if (index >= 0)
            {
                return s.Substring(0, index);
            }
            else
            {
                return s;
            }
        }

        /// <summary>
        /// Returns the part of a string before the last instance of a given character,
        /// or the full string if it is not found.
        /// </summary>
        public static string PartBeforeLast(this string s, char c)
        {
            var index = s.LastIndexOf(c);

            if (index >= 0)
            {
                return s.Substring(0, index);
            }
            else
            {
                return s;
            }
        }

        /// <summary>
        /// Returns the part of a string before the last instance of a given string,
        /// or the full string if it is not found.
        /// </summary>
        public static string PartBeforeLast(this string s, string t, StringComparison comparisonType = StringComparison.InvariantCulture)
        {
            var index = s.LastIndexOf(t, comparisonType);

            if (index >= 0)
            {
                return s.Substring(0, index);
            }
            else
            {
                return s;
            }
        }

        /// <summary>
        /// Returns the part of a string after the first instance of a given character,
        /// or the full string if it is not found.
        /// </summary>
        public static string PartAfter(this string s, char c)
        {
            var index = s.IndexOf(c);

            if (index >= 0)
            {
                return s.Substring(index + 1);
            }
            else
            {
                return s;
            }
        }

        /// <summary>
        /// Returns the part of a string after the first instance of a given string,
        /// or the full string if it is not found.
        /// </summary>
        public static string PartAfter(this string s, string t, StringComparison comparisonType = StringComparison.InvariantCulture)
        {
            var index = s.IndexOf(t, comparisonType);

            if (index >= 0)
            {
                return s.Substring(index + t.Length);
            }
            else
            {
                return s;
            }
        }

        /// <summary>
        /// Returns the part of a string after the last instance of a given character,
        /// or the full string if it is not found.
        /// </summary>
        public static string PartAfterLast(this string s, char c)
        {
            var index = s.LastIndexOf(c);

            if (index >= 0)
            {
                return s.Substring(index + 1);
            }
            else
            {
                return s;
            }
        }

        /// <summary>
        /// Returns the part of a string after the last instance of a given string,
        /// or the full string if it is not found.
        /// </summary>
        public static string PartAfterLast(this string s, string t, StringComparison comparisonType = StringComparison.InvariantCulture)
        {
            var index = s.LastIndexOf(t, comparisonType);

            if (index >= 0)
            {
                return s.Substring(index + t.Length);
            }
            else
            {
                return s;
            }
        }

        /// <inheritdoc cref="string.Join(string, IEnumerable{string})"/>
        public static string Join(this IEnumerable<string> source, string separator)
        {
            return string.Join(separator, source);
        }

        /// <inheritdoc cref="string.Join(string, IEnumerable{string})"/>
        public static string Join(this IEnumerable<string> source, char separator)
        {
            return string.Join(separator, source);
        }
        
        /// <summary>
        /// Splits a string by a separator, but ignore the separator if preceded by the specified escape character.
        /// </summary>
        /// <source>https://stackoverflow.com/a/78354656</source>
        public static IEnumerable<string> Split(this string text, char separator, char escapeCharacter, bool removeEmptyEntries)
        {
            var buffer = new StringBuilder();
            var escape = false;

            foreach (var c in text)
            {
                if (!escape && c == separator)
                {
                    if (!removeEmptyEntries || buffer.Length > 0)
                    {
                        yield return buffer.ToString();
                    }

                    buffer.Clear();
                }
                else
                {
                    if (c == escapeCharacter && !escape)
                    {
                        escape = true;
                    }
                    else
                    {
                        buffer.Append(c);
                        escape = false;
                    }
                }
            }

            if (buffer.Length != 0)
            {
                yield return buffer.ToString();
            }
        }
        
        public static bool ContainsInsensitive(this string source, string toCheck)
        {
            return source.Contains(toCheck, StringComparison.OrdinalIgnoreCase);
        }
    }
}