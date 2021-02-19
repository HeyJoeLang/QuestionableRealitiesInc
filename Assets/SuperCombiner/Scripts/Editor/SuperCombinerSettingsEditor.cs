using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace LunarCatsStudio.SuperCombiner
{
    [CustomEditor(typeof(SuperCombinerSettings))]
    public class SuperCombinerSettingsEditor : Editor
    {
        SuperCombinerSettings _sc_settings;

        // flag used for show or hide sub-settings
        bool _show_instructions = true;
        bool _show_general_settings = true;
        bool _show_texture_settings = true;
        bool _show_material_settings = true;
        bool _show_mesh_settings = true;
        bool _showMultipleMaterials = false;

        Vector2 _pos_material_list;

        // Serialized
        private SerializedObject _serializedSettings;
        private SerializedProperty _customShaderProperties;
        private List<SerializedProperty> _multiMaterialsSC = new List<SerializedProperty>();

        public void OnEnable()
        {
            _sc_settings = (SuperCombinerSettings)target;

            _serializedSettings = new SerializedObject(_sc_settings);
            _customShaderProperties = _serializedSettings.FindProperty("_material_settings._customShaderProperties");

            for (int i = 0; i < SuperCombinerEditor.MAX_MULTI_MATERIAL_COUNT; i++)
            {
                _multiMaterialsSC.Add(_serializedSettings.FindProperty("_material_settings.multiMaterials" + i));
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI(); //utilisation de la methode de base
            GUI.enabled = false;
            DisplayHelp();
            EditorGUILayout.Space();

            DisplayGeneralSettings();
            EditorGUILayout.Space();

            DisplayTextureSettings();
            EditorGUILayout.Space();

            DisplayMaterialSettings();
            EditorGUILayout.Space();

            DisplayMeshSettings();
            EditorGUILayout.Space();
            GUI.enabled = true;

            EditorUtility.SetDirty(_sc_settings); // tag SC setting as modified and Save it
        }


        private void DisplayHelp()
        {
            _show_instructions = EditorGUILayout.Foldout(_show_instructions, "Instructions for Super Combiner (v " + _sc_settings.generalSettings.versionNumber + ")");

            if (_show_instructions)
            {
                GUILayout.Label("Put all you prefabs to combine as children of me. " +
                    "Select your session name, the texture atlas size and whether or not to combine meshes. " +
                    "When you are ready click 'Combine' button to start the process (it may take a while depending on the quantity of different assets). " +
                    "When the process is finished you'll see the result on the scene (all original mesh renderers will be deactivated). " +
                    "If you want to save the combined assets, select your saving options and click 'Save' button. " +
                    "To revert the process just click 'Uncombine' button.", EditorStyles.helpBox);
            }
        }

        private void DisplayGeneralSettings()
        {
            _show_general_settings = EditorGUILayout.Foldout(_show_general_settings, "General Settings:");

            if (_show_general_settings)
            {
                _sc_settings.generalSettings.sessionName = EditorGUILayout.TextField(new GUIContent("Session name", "Your session name should be different for every SuperCombiner instance. Avoid using special characters."), _sc_settings.generalSettings.sessionName, GUILayout.ExpandWidth(true));
                _sc_settings.generalSettings.combineAtRuntime = EditorGUILayout.Toggle(new GUIContent("Combine at runtime?", "Set to true if you want the process to combine at startup during runtime (beware that combining is a complex task that may takes some time to process)"), _sc_settings.generalSettings.combineAtRuntime);
                _sc_settings.generalSettings.targetGameObject = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Target GameObject", "The GameObject into which the combined GameObject(s) will be created. If you leave it empty, a new GameObject will be created under this GameObject with the name of you session name."), _sc_settings.generalSettings.targetGameObject, typeof(GameObject), true);
            }
        }

        private void DisplayTextureSettings()
        {
            _show_texture_settings = EditorGUILayout.Foldout(_show_texture_settings, "Texture Atlas Settings:");

            if (_show_texture_settings)
            {
                _sc_settings.textureSettings.atlasSize = EditorGUILayout.IntPopup("Texture Atlas size", _sc_settings.textureSettings.atlasSize, _sc_settings.textureSettings.textureAtlasSizesLabels.ToArray(), _sc_settings.textureSettings.textureAtlasSizesValues.ToArray(), GUILayout.ExpandWidth(true));
                _sc_settings.textureSettings.tilingFactor = EditorGUILayout.Slider(new GUIContent("tiling factor", "Apply a tiling factor on the textures. This may be helpfull if you observe strange artifacts after combining materials with heightmap"), _sc_settings.textureSettings.tilingFactor, TextureSettings.MIN_TILING_FACTOR, TextureSettings.MAX_TILING_FACTOR, GUILayout.ExpandWidth(true));
                _sc_settings.textureSettings.padding = EditorGUILayout.IntField(new GUIContent("padding", "Padding between textures in the atlas"), _sc_settings.textureSettings.padding, GUILayout.ExpandWidth(true));

            }
        }

        private void DisplayMaterialSettings()
        {
            _show_material_settings = EditorGUILayout.Foldout(_show_material_settings, "Materials and Shaders Settings:");

            if (_show_material_settings)
            {
                _sc_settings.materialSettings.multipleMaterialsMode = EditorGUILayout.Toggle(new GUIContent("Multiple materials", "The multi material feature lets you combine to several materials (up to 10) from the listed source materials. This is usually usefull when combining meshes that have various materials (submeshes) that cannot be combined together."), _sc_settings.materialSettings.multipleMaterialsMode);

                // Multiple Material
                if (_sc_settings.materialSettings.multipleMaterialsMode)
                {
                    _sc_settings.materialSettings.combineEachGroupAsSubmesh = EditorGUILayout.Toggle(new GUIContent("Set as submesh", "If set to true, each combined mesh for each material source group will be a submesh of the final combine mesh. If set to false, each material source group will be a separate mesh in a separate GameObject."), _sc_settings.materialSettings.combineEachGroupAsSubmesh);

                    _showMultipleMaterials = EditorGUILayout.Foldout(_showMultipleMaterials, "Materials group");
                    if (_showMultipleMaterials)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        for (int i = 0; i < _sc_settings.materialSettings.multipleMaterialsCount; i++)
                        {
                            EditorGUILayout.BeginHorizontal();

                            // Source materials
                            EditorGUILayout.PropertyField(_multiMaterialsSC[i], new GUIContent("source materials (group " + i + ")", "Define here all the source material to be included in combined material " + i), true);

                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();
                    }
                }

                // Custom Shader propertues
                EditorGUILayout.PropertyField(_customShaderProperties, new GUIContent("Custom shader properties", "Super Combiner uses the list of texture properties from standard shader. If you are using custom shader with different texture properties, add their exact name in the list."), true);
            }
        }

        private void DisplayMeshSettings()
        {
            _show_mesh_settings = EditorGUILayout.Foldout(_show_mesh_settings, "Meshs Settings");

            if (_show_mesh_settings)
            {
                _sc_settings.meshSettings.combineMeshs = EditorGUILayout.Toggle(new GUIContent("Combine meshes?", "If set to false, only materials and textures will be combined, all meshes will remain separated. If set to true, all meshes will be combined into a unique combined mesh."), _sc_settings.meshSettings.combineMeshs);
                if (_sc_settings.meshSettings.combineMeshs)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    _sc_settings.meshSettings.generateUv2 = EditorGUILayout.Toggle(new GUIContent("Generate UV2?", "If set to true, Super Combiner will generate UV2 for the combined mesh."), _sc_settings.meshSettings.generateUv2);
                    _sc_settings.meshSettings.meshOutputType = (MeshSettings.MeshOutputType)EditorGUILayout.EnumPopup(new GUIContent("Mesh output", "Chose to combine into a Mesh or a SkinnedMesh. Combining into SkinnedMesh is in alpha release, it will only works properly if there are only SkinnedMeshes as input. Combining Meshes and SkinnedMeshes into a SkinnedMesh is not supported yet."), _sc_settings.meshSettings.meshOutputType, GUILayout.ExpandWidth(true));
                    _sc_settings.meshSettings.manageColliders = EditorGUILayout.Toggle(new GUIContent("Include colliders", "If set to true, SuperCombiner will integrate all colliders into the combined GameObject"), _sc_settings.meshSettings.manageColliders);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.BeginHorizontal();
                _sc_settings.meshSettings.manageLODs = EditorGUILayout.Toggle(new GUIContent("Manage LOD level", "If set to true, SuperCombiner will only take into account the specified LOD level for each LODGroup in the list of GameObjects to combine."), _sc_settings.meshSettings.manageLODs);
                if (_sc_settings.meshSettings.manageLODs)
                {
                    _sc_settings.meshSettings.managedLODLevel = EditorGUILayout.IntField(new GUIContent("LOD level to take into account", "LOD Level to take into account"), _sc_settings.meshSettings.managedLODLevel, GUILayout.ExpandWidth(true));
                }
                EditorGUILayout.EndHorizontal();

                _sc_settings.meshSettings.targetGameObject = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Target GameObject", "The GameObject into which the combined GameObject(s) will be created. If you leave it empty, a new GameObject will be created under this GameObject with the name of you session name."), _sc_settings.meshSettings.targetGameObject, typeof(GameObject), true);

            }
        }
    }
}


