//
//  SuperCombinerEditor.cs
//
//  Author:
//       Lunar Cats Studio <lunarcatsstudio@gmail.com>
//
//  Copyright (c) 2018 Lunar Cats Studio

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LunarCatsStudio.SuperCombiner;

namespace LunarCatsStudio.SuperCombiner
{

    /// <summary>
    /// Super combiner editor class, manage gui editor for interact with super combiner script
    /// </summary>
    [CustomEditor(typeof(SuperCombiner))]
    public class SuperCombinerEditor : Editor
    {

        #region Inspector
        //private enum CombineStatesList {Uncombined, Combining, Combined}
        // Reference to the SuperCombiner script
        private SuperCombiner _superCombiner;

        public List<int> _TextureAtlasSizes = new List<int>() {
            32, 64, 128, 256, 512, 1024, 2048, 4096, 8192
        };
        public List<string> _TextureAtlasSizesNames = new List<string>() {
            "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192"
        };

        public bool _include_dependencies = true;
        // Constants
        public const int MAX_MULTI_MATERIAL_COUNT = 10;


        // Serialized
        private SerializedObject _serializedCombiner;
        private SerializedProperty _customShaderProperties;
        private List<SerializedProperty> _multiMaterialsSC = new List<SerializedProperty>();
        private List<int> _multiMaterialsOrder = new List<int>();

        // Scroll views
        private Vector2 _originalMaterialsPosition;
        private Vector2 _combinedMaterialsPosition;
        private Vector2 _combinedMeshsPosition;

        // Editor Foldouts
        public bool _showInstructions = true;
        public bool _showCombineSettings = false;
        public bool _showMeshSettings = false;
        public bool _showTextureSettings = true;
        public bool _showAdditionalParameters = false;
        public bool _showMeshResults = false;
        public bool _showOriginalMaterials = false;
        public bool _showCombinedAtlas = false;
        public bool _showCombinedMaterials = false;
        public bool _showCombinedMesh = false;
        public bool _showSaveOptions = false;
        public bool _showMultiMaterials = false;
        public bool _showPackageOptions = true;

        // Info popup to display more information about combining results
        private InfoPopup infoPopup;

        /// <summary>
        /// Raises the enable event.
        /// </summary>
        private void OnEnable()
        {
            _superCombiner = (SuperCombiner)target;

            _serializedCombiner = new SerializedObject(_superCombiner);
            _customShaderProperties = _serializedCombiner.FindProperty("_customTextureProperies");

            for (int i = 0; i < MAX_MULTI_MATERIAL_COUNT; i++)
            {
                _multiMaterialsSC.Add(_serializedCombiner.FindProperty("multiMaterials" + i));
                _multiMaterialsOrder.Add(i);
            }
        }

        /// <summary>
        /// Raises the inspector GUI event.
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorStyles.whiteBoldLabel.fontSize = 15;

            DisplayHelpSection();
            // Display settings sections
            GUILayout.Label("Combine Settings", EditorStyles.whiteBoldLabel);

            DisplayMainSettingsSection();
            DisplayTextureSettingsSection();
            DisplayMeshesSettingsSection();
            DisplayCombineButton();

            // Display results sections
            if (_superCombiner._combiningState == SuperCombiner.CombineStatesList.Combined)
            {
                GUILayout.Label("Combine results", EditorStyles.whiteBoldLabel);

                if (_superCombiner._combinedResult != null)
                {
                    DisplayWarningMessages();
                    DisplayMeshStatsSection();
                    DisplayCombinedAtlasSection();
                    DisplayOriginalMaterialsSection();
                    DisplayCombinedMaterialsSection();
                    DisplayCombinedMeshSection();
                    DisplaySaveSection();
                }
                else
                {
                    GUILayout.Label("No reference of combine results have been found");
                }
            }
            else if (_superCombiner._combiningState == SuperCombiner.CombineStatesList.CombinedMaterials)
            {
                GUILayout.Label("Combine results", EditorStyles.whiteBoldLabel);

                if (_superCombiner._combinedResult != null)
                {
                    DisplayWarningMessages();
                    DisplayCombinedAtlasSection();
                    DisplayOriginalMaterialsSection();
                    DisplayCombinedMaterialsSection();
                    DisplaySaveSection();
                }
                else
                {
                    GUILayout.Label("No reference of combine results have been found");
                }
            }

            _serializedCombiner.ApplyModifiedProperties();
            _serializedCombiner.Update();
            /*#if UNITY_2017_1_OR_NEWER
                        EditorGUIUtility.ExitGUI();
            #endif*/
        }

