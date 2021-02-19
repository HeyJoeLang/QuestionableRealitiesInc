using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LunarCatsStudio.SuperCombiner
{
	/// <summary>
	/// Submesh splitter.
    /// A helper class that offers possibility to split submeshes into separate meshes
	/// </summary>
	public class SubmeshSplitter {

        /// <summary>
        /// Extract a specific submesh at a given index from a mesh and return it as a new Mesh
        /// </summary>
        /// <param name="mesh">The mesh containing subMeshes</param>
        /// <param name="index">The index of the subMesh to extract</param>
        /// <returns></returns>
        public static Mesh ExtractSubmesh(Mesh mesh, int index)
        {
            // Create the new mesh corresponding to the submesh at index i
            Mesh extractedSubMesh = CreateMeshFromSubmesh(mesh, new int[] { index }, index);
            // Return the extracted subMesh
            return extractedSubMesh;
        }

        /// <summary>
        /// Extract all the submeshes at given indexes into a separate mesh in a new GameObject
        /// </summary>
        /// <param name="meshFilter"></param>
        /// <param name="subMeshesIndex">List of ths submeshes indexes to extract</param>
        /// <param name="index">Only used to name the new created mesh</param>
        /// <returns>Returns a new GameObject with MeshRenderer and MeshFilter components.</returns>
		public static MeshRenderer SplitSubmeshes(MeshFilter meshFilter, int[] subMeshesIndex, int index)
        {
            if(subMeshesIndex.Length == 0)
            {
                Logger.Instance.AddLog("SuperCombiner", "Could not split submeshes of mesh " + meshFilter + " because indexes is null", Logger.LogLevel.LOG_ERROR);
                return null;
            }
            else if(meshFilter.sharedMesh == null)
            {
                Logger.Instance.AddLog("SuperCombiner", "Could not split submeshes of mesh " + meshFilter + " because it has no mesh", Logger.LogLevel.LOG_ERROR);
                return null;
            }

            Mesh mesh = meshFilter.sharedMesh;
            
            Material[] materials = meshFilter.GetComponent<MeshRenderer>().sharedMaterials;
            Material[] splitMaterials = new Material[subMeshesIndex.Length];
            for(int i=0; i<subMeshesIndex.Length; i++)
            {
                splitMaterials[i] = materials[subMeshesIndex[0]];
            }

            // Create the new mesh corresponding to the submesh at index i
            Mesh newMesh = CreateMeshFromSubmesh(mesh, subMeshesIndex, index);

            // Generate target GameObject with MeshFilter and MeshRenderer
            GameObject go = GenerateGameObject(meshFilter.transform, false, meshFilter.gameObject.name, newMesh, splitMaterials);

            // Return the new meshRenderer component
            return go.GetComponent<MeshRenderer>();
		}

        /// <summary>
        /// Extract all the submeshes at given indexes into a separate mesh in a new GameObject
        /// </summary>
        /// <param name="skinnedMesh"></param>
        /// <param name="subMeshesIndex"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static SkinnedMeshRenderer SplitSubmeshes(SkinnedMeshRenderer skinnedMesh, int[] subMeshesIndex, int index)
        {
            if (subMeshesIndex.Length == 0)
            {
                Logger.Instance.AddLog("SuperCombiner", "Could not split submeshes of mesh " + skinnedMesh + " because indexes is null", Logger.LogLevel.LOG_ERROR);
                return null;
            }
            else if (skinnedMesh.sharedMesh == null)
            {
                Logger.Instance.AddLog("SuperCombiner", "Could not split submeshes of mesh " + skinnedMesh + " because it has no mesh", Logger.LogLevel.LOG_ERROR);
                return null;
            }

            Mesh mesh = skinnedMesh.sharedMesh;

            Material[] materials = skinnedMesh.sharedMaterials;
            Material[] splitMaterials = new Material[subMeshesIndex.Length];
            for (int i = 0; i < subMeshesIndex.Length; i++)
            {
                splitMaterials[i] = materials[subMeshesIndex[0]];
            }

            // Create the new mesh corresponding to the submesh at index i
            Mesh newMesh = CreateMeshFromSubmesh(mesh, subMeshesIndex, index);

            // Generate target GameObject with MeshFilter and MeshRenderer
            GameObject go = GenerateGameObject(skinnedMesh.transform, true, skinnedMesh.gameObject.name, newMesh, splitMaterials);

            // Return the new meshRenderer component
            return go.GetComponent<SkinnedMeshRenderer>();
        }

        /// <summary>
        /// Creates the mesh corresponding to a specific submesh.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="subMeshesIndex"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static Mesh CreateMeshFromSubmesh(Mesh mesh, int[] subMeshesIndex, int index)
        {
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Vector4[] tangents = mesh.tangents;
            Color[] colors = mesh.colors;
            Vector2[] uvs = mesh.uv;
            Vector2[] uvs2 = mesh.uv2;

            Mesh newMesh = new Mesh();
            newMesh.name = mesh.name + "_" + index;
            newMesh.vertices = vertices;
            newMesh.normals = normals;
            newMesh.tangents = tangents;
            newMesh.colors = colors;
            newMesh.uv = uvs;
            newMesh.uv2 = uvs2;
            newMesh.subMeshCount = subMeshesIndex.Length;

            for (int i=0; i<subMeshesIndex.Length; i++)
            {
                int[] triangles = mesh.GetTriangles(subMeshesIndex[i]);
                int[] indices = mesh.GetIndices(subMeshesIndex[i]);
                MeshTopology topology = mesh.GetTopology(subMeshesIndex[i]);
                newMesh.SetIndices(indices, topology, i);
                newMesh.SetTriangles(triangles, i);
            }

#if UNITY_EDITOR
            // Optimize the vertices
            MeshUtility.Optimize(newMesh);
#endif

            return newMesh;
        }

        /// <summary>
        /// Generates a new game object.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="skinnedMesh"></param>
        /// <param name="name_p"></param>
        /// <param name="newMesh_p"></param>
        /// <param name="mat"></param>
        /// <returns></returns>
        private static GameObject GenerateGameObject(Transform parent, bool skinnedMesh, string name_p, Mesh newMesh_p, Material[] mat)
        {
            GameObject go = new GameObject(name_p);
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;            
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.mesh = newMesh_p;
            if(skinnedMesh)
            {
                SkinnedMeshRenderer meshRenderer = go.AddComponent<SkinnedMeshRenderer>();
                meshRenderer.materials = mat;
            } else
            {
                MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
                meshRenderer.materials = mat;
            }
            return go;
        }
    }
}
