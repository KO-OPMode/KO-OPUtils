using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.Localization.Tables;

namespace KOOP.Extensions
{
    /// <summary>
    /// Helper methods for interacting with Unity's Localization package
    /// </summary>
    public static class LocalizationExtensions
    {
        public static bool IsNullOrEmpty(this LocalizedString localizedString)
        {
            return localizedString == null || localizedString.IsEmpty;
        }
        
        
        /// <summary>
        /// Checks whether a localized string exists in the localization tables.
        /// This method checks whether the string is *defined* -- it will return true even for keys with empty strings.
        /// </summary>
        public static bool HasEntryInLocalizationTables(this LocalizedString localizedString)
        {
            if (localizedString.IsNullOrEmpty())
            {
                return false;
            }

            var table = LocalizationSettings.StringDatabase.GetTable(localizedString.TableReference);
            
            if (table == null)
            {
                return false;
            }

            var sharedData = table.SharedData;
            
            if (sharedData == null)
            {
                return false;
            }

            var tableEntryReference = localizedString.TableEntryReference;
            
            if (tableEntryReference.ReferenceType == TableEntryReference.Type.Id)
            {
                return sharedData.Contains(tableEntryReference.KeyId);
            }
            else
            {
                return sharedData.Contains(tableEntryReference.Key);
            }
        }

        /// <summary>
        /// Returns a new <see cref="LocalizedString"/> referencing the same table and entry as the original
        /// Does not copy any variables, arguments, or other state from the source string
        /// </summary>
        public static LocalizedString ShallowClone(this LocalizedString localizedString)
        {
            if (localizedString == null)
            {
                return null;
            }
            
            if (localizedString.IsEmpty)
            {
                return new LocalizedString();
            }

            return new LocalizedString(localizedString.TableReference, localizedString.TableEntryReference);
        }

        /// <summary>
        /// Returns the localized string if it exists. Otherwise, returns a hardcoded fallback to help debug the issue.
        /// </summary>
        /// <param name="localizedString">The localized string to return</param>
        /// <param name="fallback">The hardcoded string to return is the localized string is not defined</param>
        /// <param name="debugFallbackSuffix">A suffix that gets appended to the fallback in debug builds and editor to make it clearer that we are missing localization.</param>
        public static string GetLocalizedStringOrFallback(this LocalizedString localizedString, string fallback, string debugFallbackSuffix = "<MISSING LOC>")
        {
            if (!localizedString.IsNullOrEmpty())
            {
                return localizedString.GetLocalizedString();
            }
            
#if DEBUG
            if (!string.IsNullOrEmpty(debugFallbackSuffix))
            {
                return $"{fallback} {debugFallbackSuffix}";
            }
#endif
            
            return fallback;
        }
        
        /// <summary>
        /// Sets a persistent <a href="https://docs.unity3d.com/Packages/com.unity.localization@1.3/manual/Smart/Persistent-Variables-Source.html">local variable</a> on the LocalizedString
        /// Note that these values are serialized onto the LocalizedString, unlike values set with localizedString.Arguments
        /// Will re-use the existing wrapper variable if one already exists of the correct type
        /// </summary>
        public static void SetLocalVariable(this LocalizedString localizedString, string key, int value)
        {
            if (localizedString.TryGetValue(key, out IVariable variable) && variable is IntVariable typedVariable)
            {
                typedVariable.Value = value;
            }
            else
            {
                localizedString[key] = new IntVariable { Value = value };
                localizedString.RefreshString();
            }
        }
        
        /// <inheritdoc cref="SetLocalVariable(UnityEngine.Localization.LocalizedString,string,int)"/>
        public static void SetLocalVariable(this LocalizedString localizedString, string key, float value)
        {
            if (localizedString.TryGetValue(key, out IVariable variable) && variable is FloatVariable typedVariable)
            {
                typedVariable.Value = value;
            }
            else
            {
                localizedString[key] = new FloatVariable { Value = value };
                localizedString.RefreshString();
            }
        }
        
        /// <inheritdoc cref="SetLocalVariable(UnityEngine.Localization.LocalizedString,string,int)"/>
        public static void SetLocalVariable(this LocalizedString localizedString, string key, bool value)
        {
            if (localizedString.TryGetValue(key, out IVariable variable) && variable is BoolVariable typedVariable)
            {
                typedVariable.Value = value;
            }
            else
            {
                localizedString[key] = new BoolVariable { Value = value };
                localizedString.RefreshString();
            }
        }
        
        /// <inheritdoc cref="SetLocalVariable(UnityEngine.Localization.LocalizedString,string,int)"/>
        public static void SetLocalVariable(this LocalizedString localizedString, string key, string value)
        {
            if (localizedString.TryGetValue(key, out IVariable variable) && variable is StringVariable typedVariable)
            {
                typedVariable.Value = value;
            }
            else
            {
                localizedString[key] = new StringVariable { Value = value };
                localizedString.RefreshString();
            }
        }
        
        /// <inheritdoc cref="SetLocalVariable(UnityEngine.Localization.LocalizedString,string,int)"/>
        public static void SetLocalVariable(this LocalizedString localizedString, string key, LocalizedString value)
        {
            // This overload doesn't require any wrapping, but we're including it to provide a consistent API
            localizedString[key] = value;
            localizedString.RefreshString();
        }
        
        /// <inheritdoc cref="SetLocalVariable(UnityEngine.Localization.LocalizedString,string,int)"/>
        public static void SetLocalVariable(this LocalizedString localizedString, string key, IVariable variable)
        {
            // This overload doesn't require any wrapping, but we're including it to provide a consistent API
            localizedString[key] = variable;
            localizedString.RefreshString();
        }
        
        /// <inheritdoc cref="SetLocalVariable(UnityEngine.Localization.LocalizedString,string,int)"/>
        public static void SetLocalVariable(this LocalizedString localizedString, string key, object value)
        {
            if (value == null)
            {
                return;
            }
            
            if (value is int intValue)
            {
                localizedString.SetLocalVariable(key, intValue);
            }
            else if (value is float floatValue)
            {
                localizedString.SetLocalVariable(key, floatValue);
            }
            else if (value is bool boolValue)
            {
                localizedString.SetLocalVariable(key, boolValue);
            }
            else if (value is LocalizedString localizedStringValue)
            {
                localizedString.SetLocalVariable(key, localizedStringValue);
            }
            else if (value is IVariable variable)
            {
                localizedString.SetLocalVariable(key, variable);
            }
            else
            {
                // Set everything else as strings
                localizedString.SetLocalVariable(key, value.ToString());
            }
        }

        /// <summary>
        /// Sets <see cref="LocalizeStringEvent.StringReference"/> on the given string event.
        /// If the assigned string is null or empty, propagates an empty string rather than the default behaviour
        /// of preserving the last string localized.
        /// </summary>
        /// TODO: We should fork the localization package and just make this part of the StringReference setter officially
        public static void SafeSetStringReference(this LocalizeStringEvent stringEvent, LocalizedString localizedString)
        {
            if (stringEvent.StringReference == localizedString)
            {
                return;
            }
            
            stringEvent.StringReference = localizedString;
            if (localizedString.IsNullOrEmpty())
            {
                stringEvent.OnUpdateString?.Invoke(string.Empty);
            }
        }
    }
}