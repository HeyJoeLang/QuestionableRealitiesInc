using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LunarCatsStudio.SuperCombiner;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// This class manage the combine process of meshes and skinned meshes
/// </summary>
namespace LunarCatsStudio.SuperCombiner
{
    public class MeshCombiner
    {
        /// <summary>
        /// The session name
        /// </summary>
        private string _sessionName = "";
        /// <summary>
        /// List of Blendshape frames
        /// </summary>
        private Dictionary<string, BlendShapeFrame> blendShapes = new Dictionary<string, BlendShapeFrame>();

        private int _vertexOffset = 0;

        /// <summary>
        /// Flag to specify if UV2 have to be generated or not
        /// </summary>
        private bool _generateUv2 = true;

        /// <summary>
        /// The reference to the _combinedResult
        /// </summary>
        private CombinedResult _combinedResult;
        public CombinedResult CombinedResult
        {
            set
            {
                _combinedResult = value;
            }
        }

        /// <summary>
        /// Set the different parameters of MeshCombiner
        /// </summary>
        /// <param name="sessionName_p"></param>
        /// <param name="generateUv2_p"></param>
        public void SetParameters(string sessionName_p, bool generateUv2_p)
        {
            _sessionName = sessionName_p;
            _generateUv2 = generateUv2_p;
        }

        public void Clear()
        {
            blendShapes.Clear();
            _vertexOffset = 0;
        }

        public List<GameObject> CombineToMeshes(List<MeshRenderer> meshRenderers, List<SkinnedMeshRenderer> skinnedMeshRenderers, Transform parent, int combinedIndex)
        {
            // The list of Meshes created
            List<GameObject> combinedMeshes = new List<GameObject>();
            // The list of _combineInstances
            CombineInstanceID combineInstances = new CombineInstanceID();

            int verticesCount = 0;
            int combinedGameObjectCount = 0;

            // Process meshes for combine process
            for (int i = 0; i < meshRenderers.Count; i++)
            {
                // Loop over all submeshes
                for (int j = 0; j < meshRenderers[i].GetComponent<MeshFilter>().sharedMesh.subMeshCount; j++)
                {
                    // Get a copy of the submesh at index j
                    Mesh newMesh = SubmeshSplitter.ExtractSubmesh(meshRenderers[i].GetComponent<MeshFilter>().sharedMesh, j);
                    verticesCount += newMesh.vertexCount;

                    // Create the list of CombineInstance for this mesh
                    Matrix4x4 matrix = parent.transform.worldToLocalMatrix * meshRenderers[i].transform.localToWorldMatrix;
                    combineInstances.AddRange(CreateCombinedInstances(newMesh, new Material[] { meshRenderers[i].sharedMaterials[j] }, meshRenderers[i].gameObject.GetInstanceID(), meshRenderers[i].gameObject.name, matrix, combinedIndex));
                }
            }

            for (int i = 0; i < skinnedMeshRenderers.Count; i++)
            {
                // Loop over all submeshes
                for (int j = 0; j < meshRenderers[i].GetComponent<MeshFilter>().sharedMesh.subMeshCount; j++)
                {
                    // Get a snapshot of the sub skinnedMesh renderer at index j
                    Mesh newMesh = SubmeshSplitter.ExtractSubmesh(skinnedMeshRenderers[i].sharedMesh, j);
                    _vertexOffset += newMesh.vertexCount;
                    verticesCount += newMesh.vertexCount;

                    // Create the list of CombineInstance for this skinnedMesh
                    Matrix4x4 matrix = parent.transform.worldToLocalMatrix * skinnedMeshRenderers[i].transform.localToWorldMatrix;
                    combineInstances.AddRange(CreateCombinedInstances(newMesh, new Material[] { skinnedMeshRenderers[i].sharedMaterials[j] }, skinnedMeshRenderers[i].GetInstanceID(), skinnedMeshRenderers[i].gameObject.name, matrix, combinedIndex));
                }
            }


            if (combineInstances.Count() > 0)
            {
                // Create the combined GameObject which contains the combined meshes
                combinedMeshes.Add(CreateCombinedMeshGameObject(combineInstances, parent, combinedGameObjectCount, combinedIndex));
            }

            return combinedMeshes;
        }