        /// <summary>
        /// 
        /// </summary>
        private void UnCombineSession()
        {
            if (infoPopup != null)
            {
                infoPopup.Close();
            }
            _superCombiner.UnCombine();
        }

        /// <summary>
        /// Display the combine button.
        /// </summary>
        private void DisplayCombineButton()
        {
            EditorGUILayout.Space();

            if (_superCombiner._combiningState == SuperCombiner.CombineStatesList.Uncombined)
            {
                if (GUILayout.Button(new GUIContent("Combine", "This will launch the combine process of combining materials to create atlas textures and the combine meshes to adjust UVs so that they fit the new atlas."), GUILayout.MinHeight(30)))
                {
                    _superCombiner.CombineChildren();
                }
            }
            else if (_superCombiner._combiningState == SuperCombiner.CombineStatesList.Combining)
            {
                EditorGUILayout.Space();
                if (GUILayout.Button(new GUIContent("Uncombine", "This will revert the combine process so that everything will be back to normal."), GUILayout.MinHeight(30)))
                {
                    UnCombineSession();
                }
                Rect r = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(r, 0.1f, "Combining in progress ... ");
                GUILayout.Space(20);
                EditorGUILayout.EndVertical();
            }
            else if (_superCombiner._combiningState == SuperCombiner.CombineStatesList.CombinedMaterials)
            {
                if (GUILayout.Button(new GUIContent("Combine meshes", "This will finish the combine process by combining meshes to adjust UVs so that they fit the new atlas."), GUILayout.MinHeight(30)))
                {
                    _superCombiner.SetTargetParentForCombinedGameObject();
                    _superCombiner.CombineMeshes(_superCombiner._meshList, _superCombiner._skinnedMeshList, _superCombiner._targetParentForCombinedGameObjects.transform);
                }
            }
            else
            {
                if (GUILayout.Button(new GUIContent("Uncombine", "This will revert the combine process so that everything will be back to normal."), GUILayout.MinHeight(30)))
                {
                    UnCombineSession();
                }
            }
        }


        /// <summary>
        /// Display the header (version number and instructions).
        /// </summary>
        private void DisplayHelpSection()
        {
            EditorGUILayout.Space();

            _showInstructions = EditorGUILayout.Foldout(_showInstructions, "Instructions for Super Combiner (v " + _superCombiner.versionNumber + ")");
            if (_showInstructions)
            {
                GUILayout.Label("Put all you prefabs to combine as children of me. " +
                    "Select your session name, the texture atlas size and whether or not to combine meshes. " +
                    "When you are ready click 'Combine' button to _start the process (it may take a while depending on the quantity of different assets). " +
                    "When the process is finished you'll see the result on the scene (all original mesh renderers will be deactivated). " +
                    "If you want to save the combined assets, select your saving options and click 'Save' button. " +
                    "To revert the process just click 'Uncombine' button.", EditorStyles.helpBox);
            }

            EditorGUILayout.Space();
        }


        /// <summary>
        /// Display the main section.
        /// </summary>
        private void DisplayMainSettingsSection()
        {
            _showCombineSettings = EditorGUILayout.Foldout(_showCombineSettings, "General Settings:");
            if (_showCombineSettings)
            {
                if (_superCombiner._combiningState == SuperCombiner.CombineStatesList.Uncombined)
                {
                    GUI.enabled = true;
                }
                else
                {
                    GUI.enabled = false;
                }

                _superCombiner._sessionName = EditorGUILayout.TextField(new GUIContent("Session name", "Your session name should be different for every SuperCombiner instance. Avoid using special characters."), _superCombiner._sessionName, GUILayout.ExpandWidth(true));
                _superCombiner._combineAtRuntime = EditorGUILayout.Toggle(new GUIContent("Combine at runtime?", "Set to true if you want the process to combine at startup during runtime (beware that combining is a complex task that may takes some time to process)"), _superCombiner._combineAtRuntime);

                GUI.enabled = true;
            }
        }


