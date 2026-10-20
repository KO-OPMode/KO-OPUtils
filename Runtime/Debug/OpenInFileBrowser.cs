using System;
using System.IO;
using System.Security.Permissions;
using UnityEngine;

namespace KOOP.Utils
{
    /// <summary>
    /// Helper class to open files in either Windows or macOS.
    /// In the editor, we can use EditorUtility.OpenWithDefaultApp(), but that doesn't work in builds
    /// </summary>
    /// <remarks>
    /// Initially from http://wiki.unity3d.com/index.php/OpenInFileBrowser (wiki now defunct, brought into project by Jon)
    /// </remarks>
    public static class OpenInFileBrowser
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
        public static bool TryOpenPath(string path)
        {
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
                    OpenInWin(path);
                    return true;
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    OpenInMac(path);
                    return true;
                default:
                    Debug.LogError($"Can't open file browser on platform {Application.platform} when trying to open path {path}");
                    return false;
            }
        }
        
        /// <summary>
        /// replace any prefixes present in the path, depending on system
        /// </summary>
        public static string CleanupPath(string path)
        {
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

        private static string CleanupPathMac(string path)
        {
            path = ReplacePrefixIfExists(path, "~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            return path;
        }

        private static string CleanupPathWin(string path)
        {
            path = ReplacePrefixIfExists(path, "%APPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            path = ReplacePrefixIfExists(path, "%LOCALAPPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            path = ReplacePrefixIfExists(path, "%USERPROFILE%", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            path = path.Replace("/", "\\"); // windows explorer doesn't like forward slashes
            return path;
        }
        
        private static void OpenInMac(string path)
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
                // tried to open mac finder in windows
                // just silently skip error
                // we currently have no platform define for the current OS we are in, so we resort to this
                e.HelpLink = ""; // do anything with this variable to silence warning about not using it
            }
        }

        private static void OpenInWin(string path)
        {
            // if path requested is a folder, automatically open insides of that folder
            bool openInsidesOfFolder = Directory.Exists(path);

            try
            {
                System.Diagnostics.Process.Start("explorer.exe", (openInsidesOfFolder ? "/root," : "/select,") + path);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                // tried to open win explorer in mac
                // just silently skip error
                // we currently have no platform define for the current OS we are in, so we resort to this
                e.HelpLink = ""; // do anything with this variable to silence warning about not using it
            }
        }

        private static string ReplacePrefixIfExists(string path, string prefix, string replacement)
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
    }
}