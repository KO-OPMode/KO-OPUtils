using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using KOOP.Extensions;
using UnityCollections;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace KOOPEditor
{
    /// <summary>
    /// An editor dialog to batch-rename the selected objects using search & replace.
    /// </summary>
    /// <owner>Lazlo</owner>
    public sealed class BatchRenamer : EditorWindow
    {
        private static BatchRenamer Instance { get; set; }

        private static readonly HashSet<Type> SupportedWindowTypes = new()
        {
            typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser"),
            typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow"),
            typeof(SceneView)
        };

        [Shortcut("KO_OP/Batch Rename", KeyCode.F2)] // Normal F2 takes priority with single selections anyway
        [MenuItem("Edit/Batch Rename Selection...")]
        private static void Shortcut()
        {
            TryOpen();
        }

        public static bool TryOpen()
        {
            if (!SupportedWindowTypes.Contains(focusedWindow.GetType()))
            {
                return false;
            }
            
            if (!GetSelectedRenamableObjects().Any())
            {
                return false;
            }

            Instance = GetWindow<BatchRenamer>();
            Instance.ShowUtility();
            Instance.Initialize();
            Instance.Focus();
            return true;
        }

        private static IEnumerable<UnityObject> GetSelectedRenamableObjects()
        {
            return Selection.GetFiltered<UnityObject>(SelectionMode.Unfiltered);
        }

        private enum Mode
        {
            ObjectNames = 0,
            AnimationPaths = 1
        }

        private readonly List<UnityObject> targets = new();
        private static Mode mode;
        private static string needle;
        private static string replacement;
        private static string prefix;
        private static string suffix;
        private static bool matchCase;
        private static bool numbersToLetters;
        private bool hasFocusedNeedleField;
        private Vector2 previewScroll;

        private void OnEnable()
        {
            titleContent.text = "Batch Renamer";
            minSize = new Vector2(400, 300);
        }

        public void Initialize()
        {
            targets.ReplaceWith(GetSelectedRenamableObjects());
            hasFocusedNeedleField = false;
            previewScroll = Vector2.zero;
        }

        private void OnGUI()
        {
            // Name fields
            const string ModeControlName = "Mode";
            const string NeedleControlName = "Needle";
            const string ReplacementControlName = "Replacement";
            const string PrefixControlName = "Prefix";
            const string SuffixControlName = "Suffix";
            const string MatchCaseControlName = "MatchCase";
            const string NumbersToLettersControlName = "NumbersToLetters";
            const string ReplaceButtonControlName = "Replace";

            var e = Event.current;

            // Catch enter keypress as confirmed
            var confirmed = false;

            if (e.type == EventType.KeyDown && e.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
            {
                var focusedControlName = GUI.GetNameOfFocusedControl();

                if (focusedControlName is
                    NeedleControlName or ReplacementControlName or
                    PrefixControlName or SuffixControlName or
                    MatchCaseControlName or
                    NumbersToLettersControlName or
                    ReplaceButtonControlName)
                {
                    confirmed = true;
                    e.Use();
                }
            }

            // Close on escape keypress
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                Close();
                e.Use();
                return;
            }

            // GUI

            EditorGUILayout.BeginVertical();
            {
                // Options

                var previousLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 120;

                GUI.SetNextControlName(ModeControlName);
                mode = (Mode)EditorGUILayout.EnumPopup("Mode", mode);

                GUI.SetNextControlName(NeedleControlName);
                needle = EditorGUILayout.TextField("Search", needle);

                GUI.SetNextControlName(ReplacementControlName);
                replacement = EditorGUILayout.TextField("Replace", replacement);

                GUI.SetNextControlName(PrefixControlName);
                prefix = EditorGUILayout.TextField("Prefix", prefix);

                GUI.SetNextControlName(SuffixControlName);
                suffix = EditorGUILayout.TextField("Suffix", suffix);

                GUI.SetNextControlName(MatchCaseControlName);
                matchCase = EditorGUILayout.Toggle("Match Case", matchCase);

                GUI.SetNextControlName(NumbersToLettersControlName);
                numbersToLetters = EditorGUILayout.Toggle("Numbers to Letters", numbersToLetters);

                EditorGUIUtility.labelWidth = previousLabelWidth;

                // Preview

                using var haystacks = GetHaystacks().OrderBy(h => h).ToPooledList();
                var effectiveRenameCount = 0;

                GUILayout.Label("Preview", "HeaderLabel");

                var windowWidth = position.width;

                // TODO: This should really be a tree-view, but I'm really in a hurry
                previewScroll = EditorGUILayout.BeginScrollView(previewScroll,
                                                                false, false,
                                                                GUIStyle.none, GUI.skin.verticalScrollbar,
                                                                Styles.previewBox,
                                                                GUILayout.ExpandHeight(true));
                {
                    var innerWidth = windowWidth - 28;

                    GUILayout.BeginVertical(GUILayout.Width(innerWidth));
                    {
                        for (var index = 0; index < haystacks.Count; index++)
                        {
                            var haystack = haystacks[index];
                            var odd = index % 2 == 1;

                            var isRename = TryRename(haystack, out var newName);
                            var foregroundColor = isRename ? Color.green : Color.white;

                            GUILayout.BeginHorizontal(odd ? Styles.previewItemOdd : Styles.previewItemEven);
                            GUILayout.Label(haystack, GUILayout.Width(innerWidth / 2f));
                            GUI.contentColor = foregroundColor;
                            GUILayout.Label(newName, GUILayout.Width(innerWidth / 2f));
                            GUI.contentColor = Color.white;
                            GUILayout.EndHorizontal();

                            if (isRename)
                            {
                                effectiveRenameCount++;
                            }
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndScrollView();

                // Button

                GUILayout.Space(6);

                EditorGUI.BeginDisabledGroup(effectiveRenameCount == 0);
                {
                    GUI.SetNextControlName(ReplaceButtonControlName);

                    if (GUILayout.Button($"Rename ({effectiveRenameCount})", GUILayout.Height(25)) || confirmed)
                    {
                        if (Rename())
                        {
                            Close();
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(3);
            }
            EditorGUILayout.EndVertical();

            // Focus needle field upon opening the window
            if (!hasFocusedNeedleField)
            {
                GUI.FocusControl(NeedleControlName);
                hasFocusedNeedleField = true;
            }
        }

        private static class Styles
        {
            public static readonly GUIStyle previewBox;
            public static readonly GUIStyle previewItemEven;
            public static readonly GUIStyle previewItemOdd;

            static Styles()
            {
                previewBox = new GUIStyle("FrameBox");
                previewBox.padding = new RectOffset(2, 2, 2, 2);

                previewItemEven = new GUIStyle("ObjectPickerResultsEven");
                previewItemEven.margin = new RectOffset(0, 0, 0, 0);

                previewItemOdd = new GUIStyle("ObjectPickerResultsOdd");
                previewItemOdd.margin = new RectOffset(0, 0, 0, 0);
            }
        }

        private IEnumerable<string> GetHaystacks()
        {
            if (mode == Mode.ObjectNames)
            {
                return targets.Where(t => t != null).Select(t => t.name);
            }
            else if (mode == Mode.AnimationPaths)
            {
                return targets.OfType<AnimationClip>()
                    .Where(clip => clip != null)
                    .SelectMany(clip => AnimationUtility.GetCurveBindings(clip).Concat(AnimationUtility.GetObjectReferenceCurveBindings(clip)))
                    .Select(binding => binding.path);
            }
            else
            {
                throw new ArgumentException(nameof(mode));
            }
        }


        private bool TryRename(string oldName, out string newName)
        {
            newName = oldName;

            if (!string.IsNullOrEmpty(needle))
            {
                var stringComparison = matchCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
                newName = oldName.Replace(needle, replacement, stringComparison);
            }

            if (numbersToLetters)
            {
                newName = Regex.Replace(newName, @"(\d+)", match =>
                {
                    if (int.TryParse(match.Value, out var number) && number is > 0 and < 26)
                    {
                        return ((char)('A' + (number - 1))).ToString();
                    }
                    else
                    {
                        return match.Value;
                    }
                });
            }

            newName = prefix + newName + suffix;
            newName = newName.Trim();

            return newName != oldName;
        }

        private bool Rename()
        {
            var isUndoable = mode == Mode.ObjectNames && !targets.Any(AssetDatabase.Contains);

            if (!isUndoable)
            {
                if (!EditorUtility.DisplayDialog($"Batch Rename",
                                                 "Are you sure you want to proceed with batch renaming?\n\nTHIS IS NOT UNDOABLE.",
                                                 "Rename",
                                                 "Cancel"))
                {
                    return false;
                }
            }
            
            foreach (var target in targets)
            {
                if (target == null)
                {
                    continue;
                }

                if (mode == Mode.ObjectNames)
                {
                    if (AssetDatabase.Contains(target))
                    {
                        var path = AssetDatabase.GetAssetPath(target);
                        var fileName = Path.GetFileNameWithoutExtension(path);

                        if (!TryRename(fileName, out var newFileName))
                        {
                            continue;
                        }

                        var renameError = AssetDatabase.RenameAsset(path, newFileName);

                        if (!string.IsNullOrEmpty(renameError))
                        {
                            Debug.LogError($"Failed to rename '{target}' to '{newFileName}': {renameError}", target);
                        }
                    }
                    else
                    {
                        if (!TryRename(target.name, out var newName))
                        {
                            continue;
                        }

                        Undo.RecordObject(target, $"Batch Rename");

                        target.name = newName;

                        if (PrefabUtility.IsPartOfPrefabInstance(target))
                        {
                            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                        }
                    }
                }
                else if (mode == Mode.AnimationPaths)
                {
                    if (target is not AnimationClip clip)
                    {
                        continue;
                    }
                    
                    var hasEdited = false;
                    
                    foreach (var floatBinding in AnimationUtility.GetCurveBindings(clip))
                    {
                        if (!TryRename(floatBinding.path, out var newPath))
                        {
                            continue;
                        }

                        var curve = AnimationUtility.GetEditorCurve(clip, floatBinding);
                        AnimationUtility.SetEditorCurve(clip, floatBinding, null);
                        var newBinding = floatBinding;
                        newBinding.path = newPath;
                        AnimationUtility.SetEditorCurve(clip, newBinding, curve);
                        
                        hasEdited = true;
                    }

                    foreach (var objectBinding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                    {
                        if (!TryRename(objectBinding.path, out var newPath))
                        {
                            continue;
                        }

                        var curve = AnimationUtility.GetObjectReferenceCurve(clip, objectBinding);
                        AnimationUtility.SetObjectReferenceCurve(clip, objectBinding, null);
                        var newBinding = objectBinding;
                        newBinding.path = newPath;
                        AnimationUtility.SetObjectReferenceCurve(clip, newBinding, curve);
                        
                        hasEdited = true;
                    }

                    if (hasEdited)
                    {
                        EditorUtility.SetDirty(clip);
                        AssetDatabase.SaveAssetIfDirty(clip);
                    }
                }
            }

            return true;
        }
    }
}