        /// <summary>
        /// Display the texture section.
        /// </summary>
        private void DisplayTextureSettingsSection()
        {
            _showTextureSettings = EditorGUILayout.Foldout(_showTextureSettings, "Texture Atlas Settings:");
            if (_showTextureSettings)
            {
                if (_superCombiner._combiningState == SuperCombiner.CombineStatesList.Uncombined)
                    GUI.enabled = true;
                else
                    GUI.enabled = false;

                //GUILayout.Label ("Texture Atlas", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("The first material found in all game objects to combine will be used as a reference for the combined material.", EditorStyles.wordWrappedMiniLabel);

                // Atlas Texture Size choice
                _superCombiner._textureAtlasSize = EditorGUILayout.IntPopup("Texture Atlas size", _superCombiner._textureAtlasSize, _TextureAtlasSizesNames.ToArray(), _TextureAtlasSizes.ToArray(), GUILayout.ExpandWidth(true));

                _showAdditionalParameters = EditorGUILayout.Foldout(_showAdditionalParameters, "Additional parameters");
                if (_showAdditionalParameters)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    // Multi materials group
                    DisplayMultiMaterialSettingsSection();

                    // Custom Shader propertues
                    EditorGUILayout.PropertyField(_customShaderProperties, new GUIContent("Custom shader properties", "Super Combiner uses the list of texture properties from standard shader. If you are using custom shader with different texture properties, add their exact name in the list."), true);

                    // Tiling factor
                    _superCombiner._tilingFactor = EditorGUILayout.Slider(new GUIContent("tiling factor", "Apply a tiling factor on the textures. This may be helpfull if you observe strange artifacts after combining materials with heightmap"), _superCombiner._tilingFactor, 1, 2, GUILayout.ExpandWidth(true));

                    //Atlas Padding
                    _superCombiner._atlasPadding = EditorGUILayout.IntField(new GUIContent("padding", "Padding between textures in the atlas"), _superCombiner._atlasPadding, GUILayout.ExpandWidth(true));

                    // Force UV to [0, 1] mode
                    //_superCombiner._forceUVTo0_1 = EditorGUILayout.Toggle(new GUIContent("Force UV to [0,1]", "Only consider UV that are in [0, 1] range so that textures won't be tiled in the atlas"), _superCombiner._forceUVTo0_1);

                    EditorGUILayout.EndVertical();
                }

                if (_superCombiner._combiningState == SuperCombiner.CombineStatesList.Uncombined)
                {
                    if (GUILayout.Button(new GUIContent("Create atlas texture", "This will combine materials and create the atlas texture(s) only. This is usefull to check if atlas texture(s) are correct without having to combine meshes which is time consuming. When materials have been combined, you'll need to hit 'Combine' button to finish the process and combine meshes."), GUILayout.MinHeight(20)))
                    {
                        _superCombiner.FindMeshesToCombine();
                        //_superCombiner.InitializeMultipleMaterialElements();
                        _superCombiner.CombineMaterials(_superCombiner._meshList, _superCombiner._skinnedMeshList);
                    }
                }
                else if (_superCombiner._combiningState == SuperCombiner.CombineStatesList.CombinedMaterials)
                {
                    GUI.enabled = true;
                    if (GUILayout.Button("Uncombine materials", GUILayout.MinHeight(20)))
                    {
                        UnCombineSession();
                    }
                }

                EditorGUILayout.EndVertical();
                GUI.enabled = true;
            }
        }

        /// <summary>
        /// Display the multi _material section
        /// </summary>
        private void DisplayMultiMaterialSettingsSection()
        {
            _superCombiner._multipleMaterialsMode = EditorGUILayout.Toggle(new GUIContent("Multiple materials", "The multi material feature lets you combine to several materials (up to 10) from the listed source materials. This is usually usefull when combining meshes that have various materials (submeshes) that cannot be combined together."), _superCombiner._multipleMaterialsMode);
            if (_superCombiner._multipleMaterialsMode)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.LabelField("Define here every source materials for each combined material. If more materials than the one listed below are found, they will be automatically assigned to the last combined material", EditorStyles.wordWrappedLabel);

                _superCombiner._combineEachGroupAsSubmesh = EditorGUILayout.Toggle(new GUIContent("Set as submesh", "If set to true, each combined mesh for each material source group will be a submesh of the final combine mesh. If set to false, each material source group will be a separate mesh in a separate GameObject."), _superCombiner._combineEachGroupAsSubmesh);

                // Foldout
                EditorGUILayout.BeginHorizontal();
                _showMultiMaterials = EditorGUILayout.Foldout(_showMultiMaterials, "combined materials (" + _superCombiner._multiMaterialsCount + ")");

                // Add new _material group button
                EditorGUI.BeginDisabledGroup(_superCombiner._multiMaterialsCount >= MAX_MULTI_MATERIAL_COUNT);
                if (GUILayout.Button("+", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(20f)))
                {
                    _superCombiner._multiMaterialsCount++;
                }
                EditorGUI.EndDisabledGroup();

                // Remove new _material group button
                EditorGUI.BeginDisabledGroup(_superCombiner._multiMaterialsCount == 0);
                if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.MaxWidth(20f)))
                {
                    _superCombiner._multiMaterialsCount--;
                    _serializedCombiner.Update();
                    _multiMaterialsSC[_multiMaterialsOrder[_superCombiner._multiMaterialsCount]].ClearArray();
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                if (_showMultiMaterials)
                {
                    for (int i = 0; i < _superCombiner._multiMaterialsCount; i++)
                    {
                        EditorGUILayout.BeginHorizontal();

                        // Source materials
                        EditorGUILayout.PropertyField(_multiMaterialsSC[_multiMaterialsOrder[i]], new GUIContent("source materials (group " + _multiMaterialsOrder[i] + ")", "Define here all the source material to be included in combined material " + _multiMaterialsOrder[i]), true);

                        // Remove a _material group button
                        if (GUILayout.Button(new GUIContent("-", "remove this combined material"), EditorStyles.miniButtonRight, GUILayout.MaxWidth(20f)))
                        {
                            _superCombiner._multiMaterialsCount--;
                            _serializedCombiner.Update();
                            _multiMaterialsSC[_multiMaterialsOrder[i]].ClearArray();

                            SerializedProperty tmp = _multiMaterialsSC[i];
                            _multiMaterialsSC.RemoveAt(i);
                            _multiMaterialsSC.Add(tmp);
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        /// <summary>
        /// Display the meshes section.
        /// </summary>
        private void DisplayMeshesSettingsSection()
        {
            _showMeshSettings = EditorGUILayout.Foldout(_showMeshSettings, "Meshes Settings:");
            if (_showMeshSettings)
            {
                if (_superCombiner._combiningState == SuperCombiner.CombineStatesList.Uncombined || _superCombiner._combiningState == SuperCombiner.CombineStatesList.CombinedMaterials)
                {
                    GUI.enabled = true;
                }
                else
                {
                    GUI.enabled = false;
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Combine MeshSettings
                _superCombiner._combineMeshes = EditorGUILayout.Toggle(new GUIContent("Combine meshes?", "If set to false, only materials and textures will be combined, all meshes will remain separated. If set to true, all meshes will be combined into a unique combined mesh."), _superCombiner._combineMeshes);
                if (_superCombiner._combineMeshes)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    _superCombiner._generateUv2 = EditorGUILayout.Toggle(new GUIContent("Generate UV2?", "If set to true, Super Combiner will generate UV2 for the combined mesh."), _superCombiner._generateUv2);
                    _superCombiner._meshOutput = EditorGUILayout.IntPopup(new GUIContent("Mesh output", "Chose to combine into a Mesh or a SkinnedMesh. Combining into SkinnedMesh is in alpha release, it will only works properly if there are only SkinnedMeshes as input. Combining Meshes and SkinnedMeshes into a SkinnedMesh is not supported yet."), _superCombiner._meshOutput, new GUIContent[] {
                        new GUIContent ("Mesh"),
                        new GUIContent ("SkinnedMesh (alpha)")
                    }, new int[] {
                        0,
                        1
                    }, GUILayout.ExpandWidth(true));

                    // Collider Settings
                    _superCombiner._manageColliders = EditorGUILayout.Toggle(new GUIContent("Include colliders", "If set to true, SuperCombiner will integrate all colliders into the combined GameObject"), _superCombiner._manageColliders);

                    EditorGUILayout.EndVertical();
                }

                // LOD Level Settings
                EditorGUILayout.BeginHorizontal();
                _superCombiner._manageLodLevel = EditorGUILayout.Toggle(new GUIContent("Manage LOD level", "If set to true, SuperCombiner will only take into account the specified LOD level for each LODGroup in the list of GameObjects to combine."), _superCombiner._manageLodLevel);
                if (_superCombiner._manageLodLevel)
                {
                    _superCombiner._managedLodLevel = EditorGUILayout.IntField(new GUIContent("LOD level to take into account", "LOD Level to take into account"), _superCombiner._managedLodLevel, GUILayout.ExpandWidth(true));
                }
                EditorGUILayout.EndHorizontal();


                // Target GameObject Settings
                _superCombiner._targetGameObject = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Target GameObject", "The GameObject into which the combined GameObject(s) will be created. If you leave it empty, a new GameObject will be created under this GameObject with the name of you session name."), _superCombiner._targetGameObject, typeof(GameObject), true);

                /*if (_superCombiner._combiningState == SuperCombiner.CombineStatesList.CombinedMaterials)
                {
                    if (GUILayout.Button("Combine meshes", GUILayout.MinHeight(20)))
                    {
                        _superCombiner.SetTargetParentForCombinedGameObject();
                        _superCombiner.CombineMeshes(_superCombiner._meshList, _superCombiner._skinnedMeshList, _superCombiner._targetParentForCombinedGameObjects.transform);
                    }
                }*/

                EditorGUILayout.EndVertical();
                GUI.enabled = true;
            }
        }
        #endregion  // Inspector

        #region CombinedResult
        /// <summary>
        /// Display warning messages and button to display InfoPopup if needed
        /// </summary>
        private void DisplayWarningMessages()
        {
            /* if (_superCombiner._combinedResult._warningMessages.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUIContent warnMessage = new GUIContent("The combine process has generated some warnings", EditorGUIUtility.IconContent("console.warnicon").image);
                GUIContent guiContentShowWarningsButton = new GUIContent("Show", "Click here to get more details about possible issues with this combine session");
                GUIContent guiContentShowWarningsButtonHide = new GUIContent("Hide", "Click here to get more details about possible issues with this combine session");
                GUILayout.Box(warnMessage);
                if (infoPopup == null)
                {
                    if (GUILayout.Button(guiContentShowWarningsButton, GUILayout.ExpandHeight(true)))
                    {
                        infoPopup = ScriptableObject.CreateInstance<InfoPopup>();
                        infoPopup.position = new Rect(Screen.width, Screen.height / 2, 600, 250);
                        //infoPopup.text = Screen.width + " / " + Screen.height;
                        foreach (string warning in _superCombiner._combinedResult._warningMessages)
                        {
                            infoPopup.text.Add(warning);
                        }

                        infoPopup.ShowPopup();
                    }
                }
                else
                {
                    if (GUILayout.Button(guiContentShowWarningsButtonHide, GUILayout.ExpandHeight(true)))
                    {
                        infoPopup.Close();
                    }
                }
                EditorGUILayout.EndHorizontal();
            } */
        }

        /// <summary>
        /// Display the stats.
        /// </summary>
        private void DisplayMeshStatsSection()
        {
            _showMeshResults = EditorGUILayout.Foldout(_showMeshResults, "Meshes:");
            if (_showMeshResults)
            {
                GUILayout.Label("Found " + _superCombiner._combinedResult._meshesCombinedCount + " different mesh(s)");
                if (_superCombiner._skinnedMeshList.Count > 0)
                {
                    GUILayout.Label("Found " + _superCombiner._combinedResult._skinnedMeshesCombinedCount + " different skinned mesh(es)");
                }
            }
        }

        /// <summary>
        /// Display the combined atlas.
        /// </summary>
        private void DisplayCombinedAtlasSection()
        {
            _showCombinedAtlas = EditorGUILayout.Foldout(_showCombinedAtlas, "Combined Atlas textures:");
            if (_showCombinedAtlas)
            {
                foreach (TexturePacker texturePacker in _superCombiner._texturePackers)
                {
                    if (texturePacker != null && texturePacker._packedTextures.Count > 0)
                    {
                        EditorGUILayout.LabelField("Combined _material " + _superCombiner._combinedResult._combinedMaterials[texturePacker.CombinedIndex].displayedIndex, EditorStyles.boldLabel);
                        foreach (KeyValuePair<string, Texture2D> keyValue in texturePacker._packedTextures)
                        {
                            if (keyValue.Value != null)
                            {
                                string PropertyName = keyValue.Key;
                                texturePacker.TexturePropertyNames.TryGetValue(keyValue.Key, out PropertyName);
                                EditorGUILayout.BeginVertical();
                                //EditorGUILayout.PrefixLabel(PropertyName + " AtlasTexture preview:);
                                EditorGUILayout.ObjectField(PropertyName + ":", keyValue.Value, typeof(Texture2D), false);
                                EditorGUILayout.EndVertical();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Display the original _material(s) section.
        /// </summary>
        private void DisplayOriginalMaterialsSection()
        {
            _showOriginalMaterials = EditorGUILayout.Foldout(_showOriginalMaterials, "Original Materials (" + _superCombiner._combinedResult._materialCombinedCount + ")");
            if (_showOriginalMaterials)
            {
                if (_superCombiner._combinedResult._materialCombinedCount > 8)
                {
                    _originalMaterialsPosition = EditorGUILayout.BeginScrollView(_originalMaterialsPosition, GUILayout.MinHeight(150));
                }
                for (int j = 0; j < _superCombiner._combinedResult._originalMaterialList.Count; j++)
                {
                    foreach (MaterialToCombine mat in _superCombiner._combinedResult._originalMaterialList[j].Values)
                    {
                        EditorGUILayout.ObjectField("", mat._material, typeof(Material), false);
                    }
                }
                if (_superCombiner._combinedResult._materialCombinedCount > 8)
                {
                    EditorGUILayout.EndScrollView();
                }
            }
            if (!Selection.activeTransform)
            {
                _showOriginalMaterials = false;
            }
        }


        /// <summary>
        /// Display the combined _material section.
        /// </summary>
        private void DisplayCombinedMaterialsSection()
        {
            _showCombinedMaterials = EditorGUILayout.Foldout(_showCombinedMaterials, "Combined Materials (" + _superCombiner._combinedResult._combinedMaterialCount + ")");

            if (_showCombinedMaterials)
            {
                for (int i = 0; i < _superCombiner._combinedResult._combinedMaterials.Count; i++)
                {
                    // TODO: The order must be correct
                    if (_superCombiner._combinedResult._combinedMaterials[i].material != null)
                    {
                        EditorGUILayout.ObjectField("", _superCombiner._combinedResult._combinedMaterials[i].material, typeof(Material), false);
                    }
                }
            }
            if (!Selection.activeTransform)
            {
                _showCombinedMaterials = false;
            }
        }


        /// <summary>
        /// Display the combined mesh(es) section.
        /// </summary>
        private void DisplayCombinedMeshSection()
        {
            // Display created meshes
            if (_superCombiner._combineMeshes)
            {
                _showCombinedMesh = EditorGUILayout.Foldout(_showCombinedMesh, "Combined Meshs (" + _superCombiner._combinedResult._meshResults.Count + ")");
                if (_showCombinedMesh)
                {
                    if (_superCombiner._combinedResult._meshResults.Count > 5)
                    {
                        _combinedMeshsPosition = EditorGUILayout.BeginScrollView(_combinedMeshsPosition, GUILayout.MinHeight(100));
                    }
                    for (int i = 0; i < _superCombiner._combinedResult._combinedGameObjectFromMeshList.Count; i++)
                    {
                        // Meshes
                        if (_superCombiner._meshOutput == 0)
                        {
                            if (_superCombiner._combinedResult._combinedGameObjectFromMeshList[i].Count > 0)
                            {
                                for (int j = 0; j < _superCombiner._combinedResult._combinedGameObjectFromMeshList[i].Count; j++)
                                {
                                    EditorGUILayout.ObjectField("", _superCombiner._combinedResult._combinedGameObjectFromMeshList[i][j].GetComponent<MeshFilter>().sharedMesh, typeof(MeshFilter), false);
                                }
                            }
                        }
                        // SkinnedMeshes
                        else if (_superCombiner._meshOutput == 1)
                        {
                            if (_superCombiner._combinedResult._combinedGameObjectFromSkinnedMeshList[i].Count > 0)
                            {
                                for (int j = 0; j < _superCombiner._combinedResult._combinedGameObjectFromSkinnedMeshList[i].Count; j++)
                                {
                                    EditorGUILayout.ObjectField("", _superCombiner._combinedResult._combinedGameObjectFromSkinnedMeshList[i][j].GetComponent<SkinnedMeshRenderer>().sharedMesh, typeof(MeshFilter), false);
                                }
                            }
                        }
                    }
                    if (_superCombiner._combinedResult._meshResults.Count > 5)
                    {
                        EditorGUILayout.EndScrollView();
                    }
                }
            }
        }


        /// <summary>
        /// Display the save section.
        /// </summary>
        private void DisplaySaveSection()
        {
            // Saving settings
            _showSaveOptions = EditorGUILayout.Foldout(_showSaveOptions, "Saving settings");
            if (_showSaveOptions)
            {

                _superCombiner._saveMaterials = EditorGUILayout.Toggle("Save materials", _superCombiner._saveMaterials);
                _superCombiner._saveTextures = EditorGUILayout.Toggle("Save textures", _superCombiner._saveTextures);
                if (_superCombiner._combiningState == SuperCombiner.CombineStatesList.CombinedMaterials)
                {
                    GUI.enabled = false;
                }
                _superCombiner._savePrefabs = EditorGUILayout.Toggle("Save prefabs", _superCombiner._savePrefabs);
                _superCombiner._saveMeshObj = EditorGUILayout.Toggle("Save meshes as Obj", _superCombiner._saveMeshObj);
                GUI.enabled = true;


                //this.SuperCombiner._saveMeshFbx = EditorGUILayout.Toggle ("Save meshes as Fbx", this.SuperCombiner._saveMeshFbx);

                if (GUILayout.Button("Save in: " + _superCombiner._folderDestination + " ...", GUILayout.MinHeight(20)))
                {
                    //this.SuperCombiner._folderDestination = EditorUtility.OpenFolderPanel("Destination Directory", "", "");
                    string folderPath = EditorUtility.SaveFolderPanel("Destination Directory", "", "combined");
                    if (folderPath != null)
                    {
                        int startIndex = folderPath.IndexOf("Assets/");
                        string relativePath = "Assets/";
                        if (startIndex > 0)
                        {
                            relativePath = folderPath.Substring(startIndex);
                        }
                        else
                        {
                            Logger.Instance.AddLog("SuperCombiner", "Please, specify a folder under Assets/", Logger.LogLevel.LOG_ERROR);
                        }
                        _superCombiner._folderDestination = relativePath;
                    }
                }
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Save", GUILayout.MinHeight(30)))
            {
                _superCombiner.Save();
            }


            if (AssetDatabase.IsValidFolder(_superCombiner._folderDestination))
            {
                EditorGUILayout.Space();
                _showPackageOptions = EditorGUILayout.Foldout(_showPackageOptions, "Unity Package Options:");

                if (_showPackageOptions)
                {
                    _include_dependencies = EditorGUILayout.Toggle("Include Dependencies: ", _include_dependencies);
                }

                if (GUILayout.Button("Generate Unity Package", GUILayout.MinHeight(30)))
                {
                    if (_include_dependencies)
                        AssetDatabase.ExportPackage(_superCombiner._folderDestination, _superCombiner._sessionName + ".unitypackage", ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
                    else
                        AssetDatabase.ExportPackage(_superCombiner._folderDestination, _superCombiner._sessionName + ".unitypackage", ExportPackageOptions.Interactive | ExportPackageOptions.Recurse);
                }
            }
        }




        #endregion //CombinedResult


        #region Menus
        /// <summary>
        /// Launch combine command for all SuperCombiner in current scene
        /// </summary>
        [MenuItem("SuperCombiner/Combine All")]
        static void CombineAll()
        {
            SuperCombiner[] sc_list = FindObjectsOfType<SuperCombiner>();
            foreach (SuperCombiner sc in sc_list)
            {
                if (sc._combiningState == SuperCombiner.CombineStatesList.Uncombined)
                    sc.CombineChildren();
            }
        }


        /// <summary>
        /// Launch save command for all SuperCombiner in current scene
        /// </summary>
        [MenuItem("SuperCombiner/Save All")]
        static void SaveAll()
        {
            SuperCombiner[] sc_list = FindObjectsOfType<SuperCombiner>();
            foreach (SuperCombiner sc in sc_list)
            {
                if (sc._combiningState != SuperCombiner.CombineStatesList.Uncombined)
                    sc.Save();
            }
        }


        /// <summary>
        /// Launch uncombine command for all SuperCombiner in current scene
        /// </summary>
        [MenuItem("SuperCombiner/UnCombine All")]
        static void UnCombineAll()
        {
            SuperCombiner[] sc_list = FindObjectsOfType<SuperCombiner>();
            foreach (SuperCombiner sc in sc_list)
            {
                if (sc._combiningState != SuperCombiner.CombineStatesList.Uncombined)
                    sc.UnCombine();
            }
        }


        /// <summary>
        /// Launch combine command for each SuperCombiner seleted in editor
        /// </summary>
        [MenuItem("SuperCombiner/Combine selected")]
        static void CombineSelected()
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                SuperCombiner sc = obj.GetComponent<SuperCombiner>();
                if (sc != null)
                {
                    if (sc._combiningState == SuperCombiner.CombineStatesList.Uncombined)
                        sc.CombineChildren();
                }
            }
        }


        /// <summary>
        /// activativate "combine selected" item menu when objects with SuperCombiner component are selected 
        /// </summary>
        /// <returns><c>true</c>, if SuperCombiner components are selected, <c>false</c> otherwise.</returns>
        [MenuItem("SuperCombiner/Combine selected", true)]
        static bool ValidateCombineSelected()
        {
            bool valide = false;
            foreach (GameObject obj in Selection.gameObjects)
            {
                if (obj.GetComponent<SuperCombiner>() != null)
                {
                    valide = true;
                }
            }

            return valide;
        }


        /// <summary>
        /// Launch save command for each SuperCombiner seleted in editor
        /// </summary>
        [MenuItem("SuperCombiner/Save selected")]
        static void SaveSelected()
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                SuperCombiner sc = obj.GetComponent<SuperCombiner>();
                if (sc != null)
                {
                    if (sc._combiningState != SuperCombiner.CombineStatesList.Uncombined)
                        sc.Save();
                }
            }
        }

        /// <summary>
        /// activativate "save selected" item menu when objects with SuperCombiner component are selected 
        /// </summary>
        /// <returns><c>true</c>, if SuperCombiner components are selected, <c>false</c> otherwise.</returns>
        [MenuItem("SuperCombiner/Save selected", true)]
        static bool ValidateSaveSelected()
        {
            bool valide = false;
            foreach (GameObject obj in Selection.gameObjects)
            {
                if (obj.GetComponent<SuperCombiner>() != null)
                {
                    valide = true;
                }
            }

            return valide;
        }


        /// <summary>
        /// Launch uncombine command for each SuperCombiner seleted in editor
        /// </summary>
        [MenuItem("SuperCombiner/UnCombine selected")]
        static void UnCombineSelected()
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                SuperCombiner sc = obj.GetComponent<SuperCombiner>();
                if (sc != null)
                {
                    if (sc._combiningState != SuperCombiner.CombineStatesList.Uncombined)
                        sc.UnCombine();
                }
            }
        }

        /// <summary>
        /// activativate "uncombine selected" item menu when objects with SuperCombiner component are selected 
        /// </summary>
        /// <returns><c>true</c>, if SuperCombiner components are selected, <c>false</c> otherwise.</returns>
        [MenuItem("SuperCombiner/UnCombine selected", true)]
        static bool ValidateUnCombineSelected()
        {
            bool valide = false;
            foreach (GameObject obj in Selection.gameObjects)
            {
                if (obj.GetComponent<SuperCombiner>() != null)
                {
                    valide = true;
                }
            }

            return valide;
        }



        // Add a menu item called "Combine" to a superCombiner's context menu.
        /// <summary>
        /// Create contextual for Launch combine process
        /// </summary>
        /// <param name="command">Command.</param>
        [MenuItem("CONTEXT/SuperCombiner/Combine")]
        static void Combine(MenuCommand command)
        {
            Logger.Instance.AddLog("SuperCombiner", "Combine All...");

            SuperCombiner sc = (SuperCombiner)command.context;
            sc.CombineChildren();
        }


        /// <summary>
        /// Create contextual menu for uncombine result
        /// </summary>
        /// <param name="command">Command.</param>
        [MenuItem("CONTEXT/SuperCombiner/UnCombine")]
        static void UnCombine(MenuCommand command)
        {
            SuperCombiner sc = (SuperCombiner)command.context;
            sc.UnCombine();
        }


        /// <summary>
        /// Create contextual menu for save combine result
        /// </summary>
        /// <param name="command">Command.</param>
        [MenuItem("CONTEXT/SuperCombiner/Save")]
        static void Save(MenuCommand command)
        {
            SuperCombiner sc = (SuperCombiner)command.context;
            sc.Save();
        }


        /// <summary>
        /// Determines if we have combine result
        /// </summary>
        /// <returns><c>true</c> if is combined the specified command; otherwise, <c>false</c>.</returns>
        /// <param name="command">Command.</param>
        [MenuItem("CONTEXT/SuperCombiner/UnCombine", true)]
        [MenuItem("CONTEXT/SuperCombiner/Save", true)]
        static bool IsCombined(MenuCommand command)
        {
            SuperCombiner sc = (SuperCombiner)command.context;
            if (sc._combiningState == SuperCombiner.CombineStatesList.Uncombined)
                return false;
            else
                return true;
        }


        /// <summary>
        /// Determines if is uncombined the specified command.
        /// </summary>
        /// <returns><c>true</c> if is uncombined the specified command; otherwise, <c>false</c>.</returns>
        /// <param name="command">Command.</param>
        [MenuItem("CONTEXT/SuperCombiner/Combine", true)]
        static bool IsUnCombined(MenuCommand command)
        {
            SuperCombiner sc = (SuperCombiner)command.context;
            if (sc._combiningState == SuperCombiner.CombineStatesList.Uncombined)
                return true;
            else
                return false;
        }


        /// <summary>
        /// Add a menu item to create game object with a SuperCombiner component.
        /// Priority 1 ensures it is grouped with the other menu items of the same kind
        /// and propagated to the hierarchy dropdown and hierarch context menus.	
        /// </summary>
        /// <param name="menuCommand">Menu command.</param>
        [MenuItem("GameObject/SuperCombiner/SuperCombiner", false, 10)]
        static void CreateSuperCombinerGameObject(MenuCommand menuCommand)
        {
            // Create a empty game object
            GameObject go = new GameObject("SuperCombiner");
            //add supercombiner componant
            go.AddComponent<SuperCombiner>();
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        #endregion //Menus
    }
}
