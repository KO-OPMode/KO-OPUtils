
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SettingsManagement;
using UnityEngine;

namespace KOOPEditor
{
    /// <summary>
    /// Provider of custom editor settings to inject into the Preferences menu
    /// </summary>
    /// <owner>jonagill</owner>
    [InitializeOnLoad]
    public static class EditorSettingsProvider
    {
        private static Settings _settings;
        public static Settings Settings
        {
            get
            {
                if (_settings == null)
                    _settings = new Settings(PackageSettingsNamespace);

                return _settings;
            }
        }

        private static readonly string UnityProjectName;
        private static readonly string UnityProjectNameAlphanumeric;
        private static readonly string ProjectDirectoryName;
        private static readonly string ProjectDirectoryNameAlphanumeric;

        private static readonly string PackageSettingsNamespace;
        
        private static string _productName;
        private static string _productNameAlphanumeric;
        
        private static readonly Regex NonAlphanumericsRegex = new Regex("[^a-zA-Z0-9]");

        static EditorSettingsProvider()
        {
            // Application.dataPath returns Assets, so step up one farther
            var unityProjectPath = Path.GetDirectoryName(Application.dataPath);
            UnityProjectName = Path.GetFileName(unityProjectPath);
            UnityProjectNameAlphanumeric = GetAlphanumericString(UnityProjectName);

            // Step one farther up, to get the folder our project is in
            // By KO_OP standards, this is still a project-specific folder
            var projectDirectoryPath = Path.GetDirectoryName(unityProjectPath);
            ProjectDirectoryName = Path.GetFileName(projectDirectoryPath);
            ProjectDirectoryNameAlphanumeric = GetAlphanumericString(ProjectDirectoryName);

            PackageSettingsNamespace = $"com.koop.{UnityProjectNameAlphanumeric}";
        }

        private static void CacheProductName()
        {
            if (!string.IsNullOrEmpty(_productName))
            {
                return;
            }
            
            // Don't do this in the static constructor, because that will often get invoked when deserializing
            // serialized objects, and we can't call PlayerSettings.productName during deserialization
            // Thanks, Unity
            
            _productName = PlayerSettings.productName;
            _productNameAlphanumeric = GetAlphanumericString(_productName);
        }
        
        
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            CacheProductName();
            
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assemblyCSharpAssemblies = GetAssembliesContainingString("assembly-csharp");
            var koopAssemblies = GetAssembliesContainingString("koop");
            var projectNameAssemblies = GetAssembliesContainingString(UnityProjectNameAlphanumeric);
            var projectDirectoryAssemblies = GetAssembliesContainingString(ProjectDirectoryNameAlphanumeric);
            var productNameAssemblies = GetAssembliesContainingString(_productNameAlphanumeric);

            var matchingAssemblies = assemblyCSharpAssemblies
                .Concat(koopAssemblies)
                .Concat(projectNameAssemblies)
                .Concat(projectDirectoryAssemblies)
                .Concat(productNameAssemblies)
                .Distinct()
                .ToArray();

            var preferencesPath = $"Preferences/{_productName}";
            
            return new UserSettingsProvider(
                preferencesPath,
                Settings,
                matchingAssemblies);

            IEnumerable<Assembly> GetAssembliesContainingString(string filter)
            {
                // If we exceed this amount, we're probably matching to something really generic
                // (e.g. "ys" matched to "System") and not getting good results
                const int maxMatchesPerFilter = 10;
                
                var matches = allAssemblies.Where(assembly => assembly.GetName().Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase));
                if (matches.Count() > maxMatchesPerFilter)
                {
                    return Array.Empty<Assembly>();
                }

                return matches.ToArray();
            }
        }

        private static string GetAlphanumericString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            
            return NonAlphanumericsRegex.Replace(value, string.Empty);
        }
    }
}
