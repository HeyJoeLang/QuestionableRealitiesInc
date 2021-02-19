using UnityEngine;
using System.Collections.Generic;

namespace LunarCatsStudio.SuperCombiner
{
    /// <summary>
    /// Data class used to store information about a mesh rendere (or skinned mesh renderer) 
    /// and all it's original materials, and eventually the list of splitted game objects
    /// </summary>
    class MeshRendererAndOriginalMaterials
    {
        /// <summary>
        /// The list of MeshRenderer
        /// </summary>
        public List<MeshRenderer> _meshRenderers = new List<MeshRenderer>();
        /// <summary>
        /// List of skinnedMeshRenderer
        /// </summary>
        public List<SkinnedMeshRenderer> _skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
        /// <summary>
        /// The list of original materials for each _meshRenderers
        /// </summary>
        public List<Material[]> _originalMaterials = new List<Material[]>();
        /// <summary>
        /// The list of original materials for each _skinnedMeshRenderers
        /// </summary>
        public List<Material[]> _originalskinnedMeshMaterials = new List<Material[]>();

        /// <summary>
        /// List of splitted GameObject created
        /// </summary>
        public List<GameObject> _splittedGameObject = new List<GameObject>();
    }
}
