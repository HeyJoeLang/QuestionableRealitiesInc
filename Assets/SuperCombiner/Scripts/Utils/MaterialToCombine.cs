using UnityEngine;
using System.Collections;

namespace LunarCatsStudio.SuperCombiner
{
	/// <summary>
	/// Class storing data of a single _material to combine
	/// </summary>
    public class MaterialToCombine
    {
        /// <summary>
		///  The _material to combine
        /// </summary>
        public Material _material;
        /// <summary>
		/// The maximum uv bounds for this _material
		/// </summary>
        public Rect _uvBounds;
		/// <summary>
		/// The _index of the combined _material
		/// </summary>
		public int _combinedIndex;
        /// <summary>
        /// Index of this element in it's list. This _index is equal to the combinedIdex
        /// </summary>
        public int _index;
        /// <summary>
        /// This is the mesh having the biggest UV bounds
        /// </summary>
        public Mesh _meshHavingBiggestUVBounds;

        /// <summary>
        /// Get the scaled and offseted by _material parameters uv bounds
        /// </summary>
        /// <returns></returns>
        public Rect GetScaledAndOffsetedUVBounds()
        {
            Rect rect = _uvBounds;
            if (_material.HasProperty("_MainTex"))
            {
                rect.size = Vector2.Scale(rect.size, _material.mainTextureScale);
                rect.position += _material.mainTextureOffset;
            }
            return rect;
        }
    }

}
