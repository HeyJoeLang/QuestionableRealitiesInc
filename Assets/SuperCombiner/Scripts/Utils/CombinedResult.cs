using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace LunarCatsStudio.SuperCombiner
{
    /// <summary>
    /// This class stores all the relevant data of a combined session
    /// </summary>
    public class CombinedResult : ScriptableObject
    {
        // Editor display parameters
        public bool _showCombinedMaterials = false;
        public bool _showCombinedMeshes = false;
        public bool _showLogs = false;

        // Logs
        public string _logs;
        public List<String> _warningMessages = new List<string>();

        // The list of combined _material
        public List<CombinedMaterial> _combinedMaterials = new List<CombinedMaterial>();

        /// <summary>
        /// The list of dictionnaries of original materials to combine. There is one dicionnary by _combinedIndex.
        /// Key = _material's instanceID
        /// </summary>
        public List<Dictionary<int, MaterialToCombine>> _originalMaterialList = new List<Dictionary<int, MaterialToCombine>>();
        /// <summary>
        /// Dictionnary of original and reference _material's instanceID for each _combinedIndex. The reference _material is the one used and copied to create the combined _material
        /// </summary>
		public Dictionary<int, int> _originalReferenceMaterial = new Dictionary<int, int>();

        /// <summary>
        /// List of generated combined GameObjects for meshes
        /// </summary>
        public List<List<GameObject>> _combinedGameObjectFromMeshList = new List<List<GameObject>>();
        /// <summary>
        /// List of generated combined GameObjects for skinnedMeshes
        /// </summary>
        public List<List<GameObject>> _combinedGameObjectFromSkinnedMeshList = new List<List<GameObject>>();

        /// <summary>
        /// The list of mesh results
        /// </summary>
		public List<MeshCombined> _meshResults = new List<MeshCombined>();
        /// <summary>
        /// The number of original materials combined
        /// </summary>
        public int _materialCombinedCount;
        /// <summary>
        /// The number of combined _material created. If multimaterial is used, more than one _material should be created
        /// </summary>
        public int _combinedMaterialCount;
        /// <summary>
        /// The number of original meshes combined
        /// </summary>
        public int _meshesCombinedCount;
        /// <summary>
        /// The number of skinnedMeshes combined
        /// </summary>
        public int _skinnedMeshesCombinedCount;
        /// <summary>
        /// The number of vertex in combined mesh
        /// </summary>
        public int _totalVertexCount;
        /// <summary>
        /// The number of submeshes
        /// </summary>
        public int _subMeshCount;
        /// <summary>
        /// The _duration of the process
        /// </summary>
        public TimeSpan _duration;

        /// <summary>
        /// Clear all combine data
        /// </summary>
        public void Clear()
        {
            if (_originalMaterialList != null)
            {
                for (int i = 0; i < _originalMaterialList.Count; i++)
                {
                    _originalMaterialList[i].Clear();
                }
                _originalMaterialList.Clear();
            }
            _originalReferenceMaterial.Clear();
            _materialCombinedCount = 0;
            _combinedMaterials.Clear();
            _combinedMaterialCount = 0;
            _meshesCombinedCount = 0;
            _skinnedMeshesCombinedCount = 0;
            _totalVertexCount = 0;
            _subMeshCount = 0;
            _meshResults.Clear();
            _combinedGameObjectFromMeshList.Clear();
            _combinedGameObjectFromSkinnedMeshList.Clear();
            _logs = " ";
            _warningMessages.Clear();
        }

        /// <summary>
        /// Add a new warning message
        /// </summary>
        /// <param name="message"></param>
        public void AddWarningMessage(String message)
        {
            _warningMessages.Add(message);
        }

        /// <summary>
        /// Sets a combined _material
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="combinedIndex"></param>
        /// <param name="isOriginal"></param>
        public void SetCombinedMaterial(Material mat, int combinedIndex, bool isOriginal)
        {
            if (combinedIndex < _combinedMaterials.Count)
            {
                _combinedMaterials[combinedIndex].material = mat;
                if (!isOriginal)
                {
                    _combinedMaterials[combinedIndex].material.name += "_" + _combinedMaterialCount;
                }
                _combinedMaterials[combinedIndex].displayedIndex = _combinedMaterialCount;
                _combinedMaterials[combinedIndex].isOriginalMaterial = isOriginal;
            }
            _combinedMaterialCount++;
        }

        /// <summary>
        /// Add a new combined _material to the list
        /// </summary>
        public void AddNewCombinedMaterial()
        {
            _combinedMaterials.Add(new CombinedMaterial());
        }

        /// <summary>
        /// Adds a new _material to be combined
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="combinedIndex">Index of the multi material group</param>
        public void AddMaterialToCombine(MaterialToCombine mat, int combinedIndex)
        {
            if (!_originalReferenceMaterial.ContainsKey(combinedIndex))
            {
                _originalReferenceMaterial.Add(combinedIndex, mat._material.GetInstanceID());
            }
            mat._index = _originalMaterialList[combinedIndex].Count;
            _originalMaterialList[combinedIndex].Add(mat._material.GetInstanceID(), mat);

            //Logger.Instance.AddLog("SuperCombiner", "Adding new _material to combine: " + mat._material.name + " at _combinedIndex " + _combinedIndex);
        }

        /// <summary>
        /// Add a new CombinedMesh
        /// </summary>
        /// <param name="combinedMesh"></param>
        /// <param name="combineInstanceID"></param>
        /// <param name="combinedIndex"></param>
        public void AddCombinedMesh(Mesh combinedMesh, CombineInstanceID combineInstanceID, int combinedIndex)
        {
            MeshCombined meshResult = new MeshCombined();

            int vertexIndex = 0;
            int triangleIndex = 0;
            for (int i = 0; i < combineInstanceID._combineInstances.Count; i++)
            {
                if (!meshResult.instanceIds.Contains(combineInstanceID._instancesID[i]))
                {
                    vertexIndex += combineInstanceID._combineInstances[i].mesh.vertexCount;
                    triangleIndex += combineInstanceID._combineInstances[i].mesh.triangles.Length;
                    meshResult.names.Add(combineInstanceID._names[i]);
                    meshResult.instanceIds.Add(combineInstanceID._instancesID[i]);
                    meshResult.indexes.Add(new CombineInstanceIndexes(combineInstanceID._combineInstances[i].mesh, vertexIndex, triangleIndex));
                }
            }

            _meshResults.Add(meshResult);
        }

        /// <summary>
        /// Returns the _index of a given _material in the list 
        /// </summary>
        /// <param name="matToFind"></param>
        /// <param name="combinedIndex"></param>
        /// <returns></returns>
        public int FindCorrespondingMaterialIndex(Material matToFind, int combinedIndex)
        {
            if (combinedIndex < _originalMaterialList.Count)
            {
                if (_originalMaterialList[combinedIndex].ContainsKey(matToFind.GetInstanceID()))
                {
                    return _originalMaterialList[combinedIndex][matToFind.GetInstanceID()]._index;
                }
            }
            Logger.Instance.AddLog("SuperCombiner", "Material " + matToFind + " was not found in list " + combinedIndex, Logger.LogLevel.LOG_WARNING);
            return 0;
        }

        /// <summary>
        /// Get the combined _material associated to the source _material in parameter
        /// </summary>
        /// <param name="sourceMaterial"></param>
        /// <returns></returns>
        public Material GetCombinedMaterial(Material sourceMaterial)
        {
            for (int i = 0; i < _originalMaterialList.Count; i++)
            {
                if (_originalMaterialList[i].ContainsKey(sourceMaterial.GetInstanceID()))
                {
                    return _combinedMaterials[i].material;
                }
            }

            Logger.Instance.AddLog("SuperCombiner", "Could not find combined _material associated with " + sourceMaterial.name, Logger.LogLevel.LOG_WARNING);
            return null;
        }

        public List<Material> GetCombinedMaterials()
        {
            List<Material> materials = new List<Material>();
            for (int i = 0; i < _combinedMaterials.Count; i++)
            {
                if (_combinedMaterials[i].material != null)
                {
                    materials.Add(_combinedMaterials[i].material);
                }
            }
            return materials;
        }

        /// <summary>
        /// Gets the combined _index of the given _material
        /// </summary>
        /// <returns>The combined _index.</returns>
        /// <param name="sourceMaterial">Material.</param>
        public int GetCombinedIndex(Material sourceMaterial)
        {
            for (int i = 0; i < _originalMaterialList.Count; i++)
            {
                if (_originalMaterialList[i].ContainsKey(sourceMaterial.GetInstanceID()))
                {
                    return i;
                }
            }

            Logger.Instance.AddLog("SuperCombiner", "Could not find combined _material associated with " + sourceMaterial.name, Logger.LogLevel.LOG_WARNING);
            return 0;
        }

        /// <summary>
        /// Return the number of combined _material (the total number of multi material group possible)
        /// </summary>
        /// <returns></returns>
        public int GetCombinedIndexCount()
        {
            return _originalMaterialList.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetMaterialGroupCount()
        {
            int count = 0;
            foreach (Dictionary<int, MaterialToCombine> dict in _originalMaterialList)
            {
                if (dict.Count > 0)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
