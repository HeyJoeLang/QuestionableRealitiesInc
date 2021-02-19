using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using LunarCatsStudio.SuperCombiner;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace LunarCatsStudio.SuperCombiner
{
    /// <summary>
    /// Main class of Super Combiner asset.
    /// </summary>
    public class SuperCombiner : MonoBehaviour
    {
        public string versionNumber { get { return "1.6.6"; } }

        public enum CombineStatesList
        {
            Uncombined,
            Combining,
            CombinedMaterials,
            Combined
        }


        public CombineStatesList _combiningState = CombineStatesList.Uncombined;
        public List<LunarCatsStudio.SuperCombiner.TexturePacker> _texturePackers = new List<LunarCatsStudio.SuperCombiner.TexturePacker>();
        public LunarCatsStudio.SuperCombiner.MeshCombiner _meshCombiner = new LunarCatsStudio.SuperCombiner.MeshCombiner();


        // Editable Parameters
        // General settings
        public string _sessionName = "combinedSession";
        public bool _combineAtRuntime = false;

        // Texture Atlas settings
        public int _textureAtlasSize = 2048;
        public List<string> _customTextureProperies = new List<string>();
        public float _tilingFactor = 1f;
        public int _atlasPadding = 0;
        public bool _combineMaterials = true;
        public bool _forceUVTo0_1 = false;

        // Multiple materials
        public bool _multipleMaterialsMode = false;

        public bool _combineEachGroupAsSubmesh = false;

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
        public List<Material> multiMaterialsAllOthers = new List<Material>();

        // Meshes settings
        public bool _combineMeshes = false;
        public bool _manageLodLevel = false;
        public int _managedLodLevel = 0;
        public bool _generateUv2 = true;
        public int _meshOutput;
        public bool _manageColliders = false;
        public GameObject _targetGameObject;

        /// <summary>
        /// The list of multi _material list defined by user
        /// </summary>
        public List<List<Material>> _multiMaterialsList = new List<List<Material>>();
        public int _multiMaterialsCount;
        /// <summary>
        /// The list of _material list to combine
        /// </summary>
        List<List<MaterialToCombine>> _materialsToCombine = new List<List<MaterialToCombine>>();

        // Saving options
        public bool _savePrefabs = true;
        public bool _saveMeshObj = false;
        public bool _saveMeshFbx = false;
        public bool _saveMaterials = true;
        public bool _saveTextures = true;
        public string _folderDestination = "Assets/SuperCombiner/Combined";

        // Internal combine process variables
        /// <summary>
        /// List of all original MeshRenderer in children to combine
        /// </summary>
        public List<RendererObject<MeshRenderer>> _meshList = new List<RendererObject<MeshRenderer>>();
        /// <summary>
		/// List of all original SkinnedMeshRenderer in children to combine
        /// </summary>
        public List<RendererObject<SkinnedMeshRenderer>> _skinnedMeshList = new List<RendererObject<SkinnedMeshRenderer>>();
        /// <summary>
        /// List of copied meshes instancesId associated with their original sharedMesh and sharedMaterial instanceId. 
        /// This is usefull not to save duplicated mesh when exporting
		/// Key=Mesh instanceID
		/// Value=Concatenation of sharedMeshName + sharedMaterialName + GameObjectName
        /// </summary>
        public Dictionary<int, string> _uniqueCombinedMeshId = new Dictionary<int, string>();
        /// <summary>
        /// Links original shared meshes with the copy created
		/// Key=original shared mesh instanceID
		/// Value=The value of _uniqueCombinedMeshId[Key]
        /// </summary>
		public Dictionary<int, string> _copyMeshId = new Dictionary<int, string>();
        /// <summary>
        /// List of transformed game objects for prefab saving
        /// </summary>
        public List<GameObject> _toSavePrefabList = new List<GameObject>();
        /// <summary>
        /// List of transformed game objects for saving purpose
        /// </summary>
        public List<MeshRenderer> _toSaveObjectList = new List<MeshRenderer>();
        /// <summary>
        /// List of meshes to save
        /// </summary>
        public List<Mesh> _toSaveMeshList = new List<Mesh>();
        /// <summary>
        /// List of transformed skinned game objects for saving purpose
        /// </summary>
        public List<SkinnedMeshRenderer> _toSaveSkinnedObjectList = new List<SkinnedMeshRenderer>();
        // <summary>
        // CombinedGameObjects[i] will use uvs[combinedTextureIndex[i]]
        // </summary>
        //public List<int> combinedTextureIndex = new List<int>();

        /// <summary>
        /// The parent GameObject for every combined object
        /// </summary>
        public GameObject _targetParentForCombinedGameObjects;

        private DateTime _timeStart;             // The date time when starting the process

        /// <summary>
        /// The result of the combine process is stored in this class. It is instanciated the first time combine process is executed
        /// </summary>
        public CombinedResult _combinedResult;

        /// <summary>
        /// input setting of supper combiner
        /// </summary>
        public SuperCombinerSettings _scSettings;

        private SuperCombiner()
        {
            // Nothing to do here
        }

        void Start()
        {
            if (_combineAtRuntime)
            {
                CombineChildren();
            }
        }

        /// <summary>
        /// Find and fill the list of enabled meshes to combine
        /// </summary>
        public void FindMeshesToCombine()
        {
            _meshList = FindEnabledMeshes(transform);
            _skinnedMeshList = FindEnabledSkinnedMeshes(transform);
        }

        /// <summary>
        /// Combine process
        /// </summary>
        public void CombineChildren()
        {
            Logger.Instance.ClearLogs();
            _timeStart = DateTime.Now;
            _combiningState = CombineStatesList.Combining;

            // Getting the list of meshes ...
            FindMeshesToCombine();

            Combine(_meshList, _skinnedMeshList);
        }

        /// <summary>
	    /// Combine Materials and Create Atlas texture
	    /// </summary>
	    /// <param name="meshesToCombine"></param>
	    /// <param name="skinnedMeshesToCombine"></param>
        /// <returns>True if process has been successfull</returns>
	    public bool CombineMaterials(List<RendererObject<MeshRenderer>> meshesToCombine, List<RendererObject<SkinnedMeshRenderer>> skinnedMeshesToCombine)
        {
#if UNITY_EDITOR
            // UI Progress bar display in Editor
            EditorUtility.DisplayProgressBar("Super Combiner", "Materials and textures listing...", 0.1f);
#endif
            // Initialize multi _material parameters
            InitializeMultipleMaterialElements();

            // If _combinedResult has not been created yet, create it
            if (_combinedResult == null)
            {
                _combinedResult = (CombinedResult)ScriptableObject.CreateInstance(typeof(CombinedResult));
            }

            // Getting list of materials
            List<MaterialToCombine> enabledMaterials = FindEnabledMaterials(meshesToCombine, skinnedMeshesToCombine);
            _combinedResult._materialCombinedCount = enabledMaterials.Count;

            foreach (MaterialToCombine mat in enabledMaterials)
            {
                bool found = false;
                for (int i = 0; i < _multiMaterialsList.Count; i++)
                {
                    if (_multiMaterialsList[i].Contains(mat._material))
                    {
                        // This _material was listed in the multi _material list by user, we add it to it's right _index in '_materialsToCombine' list
                        _materialsToCombine[i].Add(mat);
                        found = true;
                    }
                }
                if (!found)
                {
                    // This _material was not listed in the multi _material list, so we add it to the last element
                    _materialsToCombine[_materialsToCombine.Count - 1].Add(mat);
                }
            }

            //  List all texture from enabled materials to be combined
            int progressCount = 0;
            for (int i = 0; i < _materialsToCombine.Count; i++)
            {
                _combinedResult._originalMaterialList.Add(new Dictionary<int, MaterialToCombine>());
                _combinedResult.AddNewCombinedMaterial();
                if (i == _multiMaterialsList.Count && _materialsToCombine[i].Count > 0 || i < _multiMaterialsList.Count && _multiMaterialsList[i].Count > 0)
                {
                    // Instanciate a new texture Packer
                    TexturePacker texturePacker = new TexturePacker
                    {
                        // Assign the _combinedResult reference to texturePacker
                        CombinedResult = _combinedResult,
                        CombinedIndex = i
                    };

                    // Setting up the custom shader property _names
                    texturePacker.SetCustomPropertyNames(_customTextureProperies);
                    // Add this texture packer to the list
                    _texturePackers.Add(texturePacker);

                    foreach (MaterialToCombine mat in _materialsToCombine[i])
                    {
                        _combinedResult.AddMaterialToCombine(mat, i);
#if UNITY_EDITOR
                        // Cancelable  UI Progress bar display in Editor
                        bool cancel = false;
                        EditorUtility.DisplayProgressBar("Super Combiner", "Processing _material " + mat._material.name, progressCount / (float)enabledMaterials.Count);
                        if (cancel)
                        {
                            UnCombine();
                            return false;
                        }
#endif
                        /*Rect materialUVBoundToUse = mat.GetScaledAndOffsetedUVBounds();
                        if(_forceUVTo0_1)
                        {
                            materialUVBoundToUse = new Rect(0, 0, 1, 1);
                        }*/
                        // Add all textures from this material on the list of textures
                        texturePacker.SetTextures(mat._material, _combineMaterials, mat, _tilingFactor);

                        /*if (!mat.HasProperty ("_MainTex") || mat.mainTexture == null) {
                        // Correction of uv for mesh without diffuse texture
                            uvBound.size = Vector2.Scale (uvBound.size, new Vector2 (1.2f, 1.2f));
                            uvBound.position -= new Vector2 (0.1f, 0.1f);
                        }*/
                        progressCount++;
                    }

                    if (_materialsToCombine[i].Count == 0)
                    {
                        if (_multiMaterialsList[i].Count == 0)
                        {
                            Logger.Instance.AddLog("SuperCombiner", "Source materials group " + i + " is empty. Skipping this combine process", Logger.LogLevel.LOG_WARNING);
                        }
                        else
                        {
                            Logger.Instance.AddLog("SuperCombiner", "Cannot combined materials for group " + i + " because none of the _material were found in the list of game objects to combine", Logger.LogLevel.LOG_WARNING);
                        }
                    }
                    else if (_materialsToCombine[i].Count == 1)
                    {
                        if (_materialsToCombine.Count == 1)
                        {
                            Logger.Instance.AddLog("SuperCombiner", "Only one material found, skipping combine material process and keep this material (" + _materialsToCombine[i][0]._material.name + ") for the combined mesh.");
                        }
                        else
                        {
                            Logger.Instance.AddLog("SuperCombiner", "Only one material found for multi material group " + i + ", skipping combine material process and keep this material (" + _materialsToCombine[i][0]._material.name + ") for the combined mesh.");
                        }
                        _combinedResult.SetCombinedMaterial(_materialsToCombine[i][0]._material, i, true);
                        _combinedResult._combinedMaterials[i].uvs = new Rect[1];
                        _combinedResult._combinedMaterials[i].uvs[0] = new Rect(0, 0, 1, 1);
                        texturePacker.SetCopiedMaterial(_materialsToCombine[i][0]._material);
                    }
                    else
                    {
#if UNITY_EDITOR
                        // UI Progress bar display in Editor
                        EditorUtility.DisplayProgressBar("Super Combiner", "Packing textures...", 0f);
#endif
                        // Pack the textures
                        texturePacker.PackTextures(_textureAtlasSize, _atlasPadding, _combineMaterials, _sessionName);
                    }
                }
                else
                {
                    // There are no materials to combine in this _combinedIndex
                    _texturePackers.Add(null);
                }
            }

            _combiningState = CombineStatesList.CombinedMaterials;
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
            return false;
        }

        public void SetTargetParentForCombinedGameObject()
        {
            if (_targetGameObject == null)
            {
                // Create the parent Game object
                _targetParentForCombinedGameObjects = new GameObject(_sessionName);
                _targetParentForCombinedGameObjects.transform.parent = this.transform;
                _targetParentForCombinedGameObjects.transform.localPosition = Vector3.zero;
            }
            else
            {
                _targetParentForCombinedGameObjects = _targetGameObject;
            }
        }

        /// <summary>
        /// Combines the meshes
        /// </summary>
        /// <param name="meshesToCombine">Meshes to combine.</param>
        /// <param name="skinnedMeshesToCombine">Skinned meshes to combine.</param>
        public void CombineMeshes(List<RendererObject<MeshRenderer>> meshesToCombine, List<RendererObject<SkinnedMeshRenderer>> skinnedMeshesToCombine, Transform parent)
        {
            // Assign the _combinedResult reference to texturePacker and MeshCombiner
            _meshCombiner.CombinedResult = _combinedResult;

            _combinedResult._meshesCombinedCount = meshesToCombine.Count;
            _combinedResult._skinnedMeshesCombinedCount = skinnedMeshesToCombine.Count;

            // Check if there is at least 2 meshes in the current combine session
            if (_combineMeshes)
            {
                // Careful here we do not take into account renderers that will not be combined from the list
                if (meshesToCombine.Count + skinnedMeshesToCombine.Count < 1)
                {
                    if (meshesToCombine.Count == 0)
                    {
#if UNITY_EDITOR
                        EditorUtility.DisplayDialog("Super Combiner", "Zero meshes found.\nUnable to proceed without at least 1 mesh.", "Ok");
#endif
                        UnCombine();
                    }
                    return;
                }
            }

            // Parametrize MeshCombiner
            _meshCombiner.SetParameters(_sessionName, _generateUv2);

#if UNITY_EDITOR
            // UI Progress bar display in Editor
            EditorUtility.DisplayProgressBar("Super Combiner", "Combining meshes", 0.5f);
#endif

            // Combine process
            if (_combineMeshes)
            {
                // Get the ordered by _combinedIndex list of meshes to combine
                List<MeshRendererAndOriginalMaterials> meshIndexedList = GetMeshRenderersByCombineIndex(meshesToCombine, skinnedMeshesToCombine, _targetParentForCombinedGameObjects.transform);

                for (int i = 0; i < _combinedResult.GetCombinedIndexCount(); i++)
                {
                    _combinedResult._combinedGameObjectFromMeshList.Add(new List<GameObject>());
                    _combinedResult._combinedGameObjectFromSkinnedMeshList.Add(new List<GameObject>());

                    if (_combinedResult._originalMaterialList[i].Count > 0)
                    {
                        if ((LunarCatsStudio.SuperCombiner.MeshOutput)_meshOutput == LunarCatsStudio.SuperCombiner.MeshOutput.Mesh)
                        {
                            // Combine the meshes together
                            _combinedResult._combinedGameObjectFromMeshList[i] = _meshCombiner.CombineToMeshes(meshIndexedList[i]._meshRenderers, meshIndexedList[i]._skinnedMeshRenderers, parent, i);

                            // Add the copy mesh instanceId with its original sharedMesh and sharedMaterial instanceId
                            if (!_combineEachGroupAsSubmesh)
                            {
                                foreach (GameObject go in _combinedResult._combinedGameObjectFromMeshList[i])
                                {
                                    _uniqueCombinedMeshId.Add(go.GetComponent<MeshFilter>().sharedMesh.GetInstanceID(), go.name);
                                }
                            }
                        }
                        else
                        {
                            _combinedResult._combinedGameObjectFromSkinnedMeshList[i] = _meshCombiner.CombineToSkinnedMeshes(meshIndexedList[i]._meshRenderers, meshIndexedList[i]._skinnedMeshRenderers, parent, i);

                            if (!_combineEachGroupAsSubmesh)
                            {
                                // Add the copy mesh instanceId with its original sharedMesh and sharedMaterial instanceId
                                foreach (GameObject go in _combinedResult._combinedGameObjectFromSkinnedMeshList[i])
                                {
                                    _uniqueCombinedMeshId.Add(go.GetComponent<SkinnedMeshRenderer>().sharedMesh.GetInstanceID(), go.name);
                                }
                            }
                        }

                        if (_combinedResult._combinedGameObjectFromMeshList.Count + _combinedResult._combinedGameObjectFromSkinnedMeshList.Count == 0)
                        {
                            Logger.Instance.AddLog("SuperCombiner", "No mesh could be combined", Logger.LogLevel.LOG_ERROR);
                            // Error, Nothing could be combined
                            //UnCombine();
                            //return;
                        }
                    }
                    // Remove all temporary splitted GameObjects created
                    for (int j = 0; j < meshIndexedList[i]._splittedGameObject.Count; j++)
                    {
                        DestroyImmediate(meshIndexedList[i]._splittedGameObject[j]);
                    }
                }

                // Combine each material group to a submesh to a unique combined mesh
                if (_multipleMaterialsMode && _combineEachGroupAsSubmesh)
                {
                    List<Mesh> meshes = new List<Mesh>();

                    for (int i = 0; i < _combinedResult._combinedGameObjectFromMeshList.Count; i++)
                    {
                        if (_combinedResult._combinedGameObjectFromMeshList[i].Count > 0)
                        {
                            if ((LunarCatsStudio.SuperCombiner.MeshOutput)_meshOutput == LunarCatsStudio.SuperCombiner.MeshOutput.Mesh)
                            {
                                MeshFilter mf = _combinedResult._combinedGameObjectFromMeshList[i][0].GetComponent<MeshFilter>();
                                meshes.Add(mf.sharedMesh);

                                // Destroy the combined GameObject as we do not need it anymore
                                DestroyImmediate(_combinedResult._combinedGameObjectFromMeshList[i][0]);
                            }
                            else
                            {
                                SkinnedMeshRenderer mf = _combinedResult._combinedGameObjectFromSkinnedMeshList[i][0].GetComponent<SkinnedMeshRenderer>();
                                meshes.Add(mf.sharedMesh);

                                // Destroy the combined GameObject as we do not need it anymore
                                DestroyImmediate(_combinedResult._combinedGameObjectFromSkinnedMeshList[i][0]);
                            }
                        }
                    }

                    // Get the combined mesh with submeshes
                    GameObject go = _meshCombiner.CombineMeshToSubmeshes(meshes, (LunarCatsStudio.SuperCombiner.MeshOutput)_meshOutput);
                    go.transform.SetParent(parent);
                    go.transform.localPosition = Vector3.zero;


                    // Add the copy mesh instanceId with its original sharedMesh and sharedMaterial instanceId
                    if ((LunarCatsStudio.SuperCombiner.MeshOutput)_meshOutput == LunarCatsStudio.SuperCombiner.MeshOutput.Mesh)
                    {
                        _combinedResult._combinedGameObjectFromMeshList[0][0] = go;
                        _uniqueCombinedMeshId.Add(go.GetComponent<MeshFilter>().sharedMesh.GetInstanceID(), go.name);
                    }
                    else
                    {
                        _combinedResult._combinedGameObjectFromSkinnedMeshList[0][0] = go;
                        _uniqueCombinedMeshId.Add(go.GetComponent<SkinnedMeshRenderer>().sharedMesh.GetInstanceID(), go.name);
                    }
                }

                // Manage Colliders if needed
                if (_manageColliders)
                {
                    // Create new parent GameObject for all colliders
                    GameObject collidersParent = new GameObject("colliders");
                    collidersParent.transform.parent = parent;

                    Collider[] colliders = transform.GetComponentsInChildren<Collider>();
                    foreach (Collider collider in colliders)
                    {
                        if (collider != null && collider.enabled)
                        {
                            CollidersHandler.CreateNewCollider(collidersParent.transform, collider);
                        }
                    }
                }
            }
            else
            {
                // Create a copy of all game objects children of this one
                CopyGameObjectsHierarchy(parent);

                List<RendererObject<MeshRenderer>> copyMeshList = FindEnabledMeshes(parent);
                List<RendererObject<SkinnedMeshRenderer>> copySkinnedMeshList = FindEnabledSkinnedMeshes(parent);

                // Get the ordered by _combinedIndex list of meshes to combine
                List<MeshRendererAndOriginalMaterials> copyMeshIndexedList = GetMeshRenderersByCombineIndex(copyMeshList, copySkinnedMeshList, null);

                for (int i = 0; i < _combinedResult.GetCombinedIndexCount(); i++)
                {
                    _combinedResult._combinedGameObjectFromMeshList.Add(new List<GameObject>());
                    _combinedResult._combinedGameObjectFromSkinnedMeshList.Add(new List<GameObject>());

                    // Generate the new GameObjects and assign combined materials to renderers
                    if (copyMeshIndexedList[i]._meshRenderers.Count > 0)
                    {
                        _combinedResult._combinedGameObjectFromMeshList[i].AddRange(GenerateTransformedGameObjects(parent, copyMeshIndexedList[i]._meshRenderers));
                    }
                    if (copyMeshIndexedList[i]._skinnedMeshRenderers.Count > 0)
                    {
                        _combinedResult._combinedGameObjectFromSkinnedMeshList[i].AddRange(GenerateTransformedGameObjects(parent, copyMeshIndexedList[i]._skinnedMeshRenderers));
                    }

                    // Generate new UVs only if there are more than 1 _material combined
                    if (_combinedResult._originalMaterialList[i].Count > 1)
                    {
                        for (int j = 0; j < copyMeshIndexedList[i]._meshRenderers.Count; j++)
                        {
                            GenerateUVs(copyMeshIndexedList[i]._meshRenderers[j].GetComponent<MeshFilter>().sharedMesh, copyMeshIndexedList[i]._originalMaterials[j], copyMeshIndexedList[i]._meshRenderers[j].name, i);
                        }
                        for (int j = 0; j < copyMeshIndexedList[i]._skinnedMeshRenderers.Count; j++)
                        {
                            GenerateUVs(copyMeshIndexedList[i]._skinnedMeshRenderers[j].sharedMesh, copyMeshIndexedList[i]._originalskinnedMeshMaterials[j], copyMeshIndexedList[i]._skinnedMeshRenderers[j].name, i);
                        }
                    }
                }
            }

            _combiningState = CombineStatesList.Combined;
            // Deactivate original renderers
            DisableRenderers(_meshList, _skinnedMeshList);

#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
        }

        /// <summary>
        /// Return the list of MeshRendererAndOriginalMaterials for a given list of MeshRenderer and SkinnedMeshRenderer to combine.
        /// The returned list also contains the list of splitted submeshes if this was necessary
        /// </summary>
        /// <param name="meshRenderers"></param>
        /// <param name="skinnedMeshRenderers"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private List<MeshRendererAndOriginalMaterials> GetMeshRenderersByCombineIndex(List<RendererObject<MeshRenderer>> meshRenderers, List<RendererObject<SkinnedMeshRenderer>> skinnedMeshRenderers, Transform parent)
        {
            // The list to be returned
            List<MeshRendererAndOriginalMaterials> meshRenderersByCombineIndex = new List<MeshRendererAndOriginalMaterials>();
            // A temporary list of list of submeshes indexes to be splitted
            List<List<int>> submeshToCombinedIndex = new List<List<int>>();

            if (_combinedResult._originalMaterialList.Count == 0)
            {
                Logger.Instance.AddLog("SuperCombiner", "List of materials to combine has been lost. Try to uncombine and combine again.", Logger.LogLevel.LOG_ERROR);
                return meshRenderersByCombineIndex;
            }

            // Initialize lists
            for (int i = 0; i < _combinedResult._originalMaterialList.Count; i++)
            {
                meshRenderersByCombineIndex.Add(new MeshRendererAndOriginalMaterials());
                submeshToCombinedIndex.Add(new List<int>());
            }

            foreach (RendererObject<MeshRenderer> meshRenderer in meshRenderers)
            {
                if (meshRenderer.WillBeCombined)
                {
                    Material[] materials = meshRenderer.Renderer.sharedMaterials;
                    // We assume here the number of sharedMaterials is equal to the number of submeshes
                    _combinedResult._subMeshCount += materials.Length - 1;
                    // List all _combinedIndex for each _material in this meshRenderer
                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (materials[i] != null)
                        {
                            int index = _combinedResult.GetCombinedIndex(materials[i]);
                            submeshToCombinedIndex[index].Add(i);
                        }
                        else
                        {
                            Logger.Instance.AddLog("SuperCombiner", "MeshRenderer of '" + meshRenderer.Renderer.name + "' has some missing _material references.", Logger.LogLevel.LOG_WARNING);
                        }
                    }
                    // If needed, split the submeshes
                    bool hasSplitSubmeshes = false;
                    for (int i = 0; i < _combinedResult._originalMaterialList.Count; i++)
                    {
                        if (submeshToCombinedIndex[i].Count > 0)
                        {
                            if (submeshToCombinedIndex[i].Count < materials.Length)
                            {
                                // Some materials in this meshRenderer correspond to different combined _index, split submesh accordingly
                                MeshRenderer newMesh = SubmeshSplitter.SplitSubmeshes(meshRenderer.Renderer.GetComponent<MeshFilter>(), submeshToCombinedIndex[i].ToArray(), i);
                                meshRenderersByCombineIndex[i]._meshRenderers.Add(newMesh);
                                meshRenderersByCombineIndex[i]._originalMaterials.Add(newMesh.sharedMaterials);
                                meshRenderersByCombineIndex[i]._splittedGameObject.Add(newMesh.gameObject);
                                Logger.Instance.AddLog("SuperCombiner", "Splitting submeshes for " + meshRenderer, Logger.LogLevel.LOG_DEBUG, false);
                                hasSplitSubmeshes = true;
                            }
                            else
                            {
                                // All materials in this meshRenderer correspond to the same combined _index, no need to split submesh
                                meshRenderersByCombineIndex[i]._meshRenderers.Add(meshRenderer.Renderer);
                                meshRenderersByCombineIndex[i]._originalMaterials.Add(meshRenderer.Renderer.sharedMaterials);
                            }
                        }
                    }
                    // If mesh has been splitted we don't combine mesh, destroy the old meshRenderer and MeshFilter component because there are copies that won't be used anymore
                    if (hasSplitSubmeshes && parent == null)
                    {
                        DestroyImmediate(meshRenderer.Renderer.GetComponent<MeshFilter>());
                        DestroyImmediate(meshRenderer.Renderer);
                    }
                    // Clear the combined _index list
                    for (int i = 0; i < _combinedResult._originalMaterialList.Count; i++)
                    {
                        submeshToCombinedIndex[i].Clear();
                    }
                }
            }

            foreach (RendererObject<SkinnedMeshRenderer> skinnedMeshRenderer in skinnedMeshRenderers)
            {
                if (skinnedMeshRenderer.WillBeCombined)
                {
                    Material[] materials = skinnedMeshRenderer.Renderer.sharedMaterials;
                    _combinedResult._subMeshCount += materials.Length - 1;
                    // List all _combinedIndex for each _material in this meshRenderer
                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (materials[i] != null)
                        {
                            int index = _combinedResult.GetCombinedIndex(materials[i]);
                            submeshToCombinedIndex[index].Add(i);
                        }
                        else
                        {
                            Logger.Instance.AddLog("SuperCombiner", "SkinnedMeshRenderer of '" + skinnedMeshRenderer.Renderer.name + "' has some missing _material references.", Logger.LogLevel.LOG_WARNING);
                        }
                    }
                    // If needed, split the submeshes
                    bool hasSplitSubmeshes = false;
                    for (int i = 0; i < _combinedResult._originalMaterialList.Count; i++)
                    {
                        if (submeshToCombinedIndex[i].Count > 0)
                        {
                            if (submeshToCombinedIndex[i].Count < materials.Length)
                            {
                                // Some materials in this meshRenderer correspond to different combined _index, split submesh accordingly
                                SkinnedMeshRenderer newMesh = SubmeshSplitter.SplitSubmeshes(skinnedMeshRenderer.Renderer, submeshToCombinedIndex[i].ToArray(), i);
                                meshRenderersByCombineIndex[i]._skinnedMeshRenderers.Add(newMesh);
                                meshRenderersByCombineIndex[i]._originalskinnedMeshMaterials.Add(newMesh.sharedMaterials);
                                meshRenderersByCombineIndex[i]._splittedGameObject.Add(newMesh.gameObject);
                                Logger.Instance.AddLog("SuperCombiner", "Splitting submeshes for " + skinnedMeshRenderer, Logger.LogLevel.LOG_DEBUG, false);
                                hasSplitSubmeshes = true;
                            }
                            else
                            {
                                // All materials in this meshRenderer correspond to the same combined _index, no need to split submesh
                                meshRenderersByCombineIndex[i]._skinnedMeshRenderers.Add(skinnedMeshRenderer.Renderer);
                                meshRenderersByCombineIndex[i]._originalskinnedMeshMaterials.Add(skinnedMeshRenderer.Renderer.sharedMaterials);
                            }
                        }
                    }
                    // If mesh has been splitted we don't combine mesh, destroy the old meshRenderer and MeshFilter component because there are copies that won't be used anymore
                    if (hasSplitSubmeshes && parent == null)
                    {
                        DestroyImmediate(skinnedMeshRenderer.Renderer.GetComponent<MeshFilter>());
                        DestroyImmediate(skinnedMeshRenderer.Renderer);
                    }
                    // Clear the combined _index list
                    for (int i = 0; i < _combinedResult._originalMaterialList.Count; i++)
                    {
                        submeshToCombinedIndex[i].Clear();
                    }
                }
            }

            return meshRenderersByCombineIndex;
        }

        public void Combine(List<MeshRenderer> meshesToCombine, List<SkinnedMeshRenderer> skinnedMeshesToCombine)
        {
            List<RendererObject<MeshRenderer>> rendererObjectsToCombine = new List<RendererObject<MeshRenderer>>();
            List<RendererObject<SkinnedMeshRenderer>> skinnedRendererObjectsToCombine = new List<RendererObject<SkinnedMeshRenderer>>();
            foreach (MeshRenderer mr in meshesToCombine)
            {
                rendererObjectsToCombine.Add(new RendererObject<MeshRenderer>(mr));
            }
            foreach (SkinnedMeshRenderer mr in skinnedMeshesToCombine)
            {
                skinnedRendererObjectsToCombine.Add(new RendererObject<SkinnedMeshRenderer>(mr));
            }

            Combine(rendererObjectsToCombine, skinnedRendererObjectsToCombine);
        }

        /// <summary>
        /// Combine the specified MeshRenderers and SkinnedMeshRenderers
        /// </summary>
        /// <param name="meshesToCombine">Meshes to combine.</param>
        /// <param name="skinnedMeshesToCombine">Skinned meshes to combine.</param>
        public void Combine(List<RendererObject<MeshRenderer>> meshesToCombine, List<RendererObject<SkinnedMeshRenderer>> skinnedMeshesToCombine)
        {
            // Start timer if necessary
            if (_combiningState == CombineStatesList.Uncombined)
            {
                _timeStart = DateTime.Now;
                _combiningState = CombineStatesList.Combining;
            }

            Logger.Instance.AddLog("SuperCombiner", "Start processing...");

#if UNITY_EDITOR
            // UI Progress bar display in Editor
            EditorUtility.DisplayProgressBar("Super Combiner", "Meshes listing...", 0.1f);
#endif

            // Combine Materials
            bool cancel = CombineMaterials(meshesToCombine, skinnedMeshesToCombine);
            if (cancel)
            {
#if UNITY_EDITOR
                EditorUtility.ClearProgressBar();
#endif
                return;
            }

            // Initialte target parent gameObject
            SetTargetParentForCombinedGameObject();

            // Combine Meshes
            CombineMeshes(meshesToCombine, skinnedMeshesToCombine, _targetParentForCombinedGameObjects.transform);

#if UNITY_EDITOR
            // Combine process is finished
            EditorUtility.ClearProgressBar();
#endif

            // Process is finished
            _combiningState = CombineStatesList.Combined;
            _combinedResult._duration = DateTime.Now - _timeStart;
            Logger.Instance.AddLog("SuperCombiner", "Successfully combined game objects!\nExecution time is " + _combinedResult._duration);
        }

        /// <summary>
        /// Initialize multiple _material elements
        /// </summary>
        public void InitializeMultipleMaterialElements()
        {
            if (_multipleMaterialsMode)
            {
                _multiMaterialsList.Add(multiMaterials0);
                _multiMaterialsList.Add(multiMaterials1);
                _multiMaterialsList.Add(multiMaterials2);
                _multiMaterialsList.Add(multiMaterials3);
                _multiMaterialsList.Add(multiMaterials4);
                _multiMaterialsList.Add(multiMaterials5);
                _multiMaterialsList.Add(multiMaterials6);
                _multiMaterialsList.Add(multiMaterials7);
                _multiMaterialsList.Add(multiMaterials8);
                _multiMaterialsList.Add(multiMaterials9);
                _multiMaterialsList.Add(multiMaterials10);
            }
            // Fill the materials to combine list
            for (int i = 0; i < _multiMaterialsList.Count + 1; i++)
            {
                // The last one in this list correspond to all other materials
                _materialsToCombine.Add(new List<MaterialToCombine>());
            }
        }

        /// <summary>
        /// Copy all GameObjects children
        /// </summary>
        /// <param name="parent"></param>
        private void CopyGameObjectsHierarchy(Transform parent)
        {
            Transform[] children = this.transform.GetComponentsInChildren<Transform>();

            foreach (Transform child in children)
            {
                if (child.parent == this.transform && child != parent)
                {
                    GameObject go = InstantiateCopy(child.gameObject, false);
                    go.transform.SetParent(parent);
                }
            }
        }

        /// <summary>
        /// Generate the new uvs of the mesh in texture atlas
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="originalMaterials"></param>
        /// <param name="objectName"></param>
        /// <param name="combinedIndex"></param>
        private void GenerateUVs(Mesh mesh, Material[] originalMaterials, string objectName, int combinedIndex)
        {
            int[] textureIndexes = new int[originalMaterials.Length];

            for (int j = 0; j < originalMaterials.Length; j++)
            {
                Material mat = originalMaterials[j];
                textureIndexes[j] = _combinedResult.FindCorrespondingMaterialIndex(mat, combinedIndex);
            }

            if (!_meshCombiner.GenerateUV(mesh, textureIndexes, _combinedResult._combinedMaterials[combinedIndex].scaleFactors.ToArray(), objectName, combinedIndex))
            {
                UnCombine();
                return;
            }
        }

        /// <summary>
        /// Reactivate original GameObjects
        /// </summary>
        /// <param name="meshes"></param>
        /// <param name="skinnedMeshes"></param>
        private void EnableRenderers(List<RendererObject<MeshRenderer>> meshes, List<RendererObject<SkinnedMeshRenderer>> skinnedMeshes)
        {
            foreach (RendererObject<MeshRenderer> go in meshes)
            {
                if (go != null)
                {
                    go.Renderer.gameObject.SetActive(true);
                }
            }
            foreach (RendererObject<SkinnedMeshRenderer> go in skinnedMeshes)
            {
                if (go != null)
                {
                    go.Renderer.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Deactivate original GameObjects
        /// </summary>
        /// <param name="meshes"></param>
        /// <param name="skinnedMeshes"></param>
        private void DisableRenderers(List<RendererObject<MeshRenderer>> meshes, List<RendererObject<SkinnedMeshRenderer>> skinnedMeshes)
        {
            foreach (RendererObject<MeshRenderer> go in meshes)
            {
                if (go != null && go.Renderer.gameObject != _targetGameObject)
                {
                    go.Renderer.gameObject.SetActive(false);
                }
            }
            foreach (RendererObject<SkinnedMeshRenderer> go in skinnedMeshes)
            {
                if (go != null && go.Renderer.gameObject != _targetGameObject)
                {
                    go.Renderer.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Generate the new transformed gameobjects and apply new materials to them, when no combining meshes
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="originalMeshRenderer"></param>
        /// <returns></returns>
        private List<GameObject> GenerateTransformedGameObjects(Transform parent, List<MeshRenderer> originalMeshRenderer)
        {
            List<GameObject> copyList = new List<GameObject>();

            for (int i = 0; i < originalMeshRenderer.Count; i++)
            {
                // Copy the new mesh to the created GameObject copy
                Mesh copyOfMesh = _meshCombiner.copyMesh(originalMeshRenderer[i].GetComponent<MeshFilter>().sharedMesh);

                // Add the copy mesh instanceId with its original sharedMesh and sharedMaterial instanceId
                if (originalMeshRenderer[i].GetComponent<Renderer>().sharedMaterial != null)
                {
                    _uniqueCombinedMeshId.Add(copyOfMesh.GetInstanceID(), originalMeshRenderer[i].GetComponent<MeshFilter>().sharedMesh.GetInstanceID().ToString() + originalMeshRenderer[i].GetComponent<Renderer>().sharedMaterial.GetInstanceID().ToString() + copyOfMesh.name);
                }
                else
                {
                    _uniqueCombinedMeshId.Add(copyOfMesh.GetInstanceID(), originalMeshRenderer[i].GetComponent<MeshFilter>().sharedMesh.GetInstanceID().ToString() + copyOfMesh.name);
                }
                _copyMeshId[originalMeshRenderer[i].GetComponent<MeshFilter>().sharedMesh.GetInstanceID()] = _uniqueCombinedMeshId[copyOfMesh.GetInstanceID()];
                originalMeshRenderer[i].GetComponent<MeshFilter>().sharedMesh = copyOfMesh;

#if UNITY_EDITOR
                // Unwrap UV2 for lightmap
                Unwrapping.GenerateSecondaryUVSet(originalMeshRenderer[i].GetComponent<MeshFilter>().sharedMesh);
#endif

                // Assign new materials
                if (_combineMaterials)
                {
                    Material[] originalMaterials = originalMeshRenderer[i].GetComponent<Renderer>().sharedMaterials;
                    Material[] newMats = new Material[originalMaterials.Length];
                    for (int k = 0; k < newMats.Length; k++)
                    {
                        newMats[k] = _combinedResult.GetCombinedMaterial(originalMaterials[k]);
                    }
                    originalMeshRenderer[i].GetComponent<Renderer>().sharedMaterials = newMats;
                }
                else
                {
                    // If materials are not combined
                    /*Material[] mat = objects [i].GetComponent<Renderer> ().sharedMaterials;
                    Material[] newMats = new Material[mat.Length];
                    for (int a = 0; a < mat.Length; a++) {
                        newMats [a] = _texturePackers[0].getTransformedMaterialValue (objects [i].GetComponent<Renderer> ().sharedMaterials [a].name);
                        // Find corresponding _material
                        combinedTextureIndex.Add (_combinedResult.FindCorrespondingMaterialIndex(mat[a], 0));
                    }
                    objects[i].GetComponent<Renderer> ().sharedMaterials = newMats;*/
                }

                copyList.Add(originalMeshRenderer[i].gameObject);
            }

            return copyList;
        }

        /// <summary>
        /// Generate the new transformed gameobjects and apply new materials to them, when no combining meshes
        /// For Skinned Mesh renderers, when no combining meshes
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="originalSkinnedMeshRenderer"></param>
        /// <returns></returns>
        private List<GameObject> GenerateTransformedGameObjects(Transform parent, List<SkinnedMeshRenderer> originalSkinnedMeshRenderer)
        {
            List<GameObject> copyList = new List<GameObject>();

            for (int i = 0; i < originalSkinnedMeshRenderer.Count; i++)
            {
                // Copy the new mesh to the created GameObject copy
                Mesh copyOfMesh = _meshCombiner.copyMesh(originalSkinnedMeshRenderer[i].GetComponent<SkinnedMeshRenderer>().sharedMesh);

                // Add the copy mesh instanceId with its original sharedMesh and sharedMaterial instanceId
                if (originalSkinnedMeshRenderer[i].GetComponent<Renderer>().sharedMaterial != null)
                {
                    _uniqueCombinedMeshId.Add(copyOfMesh.GetInstanceID(), originalSkinnedMeshRenderer[i].GetComponent<SkinnedMeshRenderer>().sharedMesh.GetInstanceID().ToString() + originalSkinnedMeshRenderer[i].GetComponent<Renderer>().sharedMaterial.GetInstanceID().ToString() + copyOfMesh.name);
                }
                else
                {
                    _uniqueCombinedMeshId.Add(copyOfMesh.GetInstanceID(), originalSkinnedMeshRenderer[i].GetComponent<SkinnedMeshRenderer>().sharedMesh.GetInstanceID().ToString() + copyOfMesh.name);
                }
                _copyMeshId[originalSkinnedMeshRenderer[i].GetComponent<SkinnedMeshRenderer>().sharedMesh.GetInstanceID()] = _uniqueCombinedMeshId[copyOfMesh.GetInstanceID()];
                originalSkinnedMeshRenderer[i].GetComponent<SkinnedMeshRenderer>().sharedMesh = copyOfMesh;

#if UNITY_EDITOR
                // Unwrap UV2 for lightmap
                //Unwrapping.GenerateSecondaryUVSet(skinnedObjects[i].GetComponent<SkinnedMeshRenderer>().sharedMesh);
#endif

                // Assign new materials
                if (_combineMaterials)
                {
                    Material[] originalMaterials = originalSkinnedMeshRenderer[i].GetComponent<Renderer>().sharedMaterials;
                    Material[] newMats = new Material[originalMaterials.Length];
                    for (int k = 0; k < newMats.Length; k++)
                    {
                        newMats[k] = _combinedResult.GetCombinedMaterial(originalMaterials[k]);
                    }
                    originalSkinnedMeshRenderer[i].GetComponent<SkinnedMeshRenderer>().sharedMaterials = newMats;
                }
                else
                {
                    // If materials are not combined
                    /*Material[] mat = skinnedObjects [i].sharedMaterials;
                    Material[] newMats = new Material[mat.Length];
                    for (int a = 0; a < mat.Length; a++) {
                        newMats [a] = _texturePackers[0].getTransformedMaterialValue (skinnedObjects [i].sharedMaterials [a].name);
                        // Find corresponding _material
                        combinedTextureIndex.Add (_combinedResult.FindCorrespondingMaterialIndex(mat[a], 0));
                    }
                    skinnedObjects[i].GetComponent<SkinnedMeshRenderer> ().sharedMaterials = newMats;*/
                }

                copyList.Add(originalSkinnedMeshRenderer[i].gameObject);
            }

            return copyList;
        }

        // Instantiate a copy of the GameObject, keeping it's transform values identical
        private GameObject InstantiateCopy(GameObject original, bool deleteChidren = true)
        {
            GameObject copy = Instantiate(original) as GameObject;
            copy.transform.parent = original.transform.parent;
            copy.transform.localPosition = original.transform.localPosition;
            copy.transform.localRotation = original.transform.localRotation;
            copy.transform.localScale = original.transform.localScale;
            copy.name = original.name;

            if (deleteChidren)
            {
                // Delete all children
                foreach (Transform child in copy.transform)
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            return copy;
        }

        // Find all enabled mesh colliders
        private List<MeshCollider> FindEnabledMeshColliders(Transform parent)
        {
            MeshCollider[] colliders;
            colliders = parent.GetComponentsInChildren<MeshCollider>();

            List<MeshCollider> meshColliders = new List<MeshCollider>();
            foreach (MeshCollider collider in colliders)
            {
                if (collider.sharedMesh != null)
                {
                    meshColliders.Add(collider);
                }
            }

            return meshColliders;
        }

        // Find and store all enabled meshes
        private List<RendererObject<MeshRenderer>> FindEnabledMeshes(Transform parent)
        {
            MeshFilter[] filters;
            LODGroup[] lodGroups;
            Dictionary<MeshRenderer, RendererObject<MeshRenderer>> meshToRendererObject = new Dictionary<MeshRenderer, RendererObject<MeshRenderer>>();
            filters = parent.GetComponentsInChildren<MeshFilter>();

            List<RendererObject<MeshRenderer>> meshRendererList = new List<RendererObject<MeshRenderer>>();

            // Get all valid meshFilter
            foreach (MeshFilter filter in filters)
            {
                if (filter.sharedMesh != null)
                {
                    MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
                    if (renderer != null && renderer.enabled && renderer.sharedMaterials.Length > 0)
                    {
                        RendererObject<MeshRenderer> rendererObject = new RendererObject<MeshRenderer>(renderer);
                        meshToRendererObject.Add(renderer, rendererObject);
                        meshRendererList.Add(rendererObject);
                    }
                }
            }

            // Remove all non-desired Lods level
            if (_manageLodLevel)
            {
                lodGroups = parent.GetComponentsInChildren<LODGroup>();
                foreach (LODGroup lodGroup in lodGroups)
                {
                    LOD[] lods = lodGroup.GetLODs();
                    for (int i = 0; i < lods.Length; i++)
                    {
                        if (i != _managedLodLevel)
                        {
                            Renderer[] renderers = lods[i].renderers;
                            foreach (Renderer rd in renderers)
                            {
                                MeshRenderer meshrd = rd.GetComponent<MeshRenderer>();
                                if (meshrd != null)
                                {
                                    meshToRendererObject[meshrd].WillBeCombined = false;
                                }
                            }
                        }
                    }
                    if (_managedLodLevel > lods.Length)
                    {
                        Logger.Instance.AddLog("SuperCombiner", "Selected lod level " + _managedLodLevel + " is higher than LODs available in " + lodGroup.name, Logger.LogLevel.LOG_WARNING);
                    }
                }
            }

            return meshRendererList;
        }

        // Find and store all enabled skin meshes
        private List<RendererObject<SkinnedMeshRenderer>> FindEnabledSkinnedMeshes(Transform parent)
        {
            // Skinned meshes
            SkinnedMeshRenderer[] skinnedMeshes = parent.GetComponentsInChildren<SkinnedMeshRenderer>();

            List<RendererObject<SkinnedMeshRenderer>> skinnedMeshRendererList = new List<RendererObject<SkinnedMeshRenderer>>();

            foreach (SkinnedMeshRenderer skin in skinnedMeshes)
            {
                if (skin.sharedMesh != null)
                {
                    if (skin.enabled && skin.sharedMaterials.Length > 0)
                    {
                        RendererObject<SkinnedMeshRenderer> rendererObject = new RendererObject<SkinnedMeshRenderer>(skin);
                        skinnedMeshRendererList.Add(rendererObject);
                    }
                }
            }

            return skinnedMeshRendererList;
        }

        /// <summary>
        /// Find and return all enabled materials in given meshes and skinnedMeshes
        /// </summary>
        /// <param name="meshes"></param>
        /// <param name="skinnedMeshes"></param>
        /// <returns></returns>
        private List<MaterialToCombine> FindEnabledMaterials(List<RendererObject<MeshRenderer>> meshes, List<RendererObject<SkinnedMeshRenderer>> skinnedMeshes)
        {
            // List of materials linked with their instanceID
            Dictionary<int, MaterialToCombine> matList = new Dictionary<int, MaterialToCombine>();

            // Meshes renderer
            foreach (RendererObject<MeshRenderer> mesh in meshes)
            {
                Mesh sharedMesh = mesh.Renderer.GetComponent<MeshFilter>().sharedMesh;
                Rect uvBound = getUVBounds(sharedMesh.uv);

                foreach (Material material in mesh.Renderer.sharedMaterials)
                {
                    if (material != null)
                    {
                        int instanceId = material.GetInstanceID();

                        if (!matList.ContainsKey(instanceId))
                        {
                            // Material has not been listed yet, add it to the list
                            MaterialToCombine matToCombine = new MaterialToCombine();
                            matToCombine._material = material;
                            matToCombine._uvBounds = uvBound;
                            matToCombine._meshHavingBiggestUVBounds = sharedMesh;
                            matList.Add(instanceId, matToCombine);
                        }
                        else
                        {
                            // This _material has already been found, check if the uv bounds is bigger
                            Rect maxRect = getMaxRect(matList[instanceId]._uvBounds, uvBound);
                            MaterialToCombine matToCombine = matList[instanceId];
                            matToCombine._uvBounds = maxRect;
                            matToCombine._meshHavingBiggestUVBounds = sharedMesh;
                            matList[instanceId] = matToCombine;
                        }
                    }
                    else
                    {
                        // The _material is null
                    }
                }
            }

            // SkinnedMeshes renderer
            foreach (RendererObject<SkinnedMeshRenderer> skinnedMesh in skinnedMeshes)
            {
                Rect uvBound = getUVBounds(skinnedMesh.Renderer.sharedMesh.uv);

                foreach (Material material in skinnedMesh.Renderer.sharedMaterials)
                {
                    if (material != null)
                    {
                        int instanceId = material.GetInstanceID();

                        if (!matList.ContainsKey(instanceId))
                        {
                            // Material has not been listed yet, add it to the list
                            MaterialToCombine matToCombine = new MaterialToCombine();
                            matToCombine._material = material;
                            matToCombine._uvBounds = uvBound;
                            matList.Add(instanceId, matToCombine);
                        }
                        else
                        {
                            // This _material has already been found, check if the uv bounds is bigger
                            Rect maxRect = getMaxRect(matList[instanceId]._uvBounds, uvBound);
                            MaterialToCombine matToCombine = matList[instanceId];
                            matToCombine._uvBounds = maxRect;
                            matList[instanceId] = matToCombine;
                        }
                    }
                    else
                    {
                        // The _material is null
                    }
                }
            }

            return new List<MaterialToCombine>(matList.Values);
        }

        // Return the bound of the uv list (min, max for x and y axis)
        private Rect getUVBounds(Vector2[] uvs)
        {
            if (uvs.Length > 0)
            {
                float[] x = new float[uvs.Length];
                float[] y = new float[uvs.Length];

                for (int i = 0; i < uvs.Length; i++)
                {
                    x[i] = uvs[i].x;
                    y[i] = uvs[i].y;
                }

                return new Rect(x.Min(), y.Min(), x.Max() - x.Min(), y.Max() - y.Min());
            }
            else
            {
                return new Rect(0, 0, 1, 1);
            }
        }

        // Return the maximum rect based on the two rect parameters
        private Rect getMaxRect(Rect uv1, Rect uv2)
        {
            Rect newRect = new Rect();
            newRect.xMin = Math.Min(uv1.xMin, uv2.xMin);
            newRect.yMin = Math.Min(uv1.yMin, uv2.yMin);
            newRect.xMax = Math.Max(uv1.xMax, uv2.xMax);
            newRect.yMax = Math.Max(uv1.yMax, uv2.yMax);
            return newRect;
        }

        /// <summary>
        /// Reverse combine process, destroy all created objects and reactivate original mesh renderers
        /// </summary>
        public void UnCombine()
        {
#if UNITY_EDITOR
            // Hide progressbar
            EditorUtility.ClearProgressBar();
#endif

            // Reactivate original renderers
            EnableRenderers(_meshList, _skinnedMeshList);

            if (_targetParentForCombinedGameObjects == _targetGameObject && _combinedResult != null)
            {
                for (int i = 0; i < _combinedResult.GetCombinedIndexCount(); i++)
                {
                    if (_combinedResult._combinedGameObjectFromMeshList.Count > i)
                    {
                        foreach (GameObject go in _combinedResult._combinedGameObjectFromMeshList[i])
                        {
                            DestroyImmediate(go);
                        }
                        foreach (GameObject go in _combinedResult._combinedGameObjectFromSkinnedMeshList[i])
                        {
                            DestroyImmediate(go);
                        }
                    }
                }
            }
            else
            {
                DestroyImmediate(_targetParentForCombinedGameObjects);
            }

            // Clear the packed textures
            _texturePackers.Clear();
            _materialsToCombine.Clear();
            _multiMaterialsList.Clear();
            _meshCombiner.Clear();
            _meshList.Clear();
            _skinnedMeshList.Clear();
            _uniqueCombinedMeshId.Clear();
            _copyMeshId.Clear();
            _toSavePrefabList.Clear();
            _toSaveObjectList.Clear();
            _toSaveMeshList.Clear();
            _toSaveSkinnedObjectList.Clear();

            if (_combinedResult != null)
            {
                _combinedResult.Clear();
            }
            _combiningState = CombineStatesList.Uncombined;

            Logger.Instance.AddLog("SuperCombiner", "Successfully uncombined game objects.");
        }

        /// <summary>
        /// Get the first level children list of the parents
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        private List<Transform> GetFirstLevelChildren(Transform parent)
        {
            List<Transform> children = new List<Transform>();
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                children.Add(parent.transform.GetChild(i));
            }
            return children;
        }

        /// <summary>
        /// Save combined objects
        /// </summary>
        public void Save()
        {
#if UNITY_EDITOR
            // Combine process is finished
            EditorUtility.ClearProgressBar();

            if (_folderDestination == "")
            {
                // Default export folder destination
                _folderDestination = "Assets/SuperCombiner/Combined";
            }

            // Check if destination folder exists
            if (!Directory.Exists(_folderDestination))
            {
                Directory.CreateDirectory(_folderDestination);
            }

            // Generate new instances (copy from modifiedObjectList) to be saved, so that objects in modifiedObjectList won't be affected by user's modification/deletion
            _toSavePrefabList.Clear();
            _toSaveObjectList.Clear();
            _toSaveMeshList.Clear();
            _toSaveSkinnedObjectList.Clear();
            Material[] savedMaterial = new Material[_combinedResult.GetCombinedIndexCount()];

            for (int i = 0; i < _combinedResult.GetCombinedIndexCount(); i++)
            {
                if (_combinedResult._combinedMaterials[i].material != null)
                {
                    _texturePackers[i].GenerateCopyedMaterialToSave();

                    if (_texturePackers[i].GetCombinedMaterialToSave() == null)
                    {
                        Logger.Instance.AddLog("SuperCombiner", "Instance of combined _material has been lost, try to combine again before saving.", Logger.LogLevel.LOG_ERROR);
                    }
                    else
                    {
                        // We need to know if the combined _material has already been saved
                        savedMaterial[i] = AssetDatabase.LoadAssetAtPath<Material>(_folderDestination + "/Materials/" + _texturePackers[i]._copyedMaterials.name + ".mat");
                    }
                }
            }

            if (_combiningState == CombineStatesList.Combined)
            {
                // List of all different meshes found on every game objects to save, with no duplication
                Dictionary<string, Mesh> meshMaterialId = new Dictionary<string, Mesh>();

                bool outputSkinnedMesh = false;
                List<Transform> children = new List<Transform>();
                if (_combineMeshes && (LunarCatsStudio.SuperCombiner.MeshOutput)_meshOutput == LunarCatsStudio.SuperCombiner.MeshOutput.SkinnedMesh)
                {
                    // If output is skinnedMesh, we save a copy of modifiedParent as prefab
                    GameObject copy = InstantiateCopy(_targetParentForCombinedGameObjects, false);
                    _toSavePrefabList.Add(copy);
                    outputSkinnedMesh = true;
                    children = GetFirstLevelChildren(copy.transform);
                }
                else
                {
                    children = GetFirstLevelChildren(_targetParentForCombinedGameObjects.transform);
                }

                // Generate copy of game objects to be saved
                foreach (Transform child in children)
                {
                    GameObject copy;
                    if (outputSkinnedMesh)
                    {
                        copy = child.gameObject;
                    }
                    else
                    {
                        copy = InstantiateCopy(child.gameObject, false);
                    }

                    List<RendererObject<MeshRenderer>> meshes = FindEnabledMeshes(copy.transform);
                    List<RendererObject<SkinnedMeshRenderer>> skinnedMeshes = FindEnabledSkinnedMeshes(copy.transform);
                    List<MeshCollider> meshColliders = FindEnabledMeshColliders(copy.transform);

                    // Create a copy of mesh
                    foreach (RendererObject<MeshRenderer> mesh in meshes)
                    {
                        int instanceId = mesh.Renderer.GetComponent<MeshFilter>().sharedMesh.GetInstanceID();

                        if (_uniqueCombinedMeshId.ContainsKey(instanceId))
                        {
                            if (meshMaterialId.ContainsKey(_uniqueCombinedMeshId[instanceId]))
                            {
                                // This mesh is shared with other game objects, so we reuse the first instance to avoid duplication
                                mesh.Renderer.GetComponent<MeshFilter>().sharedMesh = meshMaterialId[_uniqueCombinedMeshId[instanceId]];
                            }
                            else
                            {
                                Mesh copyOfMesh = _meshCombiner.copyMesh(mesh.Renderer.GetComponent<MeshFilter>().sharedMesh);
                                mesh.Renderer.GetComponent<MeshFilter>().sharedMesh = copyOfMesh;
                                meshMaterialId.Add(_uniqueCombinedMeshId[instanceId], copyOfMesh);
                                _toSaveMeshList.Add(copyOfMesh);
                            }

                            // Apply a copy of the _material to save
                            Material[] newMat = new Material[mesh.Renderer.sharedMaterials.Length];
                            for (int j = 0; j < mesh.Renderer.sharedMaterials.Length; j++)
                            {
                                if (_combineMaterials)
                                {
                                    // Get the _index of this combined _material
                                    int index = 0;
                                    for (int k = 0; k < _combinedResult._combinedMaterials.Count; k++)
                                    {
                                        if (mesh.Renderer.sharedMaterials[j] == _combinedResult._combinedMaterials[k].material)
                                        {
                                            index = k;
                                        }
                                    }
                                    if (savedMaterial[index] != null)
                                    {
                                        // If the combined _material already exists, assign it
                                        newMat[j] = savedMaterial[index];
                                    }
                                    else
                                    {
                                        newMat[j] = _texturePackers[index].GetCombinedMaterialToSave();
                                    }
                                }
                                else
                                {
                                    //newMat[j] = _texturePackers[i].GetTransformedMaterialToSave(mesh.sharedMaterials[j].name);
                                }
                            }

                            mesh.Renderer.sharedMaterials = newMat;
                            _toSaveObjectList.Add(mesh.Renderer);
                        }
                        else
                        {
                            Logger.Instance.AddLog("SuperCombiner", "Could not find " + mesh.Renderer.name + " in _uniqueCombinedMeshId, data may has been lost, try to combine again before saving.", Logger.LogLevel.LOG_ERROR);
                        }

                    }
                    foreach (RendererObject<SkinnedMeshRenderer> skinnedmesh in skinnedMeshes)
                    {
                        int instanceId = skinnedmesh.Renderer.sharedMesh.GetInstanceID();

                        if (_uniqueCombinedMeshId.ContainsKey(instanceId))
                        {
                            if (meshMaterialId.ContainsKey(_uniqueCombinedMeshId[instanceId]))
                            {
                                // This mesh is shared with other game objects, so we reuse the first instance to avoid duplication
                                skinnedmesh.Renderer.sharedMesh = meshMaterialId[_uniqueCombinedMeshId[instanceId]];
                            }
                            else
                            {
                                Mesh copyOfMesh = _meshCombiner.copyMesh(skinnedmesh.Renderer.sharedMesh);
                                skinnedmesh.Renderer.sharedMesh = copyOfMesh;
                                meshMaterialId.Add(_uniqueCombinedMeshId[instanceId], copyOfMesh);
                                _toSaveMeshList.Add(copyOfMesh);
                            }

                            // Apply a copy of the _material to save
                            Material[] newMat = new Material[skinnedmesh.Renderer.sharedMaterials.Length];
                            for (int j = 0; j < skinnedmesh.Renderer.sharedMaterials.Length; j++)
                            {
                                if (_combineMaterials)
                                {
                                    // Get the _index of this combined _material
                                    int index = 0;
                                    for (int k = 0; k < _combinedResult._combinedMaterials.Count; k++)
                                    {
                                        if (skinnedmesh.Renderer.sharedMaterials[j] == _combinedResult._combinedMaterials[k].material)
                                        {
                                            index = k;
                                        }
                                    }
                                    if (savedMaterial[index] != null)
                                    {
                                        // If the combined _material already exists, assign it
                                        newMat[j] = savedMaterial[index];
                                    }
                                    else
                                    {
                                        newMat[j] = _texturePackers[index].GetCombinedMaterialToSave();
                                    }
                                }
                                else
                                {
                                    //newMat[j] = _texturePackers[i].GetTransformedMaterialToSave(skinnedmesh.sharedMaterials[j].name);
                                }
                            }
                            skinnedmesh.Renderer.sharedMaterials = newMat;
                            _toSaveSkinnedObjectList.Add(skinnedmesh.Renderer);
                        }
                        else
                        {
                            Logger.Instance.AddLog("SuperCombiner", "Could not find " + skinnedmesh.Renderer.name + " in _uniqueCombinedMeshId, data may has been lost, try to combine again before saving.", Logger.LogLevel.LOG_ERROR);
                        }

                    }

                    // Assign to mesh colliders the mesh that will be saved
                    foreach (MeshCollider collider in meshColliders)
                    {
                        int instanceId = collider.sharedMesh.GetInstanceID();

                        string id = null;
                        _copyMeshId.TryGetValue(instanceId, out id);
                        if (id != null)
                        {
                            if (meshMaterialId.ContainsKey(id))
                            {
                                collider.sharedMesh = meshMaterialId[id];
                            }
                        }
                        else
                        {
                            // This means the collider has a mesh that is not present in the combine list
                            // In this case, keep the meshCollider component intact
                        }
                    }

                    // Add this GameObject to the list of prefab to save
                    if (!outputSkinnedMesh)
                    {
                        _toSavePrefabList.Add(copy);
                    }
                }
            }

            // Saving process
            if (_saveTextures)
            {
                for (int i = 0; i < _combinedResult.GetCombinedIndexCount(); i++)
                {
                    if (_combinedResult._combinedMaterials[i].material != null)
                    {
                        Saver.SaveTextures(i, _folderDestination, _sessionName, _texturePackers[i]);
                    }
                }
            }
            if (_saveMaterials)
            {
                for (int i = 0; i < _combinedResult.GetCombinedIndexCount(); i++)
                {
                    if (_combinedResult._combinedMaterials[i].material != null)
                    {
                        Saver.SaveMaterial(i, _folderDestination, _sessionName, _texturePackers[i]);
                    }
                }
            }
            if (_savePrefabs)
            {
                Saver.SavePrefabs(_toSavePrefabList, _toSaveMeshList, _folderDestination, _sessionName);
                for (int n = 0; n < _toSavePrefabList.Count; n++)
                {
                    DestroyImmediate(_toSavePrefabList[n]);
                }
                _toSavePrefabList.Clear();
                _toSaveMeshList.Clear();
            }
            if (_saveMeshObj)
            {
                for (int i = 0; i < _combinedResult.GetCombinedIndexCount(); i++)
                {
                    if (_combinedResult._combinedMaterials[i].material != null)
                    {
                        Saver.SaveMeshesObj(_combinedResult._combinedGameObjectFromMeshList[i], _combinedResult._combinedGameObjectFromSkinnedMeshList[i], _folderDestination);
                        for (int n = 0; n < _toSaveObjectList.Count; n++)
                        {
                            DestroyImmediate(_toSaveObjectList[n]);
                        }
                        for (int n = 0; n < _toSaveSkinnedObjectList.Count; n++)
                        {
                            DestroyImmediate(_toSaveSkinnedObjectList[n]);
                        }
                        _toSaveObjectList.Clear();
                        _toSaveSkinnedObjectList.Clear();
                    }
                }
            }
            if (_saveMeshFbx)
            {
                //SaveMeshesFbx();
            }


            //Save the combine settings
            createSuperCombinerSettings();
            Saver.SaveCombinedSettings(_scSettings, _folderDestination, _sessionName);

            // Saves the combined result asset
            _combinedResult._logs = Logger.Instance.GetLogs();
            Saver.SaveCombinedResults(_combinedResult, _folderDestination, _sessionName);

            EditorUtility.DisplayDialog("Super Combiner", "Objects saved in '" + _folderDestination + "/' \n\nThanks for using Super Combiner.", "Ok");

            // Hide progressbar
            EditorUtility.ClearProgressBar();
#endif
        }

        /// <summary>
        /// create and set the super combiner settings scriptable object
        /// </summary>
        private void createSuperCombinerSettings()
        {
            // If _scSettings has not been created yet, create it
            if (_scSettings == null)
            {
                _scSettings = (SuperCombinerSettings)ScriptableObject.CreateInstance(typeof(SuperCombinerSettings));
            }

            // General settings
            _scSettings.generalSettings.versionNumber = versionNumber;
            _scSettings.generalSettings.combineAtRuntime = _combineAtRuntime;
            _scSettings.generalSettings.sessionName = _sessionName;
            _scSettings.generalSettings.targetGameObject = _targetGameObject;

            // Textures settings
            _scSettings.textureSettings.atlasSize = _textureAtlasSize;
            _scSettings.textureSettings.padding = _atlasPadding;
            _scSettings.textureSettings.tilingFactor = _tilingFactor;

            // Materials settings
            _scSettings.materialSettings.multipleMaterialsCount = _multiMaterialsCount;
            _scSettings.materialSettings.multipleMaterialsMode = _multipleMaterialsMode;
            _scSettings.materialSettings.multiMaterials0 = multiMaterials0;
            _scSettings.materialSettings.multiMaterials1 = multiMaterials1;
            _scSettings.materialSettings.multiMaterials2 = multiMaterials2;
            _scSettings.materialSettings.multiMaterials3 = multiMaterials3;
            _scSettings.materialSettings.multiMaterials4 = multiMaterials4;
            _scSettings.materialSettings.multiMaterials5 = multiMaterials5;
            _scSettings.materialSettings.multiMaterials6 = multiMaterials6;
            _scSettings.materialSettings.multiMaterials7 = multiMaterials7;
            _scSettings.materialSettings.multiMaterials8 = multiMaterials8;
            _scSettings.materialSettings.multiMaterials9 = multiMaterials9;
            _scSettings.materialSettings.multiMaterials10 = multiMaterials10;
            _scSettings.materialSettings.customShaderProperties = _customTextureProperies;


            // Meshs settings
            _scSettings.meshSettings.manageLODs = _manageLodLevel;
            _scSettings.meshSettings.managedLODLevel = _managedLodLevel;
            _scSettings.meshSettings.manageColliders = _manageColliders;
            _scSettings.meshSettings.targetGameObject = _targetGameObject;
            _scSettings.meshSettings.combineMeshs = _combineMeshes;
            _scSettings.meshSettings.generateUv2 = _generateUv2;
            _scSettings.meshSettings.meshOutputType = (MeshSettings.MeshOutputType)_meshOutput;
        }
    }
}