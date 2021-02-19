using UnityEngine;
using System.Collections.Generic;

namespace LunarCatsStudio.SuperCombiner
{
    /// <summary>
    /// Mesh Combined
    /// Each mesh is a part of the combined result.
    /// Either you combine only materials, then this is a list of new created meshes
    /// Or you combine meshes, they may be split if size exceeds 65k vertices
    /// </summary>
    [System.Serializable]
    public class MeshCombined
    {
        // List of name of the original game objects combined
        public List<string> names = new List<string>();
        // List of instance Id for each original game object combined
        public List<int> instanceIds = new List<int>();
        // List of indexes for combined meshes
        public List<CombineInstanceIndexes> indexes = new List<CombineInstanceIndexes>();

        // Editor display parameter
        public bool showMeshCombined;

        /// <summary>
        /// Removes a mesh (given by the instanceID of the gameObject on which it is attached) from the combined mesh
        /// </summary>
        /// <param name="instanceID"></param>
        public Mesh RemoveMesh(int instanceID, Mesh mesh)
        {
            if (instanceIds.Contains(instanceID))
            {
                int index = instanceIds.IndexOf(instanceID);

                Vector3[] vertices = mesh.vertices;
                Vector3[] newVertices = new Vector3[mesh.vertexCount - indexes[index].vertexCount];
                int[] triangles = mesh.triangles;
                int[] newTriangles = new int[triangles.Length - indexes[index].triangleCount];
                Vector4[] tangents = mesh.tangents;
                Vector4[] newTangents = new Vector4[mesh.tangents.Length - indexes[index].vertexCount];
                Vector2[] uv = mesh.uv;
                Vector2[] newUv = new Vector2[newVertices.Length];
                Vector2[] uv2 = mesh.uv2;
                Vector2[] newUv2 = new Vector2[newVertices.Length];

                // Assign new vertices, uv and uv2
                for (int i = 0; i < newVertices.Length; i++)
                {
                    if (i < indexes[index].firstVertexIndex)
                    {
                        newVertices[i] = vertices[i];
                        newUv[i] = uv[i];
                        newUv2[i] = uv2[i];
                        newTangents[i] = tangents[i];
                    }
                    else
                    {
                        newVertices[i] = vertices[i + indexes[index].vertexCount];
                        newUv[i] = uv[i + indexes[index].vertexCount];
                        newUv2[i] = uv2[i + indexes[index].vertexCount];
                        newTangents[i] = tangents[i + indexes[index].vertexCount];
                    }
                }

                // Assign new triangles
                for (int i = 0; i < newTriangles.Length; i++)
                {
                    if (i < indexes[index].firstTriangleIndex)
                    {
                        newTriangles[i] = triangles[i];
                    }
                    else
                    {
                        newTriangles[i] = triangles[i + indexes[index].triangleCount] - indexes[index].vertexCount;
                    }
                }

                // Offset all vertices and triangles of meshes placed after the instanceID's mesh 
                for (int i = index; i < indexes.Count; i++)
                {
                    indexes[i].MoveIndexes(indexes[index].vertexCount, indexes[index].triangleCount);
                }
                // Delete the mesh from the list
                indexes.RemoveAt(index);
                instanceIds.RemoveAt(index);
                names.RemoveAt(index);

                // Reasign new vertices, triangles and uvs to the mesh
                mesh.Clear();
                mesh.vertices = new List<Vector3>(newVertices).ToArray();
                mesh.SetTriangles(newTriangles, 0);
                mesh.tangents = newTangents;
                mesh.uv = newUv;
                mesh.uv2 = newUv2;
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
            }
            return mesh;
        }
    }
}

