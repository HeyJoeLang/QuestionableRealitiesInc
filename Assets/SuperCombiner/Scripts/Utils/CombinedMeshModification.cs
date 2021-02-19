using UnityEngine;
using System.Collections;
using LunarCatsStudio.SuperCombiner;

namespace LunarCatsStudio.SuperCombiner
{
    /// <summary>
    /// Attach this script to each combined Gameobject that you wish to remove part during runtime.
    /// This only works for a combined GameObject with "combine mesh" parameter set to true.
    /// You can remove parts of the combined mesh using the "RemoveFromCombined" API. Use the instanceID of the object you wish to 
    /// remove. In order know the correct instanceID, check in the "combinedResults" file under "mesh Results" -> "Instance Ids".
    /// </summary>
    public class CombinedMeshModification : MonoBehaviour
    {
		// The combined result
		[Tooltip("Reference to the _combinedResult file")]
        public CombinedResult _combinedResult;
		// The MeshFilter to which the combinedMesh is set
        [Tooltip("Reference to the MeshFilter in which the combined mesh is attached to")]
		public MeshFilter _meshFilter;

		// A new instance of combined result is created at runtime to keep original intact
		private CombinedResult _currentCombinedResult;

        // Use this for initialization
        void Awake()
        {
            // Instanciate a copy of the _combinedResult
			_currentCombinedResult = GameObject.Instantiate(_combinedResult) as CombinedResult;
        }

        /// <summary>
        /// Remove a GameObject from the combined mesh
        /// </summary>
        /// <param name="gameObject"></param>
        public void RemoveFromCombined(GameObject gameObject)
        {
			RemoveFromCombined (gameObject.GetInstanceID ());
        }

        /// <summary>
        /// Remove a GameObject from the combined mesh
        /// </summary>
        /// <param name="instanceID"></param>
        public void RemoveFromCombined(int instanceID)
        {
			// Check if _meshFilter is set
			if (_meshFilter == null) 
			{
                Logger.Instance.AddLog("SuperCombiner", "MeshFilter is not set, please assign MeshFilter parameter before trying to remove a part of it's mesh", Logger.LogLevel.LOG_WARNING);
				return;
			}
            bool success = false;
			foreach (MeshCombined meshResult in _currentCombinedResult._meshResults)
            {
                if (meshResult.instanceIds.Contains(instanceID))
                {
                    Logger.Instance.AddLog("SuperCombiner", "Removing object '" + instanceID + "' from combined mesh");
					_meshFilter.mesh = meshResult.RemoveMesh(instanceID, _meshFilter.mesh);
                    success = true;
                }
            }
            if (!success)
            {
                Logger.Instance.AddLog("SuperCombiner", "Could not remove object '" + instanceID + "' because it was not found", Logger.LogLevel.LOG_WARNING);
            }
        }
    }

}