        public List<GameObject> CombineToSkinnedMeshes(List<MeshRenderer> meshRenderers, List<SkinnedMeshRenderer> skinnedMeshRenderers, Transform parent, int combinedIndex)
        {
            // The list of Meshes created
            List<GameObject> combinedMeshes = new List<GameObject>();
            // The list of _combineInstances
            CombineInstanceID combineInstances = new CombineInstanceID();

            int verticesCount = 0;
            int combinedGameObjectCount = 0;

            /*
			/ Skinned mesh parameters
			*/
            // List of bone weight
            List<BoneWeight> boneWeights = new List<BoneWeight>();
            // List of bones
            List<Transform> bones = new List<Transform>();
            // List of bindposes
            List<Matrix4x4> bindposes = new List<Matrix4x4>();
            // List of original bones mapped to their instanceId
            Dictionary<int, Transform> originalBones = new Dictionary<int, Transform>();
            // Link original bone instanceId to the new created bones
            Dictionary<int, Transform> originToNewBoneMap = new Dictionary<int, Transform>();
            // The vertices count
            int boneOffset = 0;

            // Get bones hierarchies from all skinned mesh
            for (int i = 0; i < skinnedMeshRenderers.Count; i++)
            {
                foreach (Transform t in skinnedMeshRenderers[i].bones)
                {
                    if (!originalBones.ContainsKey(t.GetInstanceID()))
                    {
                        originalBones.Add(t.GetInstanceID(), t);
                    }
                }
            }

            // Find the root bones
            Transform[] rootBones = FindRootBone(originalBones);
            for (int i = 0; i < rootBones.Length; i++)
            {
                // Instantiate the GameObject parent for this rootBone
                GameObject rootBoneParent = new GameObject("rootBone" + i);
                rootBoneParent.transform.position = rootBones[i].position;
                rootBoneParent.transform.parent = parent;
                rootBoneParent.transform.localPosition -= rootBones[i].localPosition;
                rootBoneParent.transform.localRotation = Quaternion.identity;

                // Instanciate a copy of the root bone
                GameObject newRootBone = InstantiateCopy(rootBones[i].gameObject);
                newRootBone.transform.position = rootBones[i].position;
                newRootBone.transform.rotation = rootBones[i].rotation;
                newRootBone.transform.parent = rootBoneParent.transform;
                newRootBone.AddComponent<MeshRenderer>();

                // Get the correspondancy map between original bones and new bones
                GetOrignialToNewBonesCorrespondancy(rootBones[i], newRootBone.transform, originToNewBoneMap);
            }

            // Copy Animator Controllers to new Combined GameObject
            foreach (Animator anim in parent.parent.GetComponentsInChildren<Animator>())
            {
                Transform[] children = anim.GetComponentsInChildren<Transform>();
                // Find the transform into which a copy of the Animator component will be added
                Transform t = FindTransformForAnimator(children, rootBones, anim);
                if (t != null)
                {
                    CopyAnimator(anim, originToNewBoneMap[t.GetInstanceID()].parent.gameObject);
                }
            }

            for (int i = 0; i < skinnedMeshRenderers.Count; i++)
            {
                // Get a snapshot of the skinnedMesh renderer 
                Mesh mesh = copyMesh(skinnedMeshRenderers[i].sharedMesh, skinnedMeshRenderers[i].GetInstanceID().ToString());
                _vertexOffset += mesh.vertexCount;
                verticesCount += skinnedMeshRenderers[i].sharedMesh.vertexCount;

                // Copy bone weights
                BoneWeight[] meshBoneweight = skinnedMeshRenderers[i].sharedMesh.boneWeights;
                foreach (BoneWeight bw in meshBoneweight)
                {
                    BoneWeight bWeight = bw;
                    bWeight.boneIndex0 += boneOffset;
                    bWeight.boneIndex1 += boneOffset;
                    bWeight.boneIndex2 += boneOffset;
                    bWeight.boneIndex3 += boneOffset;

                    boneWeights.Add(bWeight);
                }
                boneOffset += skinnedMeshRenderers[i].bones.Length;

                // Copy bones and bindposes
                Transform[] meshBones = skinnedMeshRenderers[i].bones;
                foreach (Transform bone in meshBones)
                {
                    bones.Add(originToNewBoneMap[bone.GetInstanceID()]);
                    bindposes.Add(bone.worldToLocalMatrix * parent.transform.localToWorldMatrix);
                }

                // Create the list of CombineInstance for this skinnedMesh
                Matrix4x4 matrix = parent.transform.worldToLocalMatrix * skinnedMeshRenderers[i].transform.localToWorldMatrix;
                combineInstances.AddRange(CreateCombinedInstances(mesh, skinnedMeshRenderers[i].sharedMaterials, skinnedMeshRenderers[i].GetInstanceID(), skinnedMeshRenderers[i].gameObject.name, matrix, combinedIndex));
            }

            if (combineInstances.Count() > 0)
            {
                // Create the combined GameObject which contains the combined meshes
                // Create the new GameObject
                GameObject combinedGameObject = CreateCombinedSkinnedMeshGameObject(combineInstances, parent, combinedGameObjectCount, combinedIndex);
                // Assign skinnedMesh parameters values
                SkinnedMeshRenderer sk = combinedGameObject.GetComponent<SkinnedMeshRenderer>();
                AssignParametersToSkinnedMesh(sk, bones, boneWeights, bindposes);
                combinedMeshes.Add(combinedGameObject);
            }

            return combinedMeshes;
        }

