using UnityEditor;
using UnityEditor.SettingsManagement;

namespace KOOPEditor
{
    /// <summary>
    /// Customized version of UserSetting<T> that automatically forwards to our settings manager
    /// </summary>
    /// <owner>jonagill</owner>
    public class EditorSetting<T> : UserSetting<T>
    {
        public EditorSetting(string key, T value, SettingsScope scope = SettingsScope.User)
            : base(EditorSettingsProvider.Settings, key, value, scope)
        {}

        EditorSetting(Settings settings, string key, T value, SettingsScope scope = SettingsScope.User)
            : base(settings, key, value, scope) { }
    }
}
