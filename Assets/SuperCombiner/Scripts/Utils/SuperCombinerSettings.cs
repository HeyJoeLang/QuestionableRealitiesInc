using System;
using UnityEngine;
using System.Collections.Generic;

namespace LunarCatsStudio.SuperCombiner
{
    [Serializable]
    public class GeneralSettings
    {

        public const string DEFAULT_SESSION_NAME = "combinedSession";
        public const string DEFAULT_VERSION_NUMBER = "UNKNOWN";

        string _version_number = DEFAULT_VERSION_NUMBER;
        string _session_name = DEFAULT_SESSION_NAME;
        bool _combine_at_runtime = false;
        GameObject _target_game_object = null;

        public string versionNumber
        {
            get { return _version_number; }
            set
            {
                if (value.Length <= 0)
                    value = DEFAULT_VERSION_NUMBER;

                if (value != _version_number)
                    _version_number = value;
            }
        }

        public string sessionName
        {
            get { return _session_name; }
            set
            {
                if (value.Length <= 0)
                    value = DEFAULT_SESSION_NAME;

                if (value != _session_name)
                    _session_name = value;
            }
        }

        public bool combineAtRuntime
        {
            get { return _combine_at_runtime; }
            set
            {
                if (value != _combine_at_runtime)
                    _combine_at_runtime = value;
            }
        }


        public GameObject targetGameObject
        {
            get { return _target_game_object; }
            set
            {
                if (value != _target_game_object)
                    _target_game_object = value;
            }
        }

    }


    [Serializable]
    public class TextureSettings
    {
        [HideInInspector]
        public List<int> textureAtlasSizesValues = new List<int>() {
            32, 64, 128, 256, 512, 1024, 2048, 4096, 8192
        };
        public List<string> textureAtlasSizesLabels = new List<string>() {
            "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192"
        };

        public const float MIN_TILING_FACTOR = 1f;
        public const float MAX_TILING_FACTOR = 2f;


        private int _atlas_size = 1024;
        [Range(MIN_TILING_FACTOR, MAX_TILING_FACTOR)] private float _tiling_factor = 1f;
        private int _padding = 0;

        public int atlasSize
        {
            get { return _atlas_size; }
            set
            {
                if (value != _atlas_size && textureAtlasSizesValues.IndexOf(value) >= 0)
                    _atlas_size = value;
            }
        }

        public float tilingFactor
        {
            get { return _tiling_factor; }
            set
            {
                if (value != _tiling_factor)
                    _tiling_factor = value;
            }
        }


        public int padding
        {
            get { return _padding; }
            set
            {
                if (value != _padding)
                    _padding = value;
            }
        }
    }

    [Serializable]
    public class MaterialSettings
    {
        private bool _multipleMaterialsMode = false;

        private bool _combineEachGroupAsSubmesh = false;

        public List<Material> multiMaterials0 = new List<Material>();
        public List<Material> multiMaterials1 = new List<Material>();
        public List<Material> multiMaterials2 = new List<Material>();
        public List<Material> multiMaterials3 = new List<Material>();
        public List<Material> multiMaterials4 = new List<Material>();
        public List<Material> multiMaterials5 = new List<Material>();
        public List<Material> multiMaterials6 = new List<Material>();
        public List<Material> multiMaterials7 = new List<Material>();
        public List<Material> multiMaterials8 = new List<Material>();
        public List<Material> multiMaterials9 = new List<Material>();
        public List<Material> multiMaterials10 = new List<Material>();

        private int _multiple_materials_count = 0;
        public List<string> _customShaderProperties;

        // Getter/Setter
        public bool combineEachGroupAsSubmesh
        {
            get => _combineEachGroupAsSubmesh;
            set => _combineEachGroupAsSubmesh = value;
        }

        public int multipleMaterialsCount
        {
            get { return _multiple_materials_count; }
            set
            {
                if (value != _multiple_materials_count)
                    _multiple_materials_count = value;
            }
        }

        public List<string> customShaderProperties
        {
            get { return _customShaderProperties; }
            set
            {
                if (value != _customShaderProperties)
                    _customShaderProperties = value;
            }
        }

        public bool multipleMaterialsMode
        {
            get { return _multipleMaterialsMode; }
            set
            {
                if (value != _multipleMaterialsMode)
                    _multipleMaterialsMode = value;
            }
        }
    }


    [Serializable]
    public class MeshSettings
    {
        public const int MIN_MESHS = 0;
        public const int MAX_MESHS = 65534;

        public enum MeshOutputType
        {
            Mesh = 0,
            SkinnedMesh = 1
        };

        bool _manage_lods = false;
        int _managed_lod_level = 0;
        bool _combine_meshs = false;
        bool _generate_uv2 = true;
        bool _manageColliders = false;
        GameObject _targetGameObject = null;
        MeshOutputType _mesh_output = MeshOutputType.Mesh;

        public int managedLODLevel
        {
            get { return _managed_lod_level; }
            set
            {
                if (value != _managed_lod_level)
                    _managed_lod_level = value;
            }
        }

        public bool manageLODs
        {
            get { return _manage_lods; }
            set
            {
                if (value != _manage_lods)
                    _manage_lods = value;
            }
        }

        public bool manageColliders
        {
            get { return _manageColliders; }
            set
            {
                if (value != _manageColliders)
                    _manageColliders = value;
            }
        }

        public GameObject targetGameObject
        {
            get { return _targetGameObject; }
            set
            {
                if (value != _targetGameObject)
                    _targetGameObject = value;
            }
        }

        public bool combineMeshs
        {
            get { return _combine_meshs; }
            set
            {
                if (value != _combine_meshs)
                    _combine_meshs = value;
            }
        }

        public bool generateUv2
        {
            get { return _generate_uv2; }
            set
            {
                if (value != _generate_uv2)
                    _generate_uv2 = value;
            }
        }

        public MeshOutputType meshOutputType
        {
            get { return _mesh_output; }
            set
            {
                if (value != _mesh_output)
                    _mesh_output = value;
            }
        }
    }

    //[CreateAssetMenu(menuName = "SuperCombiner/New Settings", fileName = "New_SuperCombiner_Settings.asset")]
    public class SuperCombinerSettings : ScriptableObject
    {
        GeneralSettings _general_settings;
        TextureSettings _texture_settings;
        public MaterialSettings _material_settings; // Needs to be public so that SuperCOmbinerSettingsEditor can access the propertyField
        MeshSettings _mesh_settings;

        public SuperCombinerSettings()
        {
            _general_settings = new GeneralSettings();
            _texture_settings = new TextureSettings();
            _material_settings = new MaterialSettings();
            _mesh_settings = new MeshSettings();
        }

        public GeneralSettings generalSettings
        {
            get { return _general_settings; }
            set
            {
                if (value != _general_settings)
                    _general_settings = value;
            }
        }

        public TextureSettings textureSettings
        {
            get { return _texture_settings; }
            set
            {
                if (value != _texture_settings)
                    _texture_settings = value;
            }
        }

        public MaterialSettings materialSettings
        {
            get { return _material_settings; }
            set
            {
                if (value != _material_settings)
                    _material_settings = value;
            }
        }

        public MeshSettings meshSettings
        {
            get { return _mesh_settings; }
            set
            {
                if (value != _mesh_settings)
                    _mesh_settings = value;
            }
        }

    }
}