        // Assign the parameters of the new skinnedMesh
        private void AssignParametersToSkinnedMesh(SkinnedMeshRenderer skin, List<Transform> bones, List<BoneWeight> boneWeights, List<Matrix4x4> bindposes)
        {
            // Complete bone weights list if some are missing. BoneWeight is either empty or has the same quantity of elements than vertexCount
            if (boneWeights.Count > 0)
            {
                for (int i = boneWeights.Count; i < skin.sharedMesh.vertexCount; i++)
                {
                    boneWeights.Add(boneWeights[0]);
                }
            }

            skin.bones = bones.ToArray();
            //sk.rootBone = newRootBone.transform;
            skin.sharedMesh.boneWeights = boneWeights.ToArray();
            skin.sharedMesh.bindposes = bindposes.ToArray();
            skin.sharedMesh.RecalculateBounds();
            skin.sharedMesh.RecalculateNormals();

            bones.Clear();
            boneWeights.Clear();
            bindposes.Clear();
            _vertexOffset = 0;
        }

        // Copy the animator component to the transform
        private void CopyAnimator(Animator anim, GameObject target)
        {
            if (target.GetComponentsInChildren<Animator>().Length == 0)
            {
                Animator newAnimator = target.AddComponent(typeof(Animator)) as Animator;
                if (newAnimator != null)
                {
                    newAnimator.applyRootMotion = anim.applyRootMotion;
                    newAnimator.avatar = anim.avatar;
                    newAnimator.updateMode = anim.updateMode;
                    newAnimator.cullingMode = anim.cullingMode;
                    newAnimator.runtimeAnimatorController = anim.runtimeAnimatorController;
                }
            }
        }

        // Find the transform in which to instanciate the animator component
        private Transform FindTransformForAnimator(Transform[] children, Transform[] rootBones, Animator anim)
        {
            foreach (Transform t in children)
            {
                for (int i = 0; i < rootBones.Length; i++)
                {
                    if (t.Equals(rootBones[i]))
                    {
                        return rootBones[i];
                    }
                }
            }
            return null;
        }

        // Return a correspondancy map between original bones and the new bones
        private void GetOrignialToNewBonesCorrespondancy(Transform rootBone, Transform newRootBone, Dictionary<int, Transform> originToNewBoneMap)
        {
            Transform[] rootBoneTransforms = rootBone.GetComponentsInChildren<Transform>();
            Transform[] newRootBoneTransforms = newRootBone.GetComponentsInChildren<Transform>();
            // Get correspondancy between original bones and the new ones recently created
            for (int i = 0; i < newRootBoneTransforms.Length; i++)
            {
                if (!originToNewBoneMap.ContainsKey(rootBoneTransforms[i].GetInstanceID()))
                {
                    originToNewBoneMap.Add(rootBoneTransforms[i].GetInstanceID(), newRootBoneTransforms[i]);
                }
                else
                {
                    Logger.Instance.AddLog("SuperCombiner", " Found duplicated root bone: " + rootBoneTransforms[i], Logger.LogLevel.LOG_WARNING);
                }
            }
        }

