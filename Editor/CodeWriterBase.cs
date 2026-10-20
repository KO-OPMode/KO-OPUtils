using System;
using System.IO;
using System.Text;
using KOOP;
using UnityCollections;
using UnityEditor;

namespace KOOPEditor
{
    /// <summary>
    /// Class for generating code automatically in response to other data changing in the project.
    /// </summary>
    public abstract class CodeWriterBase
    {
        public abstract string GeneratedClassName { get; }
        public abstract string GeneratedFileDirectory { get; }
        public virtual string GeneratedFileName => GeneratedClassName;
        
        public string GeneratedFilePath => Path.Combine(GeneratedFileDirectory, $"{GeneratedFileName}.cs");
        
        private string LastWriteHashKey => $"{GetType().Name}_LastWriteHash";

        private int LastWriteHash
        {
            get => EditorPrefs.GetInt(LastWriteHashKey, 0);
            set => EditorPrefs.SetInt(LastWriteHashKey, value);
        }
        

        public void Write(bool force = false)
        {
            using var codeBuilder = new PooledStringBuilder();
            WriteFileContents(codeBuilder.BackingBuilder);
            
            var contents = codeBuilder.ToString();
            contents = contents.Replace(Environment.NewLine, "\n");
            
            int contentsHash = contents.GetStableHashCode();
            string absolutePath = AssetUtils.AssetPathToAbsolute(GeneratedFilePath);

            if (!force && File.Exists(absolutePath))
            {
                if (contentsHash == LastWriteHash)
                {
                    // We've written this exact file locally before
                    return;
                }

                if (string.Equals(File.ReadAllText(absolutePath), contents, StringComparison.Ordinal))
                {
                    // We've not written this file before locally, but it already exists on disk
                    // Mark this hash so we can skip this check faster in future
                    LastWriteHash = contentsHash;
                    return;
                }
            }
            
            File.WriteAllText(absolutePath, contents);
            LastWriteHash = contentsHash;
            AssetDatabase.Refresh();
        }

        protected abstract void WriteFileContents(StringBuilder codeBuilder);
    }
}