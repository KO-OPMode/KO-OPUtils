using System;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace KOOP
{
    /// <summary>
    /// Helper class to open files in either Windows or macOS.
    /// In the editor, we can use EditorUtility.OpenWithDefaultApp(), but that doesn't work in builds
    /// </summary>
    /// <remarks>
    /// Initially from http://wiki.unity3d.com/index.php/OpenInFileBrowser (wiki now defunct, brought into project by Jon)
    /// </remarks>
    public static class OpenInExternalApplication
    {
        public static bool CanOpenFileBrowserOnCurrentPlatform
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsPlayer:
                    case RuntimePlatform.WindowsEditor:
                        return true;
                    case RuntimePlatform.OSXPlayer:
                    case RuntimePlatform.OSXEditor:
                        return true;
                    default:
                        return false;
                }
            }
        }
        
        /// <summary>
        /// Opens the file path in the OS file browser, and returns <c>true</c> if the path could be found.
        /// </summary>
        public static bool TryOpenInFileBrowser(string path)
        {
            static void OpenInWindowsExplorer(string path)
            {
                // if path requested is a folder, automatically open insides of that folder
                bool openInsidesOfFolder = Directory.Exists(path);

                try
                {
                    System.Diagnostics.Process.Start("explorer.exe", (openInsidesOfFolder ? "/root," : "/select,") + path);
                }
                catch (System.ComponentModel.Win32Exception e)
                {
                    Debug.LogException(e);
                }
            }
            
            static void OpenInMacFinder(string path)
            {
                // if path requested is a folder, automatically open insides of that folder
                bool openInsidesOfFolder = Directory.Exists(path);

                if (!path.StartsWith("\""))
                {
                    path = "\"" + path;
                }

                if (!path.EndsWith("\""))
                {
                    path += "\"";
                }

                string arguments = (openInsidesOfFolder ? "" : "-R ") + path;

                try
                {
                    System.Diagnostics.Process.Start("open", arguments);
                }
                catch (System.ComponentModel.Win32Exception e)
                {
                    Debug.LogException(e);
                }
            }
            
            if (!CanOpenFileBrowserOnCurrentPlatform)
            {
                Debug.LogError($"Can't open file browser on platform {Application.platform} when trying to open path {path}");
                return false;
            }

            // Perform any platform-specific path cleanup
            path = CleanupPath(path);
            
            if (!Directory.Exists(path) && !File.Exists(path))
            {
                Debug.Log($"Can't open file at \"{path}\" because it does not exist!");
                return false;
            }
            
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    OpenInWindowsExplorer(path);
                    return true;
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    OpenInMacFinder(path);
                    return true;
                default:
                    Debug.LogError($"Can't open file browser on platform {Application.platform} when trying to open path {path}");
                    return false;
            }
        }

        /// <summary>
        /// Open a specified file and/or folder in Visual Studio Code if it is installed on this system.
        /// </summary>
        /// <param name="file">The file to open.</param>
        /// <param name="folder">The folder to open in VS Code. Can be used along with file to load a file within the context of a folder.</param>
        /// <param name="line">The line to select upon opening the file. Must specify file to function.</param>
        /// <param name="column">The column to select upon opening the file. Must specify file and line to function.</param>
        /// <returns></returns>
        public static bool TryOpenInVisualStudioCode(
            string file = null, 
            string folder = null, 
            int line = -1,
            int column = -1)
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN

            if (string.IsNullOrEmpty(file) && string.IsNullOrEmpty(folder))
            {
                Debug.LogError($"{nameof(TryOpenInVisualStudioCode)}: Must specify either a file or folder to open in VS Code.");
                return false;
            }

            if (!string.IsNullOrEmpty(file))
            {
                file = CleanupPath(file);

                if (column >= 0 && line < 0)
                {
                    Debug.LogWarning($"{nameof(TryOpenInVisualStudioCode)}: Should not specify character argument without a line argument.");
                }
            }
            else
            {
                if (line >= 0 || column >= 0)
                {
                    Debug.LogWarning($"{nameof(TryOpenInVisualStudioCode)}: Should not specify line and character arguments without a file to open.");
                }
            }

            if (!string.IsNullOrEmpty(folder))
            {
                folder = CleanupPath(folder);
            }
            
            try
            {
                string arguments = string.Empty;
                if (!string.IsNullOrEmpty(folder))
                {
                    arguments += $"\"{folder}\" ";
                }
                
                if (!string.IsNullOrEmpty(file))
                {
                    arguments += $"--goto \"{file}";

                    if (line >= 0)
                    {
                        arguments += $":{line}";

                        if (column >= 0)
                        {
                            arguments += $":{column}";
                        }
                    }

                    arguments += "\"";
                }

#if UNITY_EDITOR_OSX
                string codePath = "/usr/local/bin/code";
#else
                string codePath = "code";
#endif
                var processStartInfo = new ProcessStartInfo()
                {
                    FileName = codePath,
                    Arguments = arguments,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                
                Process.Start(processStartInfo);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                Debug.LogException(e);
            }
            
            return true;
#else
            Debug.LogError($"{nameof(TryOpenInVisualStudioCode)}: Cannot open VS Code on platform: {Application.platform}");
            return false;
#endif
        }
        
        /// <summary>
        /// replace any prefixes present in the path, depending on system
        /// </summary>
        public static string CleanupPath(string path)
        {
            static string ReplacePrefixIfExists(string path, string prefix, string replacement)
            {
                if (path.StartsWith(prefix))
                {
                    // Replace the initial occurrence of the prefix
                    // Use straight concatenation instead of Path.Combine(),
                    // which will actually discard our replacement since the remainder of the path
                    // may appear to be an absolute path (e.g. "/Folder") 
                    path = replacement + path[prefix.Length..];
                }

                return path;
            }
            
            static string CleanupPathMac(string path)
            {
                path = ReplacePrefixIfExists(path, "~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                return path;
            }

            static string CleanupPathWin(string path)
            {
                path = ReplacePrefixIfExists(path, "%APPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                path = ReplacePrefixIfExists(path, "%LOCALAPPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                path = ReplacePrefixIfExists(path, "%USERPROFILE%", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                path = path.Replace("/", "\\"); // windows explorer doesn't like forward slashes
                return path;
            }

            if (path == null)
            {
                return null;
            }
            
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    path = CleanupPathMac(path);
                    break;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    path = CleanupPathWin(path);
                    break;
            }

            return path;
        }
    }
}