        // Find the list of root bone from a hierachy of bones
        private Transform[] FindRootBone(Dictionary<int, Transform> bones)
        {
            List<Transform> rootBones = new List<Transform>();
            List<Transform> bonesList = new List<Transform>(bones.Values);
            if (bonesList.Count == 0)
            {
                return rootBones.ToArray();
            }
            Transform rootBone = bonesList.ToArray()[0];
            while (rootBone.parent != null)
            {
                if (bones.ContainsKey(rootBone.parent.GetInstanceID()))
                {
                    rootBone = rootBone.parent;
                }
                else
                {
                    rootBones.Add(rootBone.parent);
                    Transform[] children = rootBone.parent.GetComponentsInChildren<Transform>();
                    foreach (Transform t in children)
                    {
                        bones.Remove(t.GetInstanceID());
                        if (t != rootBone.parent && rootBones.Contains(t))
                        {
                            rootBones.Remove(t);
                        }
                    }
                    Transform[] otherBones = (new List<Transform>(bones.Values)).ToArray();
                    if (otherBones.Length > 0)
                    {
                        rootBone = otherBones[0];
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return rootBones.ToArray();
        }

        // Instantiate a copy of the GameObject, keeping it's transform values identical
        private GameObject InstantiateCopy(GameObject original)
        {
            GameObject copy = GameObject.Instantiate(original) as GameObject;
            copy.transform.parent = original.transform.parent;
            copy.transform.localPosition = original.transform.localPosition;
            copy.transform.localRotation = original.transform.localRotation;
            copy.transform.localScale = original.transform.localScale;
            copy.name = original.name;

            // Remove all SkinnedMeshRenderes that may be inside root hierarchy
            foreach (SkinnedMeshRenderer skin in copy.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                GameObject.DestroyImmediate(skin);
            }

            return copy;
        }

        /// <summary>
        /// Create a new combineInstance based on a new mesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="sharedMaterials"></param>
        /// <param name="instanceID"></param>
        /// <param name="name"></param>
        /// <param name="matrix"></param>
        /// <param name="combinedIndex"></param>
        /// <returns></returns>
        private CombineInstanceID CreateCombinedInstances(Mesh mesh, Material[] sharedMaterials, int instanceID, string name, Matrix4x4 matrix, int combinedIndex)
        {
            CombineInstanceID instances = new CombineInstanceID();
            int[] textureIndexes = new int[mesh.subMeshCount];
            for (int k = 0; k < mesh.subMeshCount; k++)
            {
                // Find corresponding _material for each submesh
                if (k < sharedMaterials.Length)
                {
                    Material mat = sharedMaterials[k];
                    textureIndexes[k] = _combinedResult.FindCorrespondingMaterialIndex(mat, combinedIndex);
                }
                else
                {
                    Logger.Instance.AddLog("SuperCombiner", " Mesh '" + mesh.name + "' has " + mesh.subMeshCount + " submeshes but only " + sharedMaterials.Length + " _material(s) assigned", Logger.LogLevel.LOG_WARNING);
                    break;
                }
            }

            // Update submesh count
            _combinedResult._subMeshCount += (mesh.subMeshCount - 1);

            // Generate new UVs only if there are more than 1 _material combined
            if (_combinedResult._originalMaterialList[combinedIndex].Count > 1)
            {
                GenerateUV(mesh, textureIndexes, _combinedResult._combinedMaterials[combinedIndex].scaleFactors.ToArray(), name, combinedIndex);
            }

            for (int k = 0; k < mesh.subMeshCount; k++)
            {
                instances.AddCombineInstance(k, mesh, matrix, instanceID, name);
            }

            return instances;
        }

        /// <summary>
        /// Create a new GameObject based on the CombineInstance list
        /// </summary>
        /// <param name="instances"></param>
        /// <param name="parent"></param>
        /// <param name="number"></param>
        /// <returns></returns>
		private GameObject CreateCombinedSkinnedMeshGameObject(CombineInstanceID instances, Transform parent, int number, int combinedIndex)
        {
            GameObject combined = new GameObject(_sessionName + number.ToString());
            SkinnedMeshRenderer skinnedMeshRenderer = combined.AddComponent<SkinnedMeshRenderer>();

            skinnedMeshRenderer.sharedMaterial = _combinedResult._combinedMaterials[combinedIndex].material;
            skinnedMeshRenderer.sharedMesh = new Mesh();
            skinnedMeshRenderer.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            skinnedMeshRenderer.sharedMesh.name = _sessionName + "_" + _combinedResult._combinedMaterials[combinedIndex].displayedIndex + "_mesh" + number;
            skinnedMeshRenderer.sharedMesh.CombineMeshes(instances._combineInstances.ToArray(), true, true);

#if UNITY_5_3_OR_NEWER
            // Add blendShapes to new skinnedMesh renderer if needed
            foreach (BlendShapeFrame blendShape in blendShapes.Values)
            {
                Vector3[] detlaVertices = new Vector3[skinnedMeshRenderer.sharedMesh.vertexCount];
                Vector3[] detlaNormals = new Vector3[skinnedMeshRenderer.sharedMesh.vertexCount];
                Vector3[] detlaTangents = new Vector3[skinnedMeshRenderer.sharedMesh.vertexCount];

                for (int p = 0; p < blendShape._deltaVertices.Length; p++)
                {
                    detlaVertices.SetValue(blendShape._deltaVertices[p], p + blendShape._vertexOffset);
                    detlaNormals.SetValue(blendShape._deltaNormals[p], p + blendShape._vertexOffset);
                    detlaTangents.SetValue(blendShape._deltaTangents[p], p + blendShape._vertexOffset);
                }

                skinnedMeshRenderer.sharedMesh.AddBlendShapeFrame(blendShape._shapeName, blendShape._frameWeight, detlaVertices, detlaNormals, detlaTangents);
            }
#endif

#if UNITY_EDITOR
            MeshUtility.Optimize(skinnedMeshRenderer.sharedMesh);
#endif
            combined.transform.SetParent(parent);
            combined.transform.localPosition = Vector3.zero;

            _combinedResult._totalVertexCount += skinnedMeshRenderer.sharedMesh.vertexCount;
            _combinedResult.AddCombinedMesh(skinnedMeshRenderer.sharedMesh, instances, combinedIndex);
            return combined;
        }

        /// <summary>
        /// Create a new GameObject based on the CombineInstance list.
        /// Set its MeshFilter and MeshRenderer to the new combined Meshe/Material
        /// </summary>
        /// <param name="instances"></param>
        /// <param name="parent"></param>
        /// <param name="number"></param>
        /// <returns></returns>
		public GameObject CreateCombinedMeshGameObject(CombineInstanceID instances, Transform parent, int number, int combinedIndex)
        {
            GameObject combined;
            MeshFilter meshFilter;
            MeshRenderer meshRenderer;

            // If parent has components MeshFilters and MeshRenderers, replace meshes and materials
            if (number == 0 && parent.GetComponent<MeshFilter>() != null && parent.GetComponent<MeshRenderer>() != null)
            {
                combined = parent.gameObject;
                meshFilter = parent.GetComponent<MeshFilter>();
                meshRenderer = parent.GetComponent<MeshRenderer>();
            }
            else
            {
                combined = new GameObject(_sessionName + "_" + _combinedResult._combinedMaterials[combinedIndex].displayedIndex + "_" + number.ToString());
                meshFilter = combined.AddComponent<MeshFilter>();
                meshRenderer = combined.AddComponent<MeshRenderer>();
                combined.transform.SetParent(parent);
                combined.transform.localPosition = Vector3.zero;
            }

            meshRenderer.sharedMaterial = _combinedResult._combinedMaterials[combinedIndex].material;
            meshFilter.mesh = new Mesh();
            meshFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            meshFilter.sharedMesh.name = _sessionName + "_" + _combinedResult._combinedMaterials[combinedIndex].displayedIndex + "_mesh" + number;
            meshFilter.sharedMesh.CombineMeshes(instances._combineInstances.ToArray());
#if UNITY_EDITOR
            MeshUtility.Optimize(meshFilter.sharedMesh);
            if (_generateUv2)
            {
                Unwrapping.GenerateSecondaryUVSet(meshFilter.sharedMesh);
            }
#endif

            _combinedResult._totalVertexCount += meshFilter.sharedMesh.vertexCount;
            _combinedResult.AddCombinedMesh(meshFilter.sharedMesh, instances, combinedIndex);
            return combined;
        }

        // Generate the new transformed gameobjects and apply new materials to them
        public bool GenerateUV(Mesh targetMesh, int[] textureIndex, float[] scaleFactors, string objectName, int combinedIndex)
        {
            int subMeshCount = targetMesh.subMeshCount;
            if (subMeshCount > textureIndex.Length)
            {
                Logger.Instance.AddLog("SuperCombiner", "GameObject '" + objectName + "' has submeshes with no _material assigned", Logger.LogLevel.LOG_WARNING);
                subMeshCount = textureIndex.Length;
            }
            Logger.Instance.AddLog("SuperCombiner", "Processing '" + objectName + "'...", Logger.LogLevel.LOG_DEBUG, false);

            Vector2[] uv = (Vector2[])(targetMesh.uv);

            if (uv.Length <= 0)
            {
#if UNITY_EDITOR
                // The mesh does not have UVs, so we try to unwrap the UV's
                Logger.Instance.AddLog("SuperCombiner", "Object " + objectName + " doesn't have uv, SuperCombiner will try to unwrap it's uvs but the result may be incorrect. In order to avoid this potential issue, add uv map to this mesh with a 3d modeler tool.", Logger.LogLevel.LOG_WARNING);
                Vector2[] uvTemp = Unwrapping.GeneratePerTriangleUV(targetMesh);
                uv = new Vector2[targetMesh.vertexCount];
                for (int i = 0; i < targetMesh.vertexCount; i++)
                {
                    uv[i] = uvTemp[i];
                }
                targetMesh.uv = new Vector2[uv.Length];
#endif
            }

            Vector2[] uv2 = (Vector2[])(targetMesh.uv2);
            Vector2[] new_uv = new Vector2[uv.Length];
            Vector2[] new_uv2 = new Vector2[uv2.Length];
            Rect[] uvsInAtlasTexture = new Rect[subMeshCount];

            if (new_uv.Length > 0)
            {
                for (int i = 0; i < subMeshCount; i++)
                {
                    // Get the list of triangles for the current submesh
                    int[] subMeshTriangles = targetMesh.GetTriangles(i);

                    if (textureIndex[i] < _combinedResult._combinedMaterials[combinedIndex].uvs.Length)
                    {
                        uvsInAtlasTexture[i] = _combinedResult._combinedMaterials[combinedIndex].uvs[textureIndex[i]];

                        // Target UV calculation, taking into account main map's scale and offset of the original _material
                        Rect targetUV = new Rect(uvsInAtlasTexture[i].position, uvsInAtlasTexture[i].size);

                        float factor = scaleFactors[textureIndex[i]];
                        if (factor > 1)
                        {
                            targetUV.size = Vector2.Scale(targetUV.size, Vector2.one / factor);
                            targetUV.position += new Vector2(uvsInAtlasTexture[i].width * (1 - 1 / factor) / 2f, uvsInAtlasTexture[i].height * (1 - 1 / factor) / 2f);
                        }

                        float xMin = _combinedResult._combinedMaterials[combinedIndex].meshUVBounds[textureIndex[i]].xMin;
                        float yMin = _combinedResult._combinedMaterials[combinedIndex].meshUVBounds[textureIndex[i]].yMin;
                        float width = _combinedResult._combinedMaterials[combinedIndex].meshUVBounds[textureIndex[i]].width;
                        float height = _combinedResult._combinedMaterials[combinedIndex].meshUVBounds[textureIndex[i]].height;

                        for (int j = 0; j < subMeshTriangles.Length; j++)
                        {
                            int uvIndex = subMeshTriangles[j];

                            new_uv[uvIndex] = uv[uvIndex];

                            // Translate new mesh's uvs so that minimun is at coordinates (0, 0)
                            new_uv[uvIndex].x -= xMin;
                            new_uv[uvIndex].y -= yMin;

                            // Scale (if necessary) new mesh's uvs so that it fits in a (1, 1) square
                            if (width != 0 && width != 1)
                            {
                                new_uv[uvIndex].Scale(new Vector2(1 / width, 1));
                            }
                            if (height != 0 && height != 1)
                            {
                                new_uv[uvIndex].Scale(new Vector2(1, 1 / height));
                            }

                            // Scale and translate new uvs to fit the correct texture in the atlas
                            new_uv[uvIndex].Scale(targetUV.size);
                            new_uv[uvIndex] += targetUV.position;
                        }
                    }
                    else
                    {
                        Logger.Instance.AddLog("SuperCombiner", "Texture _index exceed packed texture size", Logger.LogLevel.LOG_ERROR);
                    }
                }
            }
            else
            {
                Logger.Instance.AddLog("SuperCombiner", "Object " + objectName + " doesn't have uv, combine process may be incorrect. Add uv map with a 3d modeler tool.", Logger.LogLevel.LOG_WARNING);
            }

            // Assign new uv
            targetMesh.uv = new_uv;

            // Lightmap
            if (_generateUv2)
            {
                if (uv2 != null && uv2.Length > 0 && _combinedResult._combinedMaterials[combinedIndex].uvs2 != null && _combinedResult._combinedMaterials[combinedIndex].uvs2.Length > 0)
                {
                    for (int l = 0; l < uv2.Length; l++)
                    {
                        new_uv2[uv2.Length + l] = new Vector2((uv2[l].x * _combinedResult._combinedMaterials[combinedIndex].uvs2[textureIndex[0]].width) + _combinedResult._combinedMaterials[combinedIndex].uvs2[textureIndex[0]].x, (uv2[l].y * _combinedResult._combinedMaterials[combinedIndex].uvs2[textureIndex[0]].height) + _combinedResult._combinedMaterials[combinedIndex].uvs2[textureIndex[0]].y);
                    }
                    targetMesh.uv2 = new_uv2;
                }
                else
                {
                    // target mesh doesn't have uv2
                }
            }
            return true;
        }

        // Copy a Mesh into a new instance
        public Mesh copyMesh(Mesh mesh, string id = "")
        {
            Mesh copy = new Mesh();
            copy.indexFormat = mesh.indexFormat;    // Carefull to set indexFormat before setting subMeshCount!
            copy.subMeshCount = mesh.subMeshCount;
            copy.vertices = mesh.vertices;
            copy.normals = mesh.normals;
            copy.uv = mesh.uv;
            copy.uv2 = mesh.uv2;
            copy.uv3 = mesh.uv3;
            copy.uv4 = mesh.uv4;
            copy.tangents = mesh.tangents;
            copy.bindposes = mesh.bindposes;
            copy.boneWeights = mesh.boneWeights;
            copy.bounds = mesh.bounds;
            copy.colors32 = mesh.colors32;
            copy.name = mesh.name;
            copy.subMeshCount = mesh.subMeshCount;

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                copy.SetIndices(mesh.GetIndices(i), mesh.GetTopology(i), i);
                copy.SetTriangles(mesh.GetTriangles(i), i);
            }

#if UNITY_5_3_OR_NEWER
            // Blendshape management
            if (mesh.blendShapeCount > 0)
            {
                Vector3[] deltaVertices = new Vector3[mesh.vertexCount];
                Vector3[] deltaNormals = new Vector3[mesh.vertexCount];
                Vector3[] deltaTangents = new Vector3[mesh.vertexCount];
                for (int s = 0; s < mesh.blendShapeCount; s++)
                {
                    for (int f = 0; f < mesh.GetBlendShapeFrameCount(s); f++)
                    {
                        if (!blendShapes.ContainsKey(mesh.GetBlendShapeName(s) + id))
                        {
                            // Copy blendShape to the new mesh
                            mesh.GetBlendShapeFrameVertices(s, f, deltaVertices, deltaNormals, deltaTangents);
                            copy.AddBlendShapeFrame(
                                mesh.GetBlendShapeName(s),
                                mesh.GetBlendShapeFrameWeight(s, f),
                                deltaVertices, deltaNormals, deltaTangents
                            );
                            // Add this blendShape to the list
                            blendShapes.Add(mesh.GetBlendShapeName(s) + id, new BlendShapeFrame(mesh.GetBlendShapeName(s) + id, mesh.GetBlendShapeFrameWeight(s, f), deltaVertices, deltaNormals, deltaTangents, _vertexOffset));
                        }
                    }
                }
            }
#endif
            return copy;
        }

        // Copy a new mesh and assign it to destination
        private void CopyNewMeshesByCombine(Mesh original, Mesh destination)
        {
            int subMeshCount = original.subMeshCount;
            CombineInstance[] combineInstances = new CombineInstance[subMeshCount];
            for (int j = 0; j < subMeshCount; j++)
            {
                combineInstances[j] = new CombineInstance();
                combineInstances[j].subMeshIndex = j;
                combineInstances[j].mesh = original;
                combineInstances[j].transform = Matrix4x4.identity;
            }

            destination.CombineMeshes(combineInstances, false);
        }

        public GameObject CombineMeshToSubmeshes(List<Mesh> meshes, MeshOutput output)
        {
            int vertexOffset = 0;
            List<int[]> indices = new List<int[]>();
            List<int[]> triangles = new List<int[]>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3[]> verticesArray = new List<Vector3[]>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector4> tangents = new List<Vector4>();
            List<Color32> colors = new List<Color32>();
            List<Vector2> uv0 = new List<Vector2>();
            List<Vector2> uv2 = new List<Vector2>();

            for (int i = 0; i < meshes.Count; i++)
            {
                // Indices
                indices.Add(meshes[i].GetIndices(0));
                // Triangles
                int[] trianglesTmp = meshes[i].GetTriangles(0);
                for (int j = 0; j < trianglesTmp.Length; j++)
                {
                    trianglesTmp[j] = trianglesTmp[j] + vertexOffset;
                }
                triangles.Add(trianglesTmp);
                // Vertices
                List<Vector3> vertexTmp = new List<Vector3>();
                meshes[i].GetVertices(vertexTmp);
                vertices.AddRange(vertexTmp);
                vertexOffset += vertexTmp.Count;
                // Normals
                List<Vector3> normalsTmp = new List<Vector3>();
                meshes[i].GetNormals(normalsTmp);
                normals.AddRange(normalsTmp);
                // Tangents
                List<Vector4> tangentsTmp = new List<Vector4>();
                meshes[i].GetTangents(tangentsTmp);
                tangents.AddRange(tangentsTmp);
                // Colors
                List<Color32> colorsTmp = new List<Color32>();
                meshes[i].GetColors(colorsTmp);
                colors.AddRange(colorsTmp);
                // UVs
                List<Vector2> uvTmp = new List<Vector2>();
                meshes[i].GetUVs(0, uvTmp);
                uv0.AddRange(uvTmp);
                meshes[i].GetUVs(1, uvTmp); // channel 1 is uv2
                uv2.AddRange(uvTmp);
            }

            GameObject go = new GameObject(_sessionName);

            Mesh mesh = new Mesh();
            if (output == LunarCatsStudio.SuperCombiner.MeshOutput.Mesh)
            {
                MeshFilter meshFilter = go.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = mesh;
            }
            else
            {
                SkinnedMeshRenderer skinnedMeshRenderer = go.AddComponent<SkinnedMeshRenderer>();
                skinnedMeshRenderer.sharedMesh = mesh;
            }

            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.name = _sessionName + "_mesh";

            mesh.subMeshCount = _combinedResult.GetMaterialGroupCount();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
            mesh.SetColors(colors);
            mesh.SetUVs(0, uv0);
            mesh.SetUVs(1, uv2);

            for (int i = 0; i < indices.Count; i++)
            {
                mesh.SetIndices(indices[i], mesh.GetTopology(0), i);
                mesh.SetTriangles(triangles[i], i);
            }

            mesh.RecalculateBounds();

#if UNITY_EDITOR
            MeshUtility.Optimize(mesh);
#endif

            if (output == LunarCatsStudio.SuperCombiner.MeshOutput.Mesh)
            {
                MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterials = _combinedResult.GetCombinedMaterials().ToArray();
            }
            else
            {
                SkinnedMeshRenderer skinnedmeshRenderer = go.GetComponent<SkinnedMeshRenderer>();
                skinnedmeshRenderer.sharedMaterials = _combinedResult.GetCombinedMaterials().ToArray();
            }

            return go;
        }
    }
}