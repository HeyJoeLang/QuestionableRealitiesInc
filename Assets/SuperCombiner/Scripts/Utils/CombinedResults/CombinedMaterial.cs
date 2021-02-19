using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LunarCatsStudio.SuperCombiner
{
    /// <summary>
    /// Stores information about a combined _material
    /// </summary>
    [System.Serializable]
    public class CombinedMaterial
    {
        // The combined _material
        public Material material;
        // The list of uvs
        public Rect[] uvs;
        public Rect[] uvs2;
        public List<float> scaleFactors = new List<float>();
        // List of meshes UV bound
        public List<Rect> meshUVBounds = new List<Rect>();
        // This will be true if there is only one _material to combine, is this case we simply reuse the existing _material
        public bool isOriginalMaterial = false;
        // The _index at which this combined _material will be displayed in the inspector. Usefull in multimaterial when some combined _material are null
        public int displayedIndex;
        // Editor display parameter
        public bool showCombinedMaterial;
        public bool showUVs;
        public bool showMeshUVBounds;
    }
}
