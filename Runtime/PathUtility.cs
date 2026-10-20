using System.Collections.Generic;
using System.IO;
using KOOP.Extensions;

namespace KOOP
{
    /// <summary>
    /// Various utilities for path manipulation. This is editor-agnostic.
    /// </summary>
    public static class PathUtility
    {
        #region Manipulation

        public static string ChangeFileName(string path, string newFileNameWithoutExtension)
        {
            return Path.Combine(Path.GetDirectoryName(path), newFileNameWithoutExtension + Path.GetExtension(path));
        }
        
        public static string ChangeFileName(string path, string newFileNameWithoutExtension, string newExtension)
        {
            if (!string.IsNullOrWhiteSpace(newExtension) && !newExtension.StartsWith('.'))
            {
                newExtension = '.' + newExtension;
            }
            
            return Path.Combine(Path.GetDirectoryName(path), newFileNameWithoutExtension + newExtension);
        }
        
        public static string ChangeFileNameWithExtension(string path, string newFileNameWithExtension)
        {
            return Path.Combine(Path.GetDirectoryName(path), newFileNameWithExtension);
        }

        public static string ChangeExtension(string path, string newExtension)
        {
            if (!string.IsNullOrWhiteSpace(newExtension) && !newExtension.StartsWith('.'))
            {
                newExtension = '.' + newExtension;
            }

            return Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + newExtension);
        }

        public static string AppendFileName(string path, string suffix)
        {
            return ChangeFileName(path, Path.GetFileNameWithoutExtension(path) + suffix);
        }

        public static string PrependFileName(string path, string prefix)
        {
            return ChangeFileName(path, prefix + Path.GetFileNameWithoutExtension(path));
        }

        public static string TrimFileNameEnd(string path, string value)
        {
            return ChangeFileName(path, Path.GetFileNameWithoutExtension(path).TrimEnd(value));
        }

        public static string TrimFileNameStart(string path, string value)
        {
            return ChangeFileName(path, Path.GetFileNameWithoutExtension(path).TrimStart(value));
        }

        public static string GetExtension(string path, bool includeDot)
        {
            var fileName = Path.GetFileName(path);
            return fileName.Contains('.') ? ((includeDot ? "." : string.Empty) + fileName.PartAfterLast('.')) : null;
        }

        #endregion


        #region Naive Paths

        // "Naive" path manipulation just disregards the type of directory separator and trailing separator,
        // but does not do a full actual path resolution (e.g. ../ or ./) nor any file system lookup.

        /// <summary>
        /// The separator character to use in naive paths.
        /// </summary>
        public const char NaiveSeparatorChar = '/';

        public static string NaiveNormalize(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            return path
                .Replace(Path.DirectorySeparatorChar, NaiveSeparatorChar)
                .Replace(Path.AltDirectorySeparatorChar, NaiveSeparatorChar)
                .TrimEnd(NaiveSeparatorChar);
        }

        public static bool NaiveEquals(string a, string b)
        {
            return a == b || NaiveNormalize(a)?.ToUpperInvariant() == NaiveNormalize(b)?.ToUpperInvariant();
        }

        public static string NaiveParent(string path)
        {
            path = NaiveNormalize(path);

            if (!path.Contains(NaiveSeparatorChar))
            {
                return string.Empty;
            }

            return path.PartBeforeLast(NaiveSeparatorChar);
        }

        public static string NaiveFileName(string path, bool includeExtension = true)
        {
            path = NaiveNormalize(path);

            if (!path.Contains(NaiveSeparatorChar))
            {
                return path;
            }

            var fileName = path.PartAfterLast(NaiveSeparatorChar);

            if (!includeExtension)
            {
                fileName = fileName.PartBeforeLast('.');
            }

            return fileName;
        }

        public static bool NaiveContains(string parentPath, string childPath, bool recursive)
        {
            parentPath = NaiveNormalize(parentPath);
            childPath = NaiveNormalize(childPath);
            var childParentPath = NaiveParent(childPath);

            if (parentPath == childParentPath)
            {
                return true;
            }

            if (recursive && childParentPath.StartsWith(parentPath + NaiveSeparatorChar))
            {
                return true;
            }

            return false;
        }

        public static string NaiveRelative(string parentPath, string fullPath)
        {
            if (!NaiveContains(parentPath, fullPath, recursive: true))
            {
                return fullPath;
            }

            parentPath = NaiveNormalize(parentPath);
            fullPath = NaiveNormalize(fullPath);
            return fullPath.TrimStart(parentPath + NaiveSeparatorChar);
        }

        public sealed class NaiveComparer : IEqualityComparer<string>
        {
            public static NaiveComparer Default { get; } = new();

            public bool Equals(string x, string y)
            {
                return NaiveEquals(x, y);
            }

            public int GetHashCode(string obj)
            {
                return NaiveNormalize(obj).GetHashCode();
            }
        }

        #endregion
    }
}