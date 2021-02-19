using UnityEngine;
using System.Collections;

namespace LunarCatsStudio.SuperCombiner
{
    /// <summary>
    /// The combine instance indexes of vertex and triangles
    /// </summary>
    [System.Serializable]
    public class CombineInstanceIndexes
    {
        // Index of the first vertex in mesh.vertices
        public int firstVertexIndex;
        // The vertexcount
        public int vertexCount;
        // Index of the first triangle in mesh.triangles
        public int firstTriangleIndex;
        // The trianglecount
        public int triangleCount;

        // Editor display parameter
        public bool showCombinedInstanceIndex;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="vertexIndex"></param>
        /// <param name="trianglesIndex"></param>
        public CombineInstanceIndexes(Mesh mesh, int vertexIndex, int trianglesIndex)
        {
            vertexCount = mesh.vertexCount;
            firstVertexIndex = vertexIndex;
            triangleCount = mesh.triangles.Length;
            firstTriangleIndex = trianglesIndex;
        }

        /// <summary>
        /// Offset first indexes for vertices and triangles
        /// </summary>
        /// <param name="vertexOffset"></param>
        /// <param name="triangleOffset"></param>
        public void MoveIndexes(int vertexOffset_p, int triangleOffset_p)
        {
            firstVertexIndex -= vertexOffset_p;
            firstTriangleIndex -= triangleOffset_p;
        }
    }
}