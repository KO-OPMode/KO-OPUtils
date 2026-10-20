using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using KOOP;
using ReflectionMagic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KOOPEditor
{
    /// <owner>Lazlo</owner>
    /// <summary>
    /// The is basically a fork of <see cref="ModelImporterMaterialEditor" /> that does supports multi-object editing.
    /// Code is adapted from the managed source. Current Reference Version: 6000.0.13f1 (Last known commit on our Unity release as of 2025-09-15)
    /// https://github.com/Unity-Technologies/UnityCsReference/blob/6000.0/Modules/AssetPipelineEditor/ImportSettings/ModelImporterMaterialEditor.cs
    /// Places where logic was modified to accomodate for multi-targets are marked with "PATCH".
    /// </summary>
    /// <note>
    /// Because this code is copied from the source at a specific Unity version,
    /// it will not magically benefit from any updates Unity does to the material import workflow.
    /// These would have to get forked again.
    /// Unfortunately, I see no clever way of simply using reflection to call the same behaviour as the base window,
    /// because it tracks state data on top of GUI code.
    /// </note>
    /// <note>
    /// It's interesting to note that Unity seems to have *almost* properly implemented multi-object editing, but decided
    /// to seemingly stop short of finishing, short-circuit it and prevent it instead.
    /// Maybe something is broken even in their implementation, and they decided to keep it but fix it later?
    /// The main limitation I see right now is that re-assigning materials with different names is not compatible (but that's
    /// mostly fine).
    /// Nothing else obvious comes from reading their implementation, but this is a deep part of the pipeline, so I can't tell.
    /// TODO: Ask ISS for a reason why it's locked?
    /// </note>
    public sealed class MultiModelMaterialImportSettingsWindow : EditorWindow, IHasCustomMenu
    {
        #region Static

        private const string DebugModePrefsKey = nameof(KOOPEditor) + "." + nameof(MultiModelMaterialImportSettingsWindow) + "." + nameof(DebugMode);

        /// <summary>
        /// Shows colors on individual material fields depending on their serialization mode.
        /// </summary>
        private static bool DebugMode
        {
            get => EditorPrefs.GetBool(DebugModePrefsKey, defaultValue: false);
            set => EditorPrefs.SetBool(DebugModePrefsKey, value);
        }

        static MultiModelMaterialImportSettingsWindow()
        {
            ReflectUnityInternals();
        }

        [MenuItem("Tools/Bulk Editing/Art/Multi Model Material Import Settings")]
        private static void MenuItem()
        {
            GetWindow<MultiModelMaterialImportSettingsWindow>();
        }

        public static MultiModelMaterialImportSettingsWindow instance { get; private set; }

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Debug Mode"), DebugMode, () => DebugMode = !DebugMode);
        }

        #endregion



        #region Data

        private bool isInitialized;

        private string[] paths;

        private ModelImporter[] targets;

        private bool m_ShowAllMaterialNameOptions = true;

        private bool m_ShowMaterialRemapOptions;

        private SerializedObject serializedObject;

        private SerializedProperty m_MaterialName;

        private SerializedProperty m_MaterialSearch;

        private SerializedProperty m_MaterialLocation;

        private SerializedProperty m_Materials;

        private SerializedProperty m_ExternalObjects;

        private SerializedProperty m_HasEmbeddedTextures;

        private SerializedProperty m_SupportsEmbeddedMaterials;

        private SerializedProperty m_UseSRGBMaterialColor;

        private SerializedProperty m_MaterialImportMode;

        private List<MaterialCache> m_MaterialsCache = new();

        private bool m_CanExtractEmbeddedMaterials;

        private Dictionary<Tuple<string, string>, ExternalObjectCache> m_ExternalObjectsCache = new();

        private static readonly int s_PPtrHash = nameof(s_PPtrHash).GetHashCode();

        #endregion



        #region Events

        private void OnEnable()
        {
            instance = this;

            titleContent.text = "Multi Model Material Import Settings";

            Initialize();

            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;

            Dispose();

            if (instance == this)
            {
                instance = null;
            }
        }

        private void OnUndoRedoPerformed()
        {
            if (!isInitialized) return;
            
            ResetValues();
        }

        private void OnSelectionChange()
        {
            Initialize();
        }

        private void OnGUI()
        {
            if (!isInitialized)
            {
                return;
            }

            EditorGUILayout.HelpBox("Warning: This window is an experimental fork of the Unity source. Please report any issue to Lazlo!", MessageType.Warning);

            DoMaterialsGUI();

            if (serializedObject.ApplyModifiedProperties())
            {
                // Ideally, we'd to an Apply/Revert GUI like asset importer editors, but that is a *very* 
                // complex thing to reimplement and maintain and ultimately not worth the hassle for our needs.
                Apply();
            }
        }

        #endregion



        #region Commands

        private void Initialize()
        {
            Dispose();

            var _targets = new List<ModelImporter>();
            var _paths = new List<string>();

            foreach (var selectedAsset in Selection.GetFiltered<GameObject>(SelectionMode.Assets))
            {
                var path = AssetDatabase.GetAssetPath(selectedAsset);
                var importer = AssetImporter.GetAtPath(path);

                if (importer == null || importer is not ModelImporter modelImporter)
                {
                    continue;
                }

                _paths.Add(path);
                _targets.Add(modelImporter);
            }

            if (_targets.Count == 0)
            {
                Repaint();
                return;
            }

            paths = _paths.ToArray();
            targets = _targets.ToArray();
            serializedObject = new SerializedObject(targets);

            m_MaterialName = serializedObject.FindProperty(nameof(m_MaterialName));
            m_MaterialSearch = serializedObject.FindProperty(nameof(m_MaterialSearch));
            m_MaterialLocation = serializedObject.FindProperty(nameof(m_MaterialLocation));
            m_Materials = serializedObject.FindProperty(nameof(m_Materials));
            m_ExternalObjects = serializedObject.FindProperty(nameof(m_ExternalObjects));
            m_HasEmbeddedTextures = serializedObject.FindProperty(nameof(m_HasEmbeddedTextures));
            m_SupportsEmbeddedMaterials = serializedObject.FindProperty(nameof(m_SupportsEmbeddedMaterials));
            m_UseSRGBMaterialColor = serializedObject.FindProperty(nameof(m_UseSRGBMaterialColor));
            m_MaterialImportMode = serializedObject.FindProperty(nameof(m_MaterialImportMode));

            BuildMaterialsCache();
            BuildExternalObjectsCache();
            EvaluateMaterialExtractionState();

            isInitialized = true;

            Repaint();
        }

        private void Dispose()
        {
            Disposable.DisposeAndSetNull(ref m_MaterialName);
            Disposable.DisposeAndSetNull(ref m_MaterialSearch);
            Disposable.DisposeAndSetNull(ref m_MaterialLocation);
            Disposable.DisposeAndSetNull(ref m_Materials);
            Disposable.DisposeAndSetNull(ref m_ExternalObjects);
            Disposable.DisposeAndSetNull(ref m_HasEmbeddedTextures);
            Disposable.DisposeAndSetNull(ref m_SupportsEmbeddedMaterials);
            Disposable.DisposeAndSetNull(ref m_UseSRGBMaterialColor);
            Disposable.DisposeAndSetNull(ref m_MaterialImportMode);
            Disposable.DisposeAndSetNull(ref serializedObject);

            targets = null;
            paths = null;

            isInitialized = false;
        }

        private void Apply()
        {
            foreach (var path in paths)
            {
                AssetDatabase.WriteImportSettingsIfDirty(path);
            }

            AssetDatabase.StartAssetEditing();

            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    AssetDatabase.ImportAsset(path);
                }
            }

            AssetDatabase.StopAssetEditing();

            ResetValues();
        }

        private void ResetValues()
        {
            serializedObject.Update();
            BuildMaterialsCache();
            BuildExternalObjectsCache();
            EvaluateMaterialExtractionState();
        }

        private void UpdateShowAllMaterialNameOptions()
        {
            // We need to display BasedOnTextureName_Or_ModelNameAndMaterialName obsolete option for objects which use this option
            #pragma warning disable 618
            m_ShowAllMaterialNameOptions = (m_MaterialName.intValue == (int)ModelImporterMaterialName.BasedOnTextureName_Or_ModelNameAndMaterialName);
            #pragma warning restore 618
        }

        private void BuildMaterialsCache()
        {
            if (m_Materials.hasMultipleDifferentValues)
            {
                // PATCH: Remove this check
                // return;
            }

            m_MaterialsCache.Clear();

            for (var materialIdx = 0; materialIdx < m_Materials.arraySize; ++materialIdx)
            {
                var mat = new MaterialCache();
                var id = m_Materials.GetArrayElementAtIndex(materialIdx);
                mat.name = id.FindPropertyRelative("name").stringValue;
                mat.type = id.FindPropertyRelative("type").stringValue;
                mat.assembly = id.FindPropertyRelative("assembly").stringValue;
                m_MaterialsCache.Add(mat);
            }
        }

        private void BuildExternalObjectsCache()
        {
            if (m_ExternalObjects.hasMultipleDifferentValues)
            {
                // PATCH: Remove this check
                // return;
            }

            m_ExternalObjectsCache.Clear();

            for (int externalObjectIdx = 0, count = m_ExternalObjects.arraySize; externalObjectIdx < count; ++externalObjectIdx)
            {
                var pair = m_ExternalObjects.GetArrayElementAtIndex(externalObjectIdx);

                var cachedObject = new ExternalObjectCache();
                cachedObject.property = pair.FindPropertyRelative("second");
                cachedObject.propertyIdx = externalObjectIdx;

                var externalName = pair.FindPropertyRelative("first.name").stringValue;
                var externalType = pair.FindPropertyRelative("first.type").stringValue;

                m_ExternalObjectsCache.Add(new Tuple<string, string>(externalName, externalType), cachedObject);
            }
        }

        private void EvaluateMaterialExtractionState()
        {
            if (m_Materials.arraySize == 0)
            {
                m_CanExtractEmbeddedMaterials = false;
            }

            //Are there any materials that haven't been extracted?
            foreach (var t in m_ExternalObjects.serializedObject.targetObjects)
            {
                var importer = t as ModelImporter;
                var externalObjectMap = importer.GetExternalObjectMap();
                var materialsList = GetSourceMaterials(importer); // PATCH: Reflect internal call

                var mappedMaterialCount = externalObjectMap.Count(x => x.Key.type == typeof(Material) && x.Value != null && Array.Exists(materialsList, y => y.name == x.Key.name));

                if (mappedMaterialCount != materialsList.Length)
                {
                    m_CanExtractEmbeddedMaterials = true;
                    return;
                }
            }

            m_CanExtractEmbeddedMaterials = false;
        }

        #endregion



        #region Reflected Unity Internals

        private static MethodInfo EditorGUI_DoObjectField;

        private static MethodInfo EditorGUILayout_Popup;

        private static void ReflectUnityInternals()
        {
            // Not using ReflectionMagic for those methods because they have such specific signatures that
            // we need to make sure we're matching the right overload and there is no inference ambiguity

            EditorGUI_DoObjectField = typeof(EditorGUI).GetMethod("DoObjectField", genericParameterCount: 0, BindingFlags.Static | BindingFlags.NonPublic, binder: null, CallingConventions.Any, new[]
            {
                typeof(Rect),
                typeof(Rect),
                typeof(int),
                typeof(Object),
                typeof(Object),
                typeof(Type),
                typeof(EditorGUI).GetNestedType("ObjectFieldValidator", BindingFlags.NonPublic),
                typeof(bool),
                typeof(GUIStyle)
            }, modifiers: null);

            EditorGUILayout_Popup = typeof(EditorGUILayout).GetMethod("Popup", genericParameterCount: 0, BindingFlags.Static | BindingFlags.NonPublic, binder: null, CallingConventions.Any, new[]
            {
                typeof(SerializedProperty),
                typeof(GUIContent[]),
                typeof(GUIContent),
                typeof(GUILayoutOption[])
            }, modifiers: null);
        }

        private static AssetImporter.SourceAssetIdentifier[] GetSourceMaterials(ModelImporter modelImporter)
        {
            return modelImporter.AsDynamic().sourceMaterials;
        }

        private static Object DoObjectField(
            Rect position,
            Rect dropRect,
            int id,
            Object obj,
            Object objBeingEdited,
            Type objType,
            bool allowSceneObjects,
            GUIStyle style = null)
        {
            return (Object)EditorGUI_DoObjectField.Invoke(obj: null, new object[]
            {
                position,
                dropRect,
                id,
                obj,
                objBeingEdited,
                objType,
                null, // Validator
                allowSceneObjects,
                style
            });
        }

        private static void Popup(SerializedProperty property, GUIContent[] displayedOptions, GUIContent label, params GUILayoutOption[] options)
        {
            EditorGUILayout_Popup.Invoke(obj: null, new object[]
            {
                property,
                displayedOptions,
                label,
                options
            });
        }

        #endregion



        #region GUI

        private void DoMaterialsGUI()
        {
            if (targets.Length > 1)
            {
                // PATCH: Remove this check
                // EditorGUILayout.HelpBox(L10n.Tr("Material Editing is not supported on multiple selection"), MessageType.Info);
                // return;
            }

            serializedObject.UpdateIfRequiredOrScript();

            UpdateShowAllMaterialNameOptions();

            using (var horizontal = new EditorGUILayout.HorizontalScope())
            {
                using (var prop = new EditorGUI.PropertyScope(horizontal.rect, Styles.MaterialCreationMode, m_MaterialImportMode))
                {
                    EditorGUI.BeginChangeCheck();
                    var newValue = (int)(ModelImporterMaterialImportMode)EditorGUILayout.EnumPopup(prop.content, (ModelImporterMaterialImportMode)m_MaterialImportMode.intValue);

                    if (EditorGUI.EndChangeCheck())
                    {
                        m_MaterialImportMode.intValue = newValue;
                    }
                }
            }

            var materialHelp = string.Empty;

            if (!m_MaterialImportMode.hasMultipleDifferentValues)
            {
                if (m_MaterialImportMode.intValue != (int)ModelImporterMaterialImportMode.None)
                {
                    if (m_MaterialImportMode.intValue == (int)ModelImporterMaterialImportMode.ImportStandard)
                    {
                        EditorGUILayout.PropertyField(m_UseSRGBMaterialColor, Styles.SRGBMaterialColor);
                    }

                    using (var horizontal = new EditorGUILayout.HorizontalScope())
                    {
                        using (var prop = new EditorGUI.PropertyScope(horizontal.rect, Styles.MaterialLocation, m_MaterialLocation))
                        {
                            EditorGUI.BeginChangeCheck();
                            var newValue = (int)(ModelImporterMaterialLocation)EditorGUILayout.EnumPopup(prop.content, (ModelImporterMaterialLocation)m_MaterialLocation.intValue);

                            if (EditorGUI.EndChangeCheck())
                            {
                                m_MaterialLocation.intValue = newValue;
                            }
                        }
                    }

                    if (!m_MaterialLocation.hasMultipleDifferentValues)
                    {
                        if (m_MaterialLocation.intValue == 0)
                        {
                            // (legacy) we're generating materials in the Materials folder
                            // PATCH: Reflect internal Popup method
                            Popup(m_MaterialName, m_ShowAllMaterialNameOptions ? Styles.MaterialNameOptAll : Styles.MaterialNameOptMain, Styles.MaterialName);
                            Popup(m_MaterialSearch, Styles.MaterialSearchOpt, Styles.MaterialSearch);

                            // PATCH: Hide parts of the message with multiple different values
                            if (!m_MaterialName.hasMultipleDifferentValues)
                            {
                                materialHelp += Styles.ExternalMaterialHelpStart.text.Replace("%MAT%", Styles.ExternalMaterialNameHelp[m_MaterialName.intValue].text) + "\n";
                            }

                            if (!m_MaterialSearch.hasMultipleDifferentValues)
                            {
                                materialHelp += Styles.ExternalMaterialSearchHelp[m_MaterialSearch.intValue].text + "\n";
                            }

                            materialHelp += Styles.ExternalMaterialHelpEnd.text;
                        }
                        else if (m_Materials.arraySize > 0 && m_CanExtractEmbeddedMaterials)
                        {
                            // we're generating materials inside the prefab
                            materialHelp = Styles.InternalMaterialHelp.text;
                        }
                    }

                    // PATCH: Remove targets count check
                    if ( /*targets.Length == 1 &&*/ m_Materials.arraySize > 0 && m_MaterialLocation.intValue != 0)
                    {
                        materialHelp += " " + Styles.MaterialAssignmentsHelp.text;
                    }

                    // display the extract buttons
                    if (m_MaterialLocation.intValue != 0 && !m_MaterialLocation.hasMultipleDifferentValues)
                    {
                        ExtractTexturesGUI();

                        if (ExtractMaterialsGUI())
                        {
                            return;
                        }
                    }
                }
                else
                {
                    // we're not importing materials
                    materialHelp = Styles.NoMaterialHelp.text;
                }
            }

            if (!string.IsNullOrEmpty(materialHelp))
            {
                EditorGUILayout.HelpBox(materialHelp, MessageType.Info);
            }

            // PATCH: Remove targets count check
            if (( /*targets.Length == 1 ||*/ m_SupportsEmbeddedMaterials.hasMultipleDifferentValues == false)
                && m_SupportsEmbeddedMaterials.boolValue == false
                && m_MaterialLocation.intValue != 0 && !m_MaterialLocation.hasMultipleDifferentValues)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(Styles.NoMaterialMappingsHelp.text, MessageType.Warning);
            }

            // PATCH: Remove targets count check
            // PATCH: Remove materials different values check (delayed later)
            // PATCH: Remove external objects different values check (not certain what it prevents, but works without it)
            if (m_MaterialImportMode.intValue != (int)ModelImporterMaterialImportMode.None

                // && targets.Length == 1 
                && m_Materials.arraySize > 0
                && m_MaterialLocation.intValue != 0
                && !m_MaterialLocation.hasMultipleDifferentValues

                // && !m_Materials.hasMultipleDifferentValues 
                // && !m_ExternalObjects.hasMultipleDifferentValues
               )
            {
                GUILayout.Label(Styles.ExternalMaterialMappings, EditorStyles.boldLabel);

                MaterialRemapOptions();

                if (!m_Materials.hasMultipleDifferentValues)
                {
                    DoMaterialRemapList();
                }
            }
        }

        private void ExtractTexturesGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(Styles.Textures);

                using (new EditorGUI.DisabledScope(!m_HasEmbeddedTextures.boolValue &&
                                                   !m_HasEmbeddedTextures.hasMultipleDifferentValues))
                {
                    if (GUILayout.Button(Styles.ExtractEmbeddedTextures))
                    {
                        // when extracting textures, we must handle the case when multiple selected assets could generate textures with the same name at the user supplied path
                        // we proceed as follows:
                        // 1. each asset extracts the textures in a separate temp folder
                        // 2. we remap the extracted assets to the respective asset importer
                        // 3. we generate unique names for each asset and move them to the user supplied path
                        // 4. we re-import all the assets to have the internal materials linked to the newly extracted textures

                        var outputsForTargets = new List<Tuple<ModelImporter, string>>(); // PATCH: Type

                        // use the first target for selecting the destination folder, but apply that path for all targets
                        var assetPath = targets.First().assetPath; // PATCH: .First()
                        assetPath = EditorUtility.SaveFolderPanel("Select Textures Folder",
                                                                  typeof(FileUtil).AsDynamicType().DeleteLastPathNameComponent(assetPath), ""); // PATCH: Internal reflection

                        if (string.IsNullOrEmpty(assetPath))
                        {
                            // cancel the extraction if the user did not select a folder
                            return;
                        }

                        var destinationPath = FileUtil.GetProjectRelativePath(assetPath);

                        if (!destinationPath.StartsWith("Assets"))
                        {
                            // PATCH: Not supporting packages for L0, limitation
                            EditorUtility.DisplayDialog("Extract Textures", "Destination path must be under Assets.", "OK");
                            return;
                            /*
                            assetPath = FileUtil.NiceWinPath(assetPath);

                            // Destination could be a package. Need to get assetPath instead of relativePath
                            // The GUID isn't known yet so can't find it from that
                            var packageInfo = PackageManager.PackageInfo.FindForAssetPath(assetPath);

                            if (packageInfo != null)
                            {
                                destinationPath = packageInfo.assetPath + assetPath.Substring(packageInfo.resolvedPath.Length);
                            }*/
                        }

                        try
                        {
                            // batch the extraction of the textures
                            AssetDatabase.StartAssetEditing();

                            foreach (var t in targets)
                            {
                                var tempPath = FileUtil.GetUniqueTempPathInProject();
                                tempPath = tempPath.Replace("Temp", InternalEditorUtility.GetAssetsFolder());
                                outputsForTargets.Add(Tuple.Create(t, tempPath));

                                var importer = t;
                                importer.ExtractTextures(tempPath);
                            }
                        }
                        finally
                        {
                            AssetDatabase.StopAssetEditing();
                        }

                        try
                        {
                            // batch the remapping and the reimport of the assets
                            AssetDatabase.Refresh();
                            AssetDatabase.StartAssetEditing();

                            foreach (var item in outputsForTargets)
                            {
                                var importer = item.Item1;

                                var guids = AssetDatabase.FindAssets("t:Texture", new[] { item.Item2 });

                                foreach (var guid in guids)
                                {
                                    var path = AssetDatabase.GUIDToAssetPath(guid);
                                    var tex = AssetDatabase.LoadAssetAtPath<Texture>(path);

                                    if (tex == null)
                                    {
                                        continue;
                                    }

                                    importer.AddRemap(new AssetImporter.SourceAssetIdentifier(tex), tex);

                                    var newPath = Path.Combine(destinationPath, typeof(FileUtil).AsDynamicType().UnityGetFileName(path)); // PATCH: Internal reflection
                                    newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);
                                    AssetDatabase.MoveAsset(path, newPath);
                                }

                                AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);

                                AssetDatabase.DeleteAsset(item.Item2);
                            }
                        }
                        finally
                        {
                            AssetDatabase.StopAssetEditing();
                        }
                    }
                }
            }
        }

        private bool ExtractMaterialsGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(Styles.Materials);

                using (new EditorGUI.DisabledScope(!m_CanExtractEmbeddedMaterials))
                {
                    if (GUILayout.Button(Styles.ExtractEmbeddedMaterials))
                    {
                        // use the first target for selecting the destination folder, but apply that path for all targets
                        var destinationPath = targets.First().assetPath; // PATCH: First
                        destinationPath = EditorUtility.SaveFolderPanel("Select Materials Folder",
                                                                        typeof(FileUtil).AsDynamicType().DeleteLastPathNameComponent(destinationPath), ""); // PATCH: Internal reflection

                        if (string.IsNullOrEmpty(destinationPath))
                        {
                            // cancel the extraction if the user did not select a folder
                            return false;
                        }

                        var assetPath = FileUtil.GetProjectRelativePath(destinationPath);

                        //Where all the required embedded materials are not in the asset database, we need to reimport them
                        if (!AllEmbeddedMaterialsAreImported())
                        {
                            if (EditorUtility.DisplayDialog(L10n.Tr("Are you sure you want to re-extract the Materials?"), L10n.Tr("In order to re-extract the Materials we'll need to reimport the mesh, this might take a while. Do you want to continue?"), L10n.Tr("Yes"), L10n.Tr("No")))
                            {
                                ReimportEmbeddedMaterials();
                            }
                            else
                            {
                                return false;
                            }
                        }

                        if (!assetPath.StartsWith("Assets"))
                        {
                            // PATCH: Not supporting packages for L0, limitation
                            EditorUtility.DisplayDialog("Extract Textures", "Destination path must be under Assets.", "OK");
                            return false;
                            /*
                            destinationPath = FileUtil.NiceWinPath(destinationPath);

                            // Destination could be a package. Need to get assetPath instead of relativePath
                            // The GUID isn't known yet so can't find it from that
                            var packageInfo = PackageManager.PackageInfo.FindForAssetPath(destinationPath);

                            if (packageInfo != null)
                            {
                                destinationPath = packageInfo.assetPath + destinationPath.Substring(packageInfo.resolvedPath.Length);
                            }
                            */
                        }

                        if (string.IsNullOrEmpty(assetPath))
                        {
                            return false;
                        }

                        try
                        {
                            // batch the extraction of the textures
                            AssetDatabase.StartAssetEditing();
                            typeof(PrefabUtility).AsDynamicType().ExtractMaterialsFromAsset(targets, assetPath);
                        }
                        finally
                        {
                            AssetDatabase.StopAssetEditing();
                        }

                        // AssetDatabase.StopAssetEditing() invokes OnEnable(), which invalidates all the serialized properties, so we must return.
                        return true;
                    }
                }
            }

            return false;
        }

        public bool AllEmbeddedMaterialsAreImported()
        {
            foreach (ModelImporter modelImporter in m_ExternalObjects.serializedObject.targetObjects)
            {
                //Find the names of embedded materials - the source materials that are not re-mapped in the externalObjectsCache
                // PATCH: Reflected internal method
                var namesOfEmbeddedMaterials = GetSourceMaterials(modelImporter)
                                               .Where(x => !m_ExternalObjectsCache.Any(y => y.Key.Item1 == x.name && y.Value.property != null && y.Value.property.objectReferenceValue != null))
                                               .Select(x => x.name);

                //Find the names of embedded materials in the AssetDatabase
                var namesOfMaterialsInAssetDatabase = AssetDatabase.LoadAllAssetsAtPath(modelImporter.assetPath)
                                                                   .Where(x => x.GetType() == typeof(Material))
                                                                   .Select(x => x.name);

                //Are there any embedded materials that *arent* in the AssetDatabase?
                if (namesOfEmbeddedMaterials.Except(namesOfMaterialsInAssetDatabase).Any())
                {
                    return false;
                }
            }

            return true;
        }

        public void ReimportEmbeddedMaterials()
        {
            //Select any material properties which are marked as "missing"
            var missingMaterialIndexes = m_ExternalObjectsCache.Values.Select((extObj, index) => new { extObj, index })
                                                               .Where(x => x.extObj.property != null && x.extObj.property.objectReferenceValue == null && x.extObj.property.objectReferenceInstanceIDValue != 0)
                                                               .Select(x => x.index)
                                                               .ToArray();

            //Remove missing materials
            for (var i = missingMaterialIndexes.Length - 1; i >= 0; i--)
            {
                m_ExternalObjects.DeleteArrayElementAtIndex(missingMaterialIndexes[i]);
            }

            serializedObject.ApplyModifiedProperties();

            //Force a reimport - any materials marked "None" (including former missing Materials), will now be assigned an embedded material
            // PATCH: All targets
            foreach (var assetImporter in targets)
            {
                AssetDatabase.ImportAsset(assetImporter.assetPath, ImportAssetOptions.ForceUpdate);
            }
        }

        private bool MaterialRemapOptions()
        {
            m_ShowMaterialRemapOptions = EditorGUILayout.Foldout(m_ShowMaterialRemapOptions, Styles.RemapOptions, toggleOnLabelClick: true);

            if (m_ShowMaterialRemapOptions)
            {
                EditorGUI.indentLevel++;

                // PATCH: Reflect internal popup methods
                Popup(m_MaterialName, m_ShowAllMaterialNameOptions ? Styles.MaterialNameOptAll : Styles.MaterialNameOptMain, Styles.MaterialName);
                Popup(m_MaterialSearch, Styles.MaterialSearchOpt, Styles.MaterialSearch);

                // PATCH: Hide parts of message on mixed values
                var searchHelp = string.Empty;

                if (!m_MaterialName.hasMultipleDifferentValues)
                {
                    searchHelp += Styles.ExternalMaterialHelpStart.text.Replace("%MAT%", Styles.ExternalMaterialNameHelp[m_MaterialName.intValue].text) + "\n";
                }

                if (!m_MaterialSearch.hasMultipleDifferentValues)
                {
                    searchHelp += Styles.ExternalMaterialSearchHelp[m_MaterialSearch.intValue].text;
                }

                if (!string.IsNullOrEmpty(searchHelp))
                {
                    EditorGUILayout.HelpBox(searchHelp, MessageType.Info);
                }

                EditorGUI.indentLevel--;

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    // PATCH: Disable disabled scope, not sure when it could happen logically
                    // using (new EditorGUI.DisabledScope(assetTarget == null))
                    {
                        if (GUILayout.Button(Styles.RemapMaterialsInProject))
                        {
                            Undo.RecordObjects(targets, "Search and Remap Materials");

                            foreach (var t in targets)
                            {
                                var importer = t;

                                // SearchAndReplaceMaterials will ensure the material name and search options get saved, while all other pending changes stay pending.
                                importer.SearchAndRemapMaterials((ModelImporterMaterialName)m_MaterialName.intValue, (ModelImporterMaterialSearch)m_MaterialSearch.intValue);
                            }

                            ResetValues();
                            EditorGUIUtility.ExitGUI();

                            return true;
                        }
                    }
                }

                EditorGUILayout.Space();
            }

            return false;
        }

        private void DoMaterialRemapList()
        {
            // OnEnabled is not called consistently when the asset gets reimported, we need to rebuild the cache here if it's outdated.
            if (m_ExternalObjects.arraySize != m_ExternalObjectsCache.Count())
            {
                ResetValues();
            }

            // The list of material names is immutable, whereas the map of external objects can change based on user actions.
            // For each material name, map the external object associated with it where one exists.
            for (var materialIdx = 0; materialIdx < m_MaterialsCache.Count; ++materialIdx)
            {
                var mat = m_MaterialsCache[materialIdx];

                var hasMatchingCachedObject = m_ExternalObjectsCache.TryGetValue(new Tuple<string, string>(mat.name, mat.type), out var cachedExternalObject) && cachedExternalObject != null;

                if (hasMatchingCachedObject)
                {
                    //The material already has a serialized property, so use it!
                    MaterialPropertyGUI(mat, cachedExternalObject);
                }
                else
                {
                    //The material doesn't have a serialized property, so it's going to have to draw the GUI a different way!
                    MaterialPropertyGUI(mat);
                }
            }
        }

        private void MaterialPropertyGUI(MaterialCache materialCache, ExternalObjectCache externalObjectCache)
        {
            if (DebugMode)
            {
                GUI.color = Color.green;
            }

            var nameLabel = new GUIContent(materialCache.name);
            nameLabel.tooltip = materialCache.name;

            EditorGUI.BeginChangeCheck();

            var property = externalObjectCache.property;
            var previousMaterial = property.objectReferenceValue as Material;
            EditorGUILayout.ObjectField(property, typeof(Material), nameLabel);
            var material = property.objectReferenceValue as Material;

            if (EditorGUI.EndChangeCheck())
            {
                if (material == null)
                {
                    m_ExternalObjects.DeleteArrayElementAtIndex(externalObjectCache.propertyIdx);
                    BuildExternalObjectsCache();
                }
                else
                {
                    // PATCH: Check all targets
                    var isMaterialValid = true;

                    foreach (var target in targets)
                    {
                        if (AssetDatabase.GetAssetPath(material) == target.assetPath)
                        {
                            Debug.LogError($"{material.name} is a sub-asset of {Path.GetFileName(target.assetPath)} and cannot be used as an external material.");
                            isMaterialValid = false;
                            break;
                        }
                    }

                    if (!isMaterialValid)
                    {
                        property.objectReferenceValue = previousMaterial;
                    }
                    else
                    {
                        var pair = m_ExternalObjects.GetArrayElementAtIndex(externalObjectCache.propertyIdx);
                        pair.FindPropertyRelative("second").objectReferenceValue = material;
                        BuildExternalObjectsCache();
                    }
                }

                EvaluateMaterialExtractionState();
            }
        }

        private void MaterialPropertyGUI(MaterialCache materialCache)
        {
            if (DebugMode)
            {
                GUI.color = Color.yellow;
            }

            var nameLabel = new GUIContent(materialCache.name);
            nameLabel.tooltip = materialCache.name;

            EditorGUI.BeginChangeCheck();
            var material = ObjectFieldWithPPtrHashID(nameLabel, obj: null, typeof(Material), allowSceneObjects: false) as Material;

            if (EditorGUI.EndChangeCheck())
            {
                if (material != null)
                {
                    // PATCH: Check all targets
                    var isMaterialValid = true;

                    foreach (var target in targets)
                    {
                        if (AssetDatabase.GetAssetPath(material) == target.assetPath)
                        {
                            Debug.LogError($"{material.name} is a sub-asset of {Path.GetFileName(target.assetPath)} and cannot be used as an external material.");
                            isMaterialValid = false;
                            break;
                        }
                    }

                    if (isMaterialValid)
                    {
                        m_ExternalObjects.arraySize++;
                        var pair = m_ExternalObjects.GetArrayElementAtIndex(m_ExternalObjects.arraySize - 1);
                        pair.FindPropertyRelative("first.name").stringValue = materialCache.name;
                        pair.FindPropertyRelative("first.type").stringValue = materialCache.type;
                        pair.FindPropertyRelative("first.assembly").stringValue = materialCache.assembly;
                        pair.FindPropertyRelative("second").objectReferenceValue = material;

                        // ExternalObjects is serialized as a map, so items are reordered when deserializing.
                        // We need to update the serializedObject to trigger the reordering before rebuilding the cache.
                        serializedObject.ApplyModifiedProperties();

                        // PATCH: Apply immediately, because ApplyModifiedProperties will eat the properties changed at the end of GUI
                        Apply();
                        GUIUtility.ExitGUI();

                        // PATCH: Useless with instant-apply
                        // serializedObject.Update();
                        // BuildExternalObjectsCache();
                    }
                }

                EvaluateMaterialExtractionState();
            }

            if (DebugMode)
            {
                GUI.color = Color.white;
            }
        }

        // Taken from EditorGUI in order to work arround the issue of ObjectField using different ControlIDs when a Serialized property is passed as argument.
        private static Object ObjectFieldWithPPtrHashID(GUIContent label, Object obj, Type objType, bool allowSceneObjects, params GUILayoutOption[] options)
        {
            var EditorGUIDynamic = typeof(EditorGUI).AsDynamicType();
            float height = EditorGUIUtility.HasObjectThumbnail(objType) ? EditorGUIDynamic.kObjectFieldThumbnailHeight : EditorGUIDynamic.kSingleLineHeight; // PATCH: Reflected internal
            var position = EditorGUILayout.GetControlRect(hasLabel: true, height, options);

            var id = GUIUtility.GetControlID(s_PPtrHash, FocusType.Keyboard, position);
            position = EditorGUI.PrefixLabel(position, id, label);

            if (EditorGUIUtility.HasObjectThumbnail(objType) && position.height > EditorGUIDynamic.kSingleLineHeight)
            {
                // Make object field with thumbnail quadratic and align to the right
                var size = Mathf.Min(position.width, position.height);
                position.height = size;
                position.xMin = position.xMax - size;
            }

            // PATCH: Reflected internal method
            return DoObjectField(position, position, id, obj, objBeingEdited: null, objType, allowSceneObjects);
        }

        #endregion



        #region Subtypes

        private class ExternalObjectCache
        {
            public int propertyIdx;

            public SerializedProperty property;
        }

        private class MaterialCache
        {
            public string name;

            public string type;

            public string assembly;
        }

        private static class Styles
        {
            public static GUIContent MaterialLocation = EditorGUIUtility.TrTextContent("Location");

            public static GUIContent MaterialName = EditorGUIUtility.TrTextContent("Naming");

            public static GUIContent[] MaterialNameOptMain =
            {
                EditorGUIUtility.TrTextContent("By Base Texture Name"),
                EditorGUIUtility.TrTextContent("From Model's Material"),
                EditorGUIUtility.TrTextContent("Model Name + Model's Material")
            };

            public static GUIContent[] MaterialNameOptAll =
            {
                EditorGUIUtility.TrTextContent("By Base Texture Name"),
                EditorGUIUtility.TrTextContent("From Model's Material"),
                EditorGUIUtility.TrTextContent("Model Name + Model's Material"),
                EditorGUIUtility.TrTextContent("Texture Name or Model Name + Model's Material (Obsolete)")
            };

            public static GUIContent MaterialSearch = EditorGUIUtility.TrTextContent("Search");

            public static GUIContent[] MaterialSearchOpt =
            {
                EditorGUIUtility.TrTextContent("Local Materials Folder"),
                EditorGUIUtility.TrTextContent("Recursive-Up"),
                EditorGUIUtility.TrTextContent("Project-Wide")
            };

            public static GUIContent NoMaterialHelp = EditorGUIUtility.TrTextContent("Do not generate materials. Use Unity's default material instead.");

            public static GUIContent ExternalMaterialHelpStart = EditorGUIUtility.TrTextContent("For each imported material, Unity first looks for an existing material named %MAT%.");

            public static GUIContent[] ExternalMaterialNameHelp =
            {
                EditorGUIUtility.TrTextContent("[BaseTextureName]"),
                EditorGUIUtility.TrTextContent("[MaterialName]"),
                EditorGUIUtility.TrTextContent("[ModelFileName]-[MaterialName]"),
                EditorGUIUtility.TrTextContent("[BaseTextureName] or [ModelFileName]-[MaterialName] if no base texture can be found")
            };

            public static GUIContent[] ExternalMaterialSearchHelp =
            {
                EditorGUIUtility.TrTextContent("Unity will look for it in the local Materials folder."),
                EditorGUIUtility.TrTextContent("Unity will do a recursive-up search for it in all Materials folders up to the Assets folder."),
                EditorGUIUtility.TrTextContent("Unity will search for it anywhere inside the Assets folder.")
            };

            public static GUIContent ExternalMaterialHelpEnd = EditorGUIUtility.TrTextContent("If it doesn't exist, a new one is created in the local Materials folder.");

            public static GUIContent InternalMaterialHelp = EditorGUIUtility.TrTextContent("Materials are embedded inside the imported asset.");

            public static GUIContent MaterialAssignmentsHelp = EditorGUIUtility.TrTextContent("Material assignments can be remapped below.");

            public static GUIContent ExternalMaterialMappings = EditorGUIUtility.TrTextContent("Remapped Materials", "External materials to use for each embedded material.");

            public static GUIContent NoMaterialMappingsHelp = EditorGUIUtility.TrTextContent("Re-import the asset to see the list of used materials.");

            public static GUIContent Textures = EditorGUIUtility.TrTextContent("Textures");

            public static GUIContent ExtractEmbeddedTextures = EditorGUIUtility.TrTextContent("Extract Textures...", "Click on this button to extract the embedded textures.");

            public static GUIContent Materials = EditorGUIUtility.TrTextContent("Materials");

            public static GUIContent ExtractEmbeddedMaterials = EditorGUIUtility.TrTextContent("Extract Materials...", "Click on this button to extract the embedded materials.");

            public static GUIContent RemapOptions = EditorGUIUtility.TrTextContent("On Demand Remap");

            public static GUIContent RemapMaterialsInProject = EditorGUIUtility.TrTextContent("Search and Remap", "Click on this button to search and remap the materials from the project.");

            public static GUIContent SRGBMaterialColor = EditorGUIUtility.TrTextContent("sRGB Albedo Colors", "Albedo colors in gamma space. Disable this for projects using linear color space.");

            public static GUIContent MaterialCreationMode = EditorGUIUtility.TrTextContent("Material Creation Mode", "Select the method used to generate materials during the import process.");
        }

        #endregion
    }
}
