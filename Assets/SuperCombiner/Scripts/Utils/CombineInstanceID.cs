using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LunarCatsStudio.SuperCombiner
{
    /// <summary>
    /// Combine Instance ID
    /// </summary>
    public class CombineInstanceID
    {
        // List of combine instances
        public List<CombineInstance> _combineInstances = new List<CombineInstance>();
        // List of instanceID of every gameObject to combine
        public List<int> _instancesID = new List<int>();
        // List of name of every gameObject to combine
        public List<string> _names = new List<string>();

        /// <summary>
        /// Add a new Combine Instance
        /// </summary>
        /// <param name="subMeshIndex"></param>
        /// <param name="mesh"></param>
        /// <param name="matrix"></param>
        /// <param name="instanceID"></param>
        /// <param name="name"></param>
        public void AddCombineInstance(int subMeshIndex, Mesh mesh, Matrix4x4 matrix, int instanceID, string name)
        {
            // Add the combine instance
            CombineInstance combineInstance = new CombineInstance();
            combineInstance.subMeshIndex = subMeshIndex;
            combineInstance.mesh = mesh;
            combineInstance.transform = matrix;
            _combineInstances.Add(combineInstance);
            // Add the instanceID
            _instancesID.Add(instanceID);
            // Add the name
            _names.Add(name);
        }

        public void AddRange(CombineInstanceID instances)
        {
            _combineInstances.AddRange(instances._combineInstances);
            _instancesID.AddRange(instances._instancesID);
            _names.AddRange(instances._names);
        }

        /// <summary>
        /// Clear data
        /// </summary>
        public void Clear()
        {
            _combineInstances.Clear();
            _instancesID.Clear();
            _names.Clear();
        }

        /// <summary>
        /// Return the number of _combineInstances instances
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return _combineInstances.Count;
        }
    }
}
