using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System;

namespace LunarCatsStudio.SuperCombiner
{
    /// <summary>
    /// Saver.
    /// This class is responsible for saving the combined resources in disk
    /// </summary>
    public class Saver
    {
#if UNITY_EDITOR
        /// <summary>
        /// Saves the textures.
        /// </summary>
        /// <param name="combinedIndex">Combined _index.</param>
        /// <param name="folderDestination">Folder destination.</param>
        /// <param name="sessionName">Session name.</param>
        /// <param name="texturePacker">Texture packer.</param>
        public static void SaveTextures(int combinedIndex, string folderDestination, string sessionName, TexturePacker texturePacker)
        {
            // UI Progress bar display in Editor
            EditorUtility.DisplayProgressBar("Super Combiner", "Saving textures ...", 0.3f);

            if (!Directory.Exists(folderDestination + "/Textures"))
            {
                Directory.CreateDirectory(folderDestination + "/Textures");
            }

            texturePacker.SaveTextures(folderDestination, sessionName);

            Logger.Instance.AddLog("SuperCombiner", "Textures saved in '" + folderDestination + "/Textures/'");
        }

        /// <summary>
        /// Saves the _material.
        /// </summary>
        /// <param name="combinedIndex">Combined _index.</param>
        public static void SaveMaterial(int combinedIndex, string folderDestination, string sessionName, TexturePacker texturePacker)
        {
            // UI Progress bar display in Editor
            EditorUtility.DisplayProgressBar("Super Combiner", "Saving materials ...", 0.6f);

            if (!Directory.Exists(folderDestination + "/Materials"))
            {
                Directory.CreateDirectory(folderDestination + "/Materials");
            }

            Material materialToSave = AssetDatabase.LoadAssetAtPath<Material>(folderDestination + "/Materials/" + texturePacker._copyedMaterials.name + ".mat");

            if (materialToSave == null)
            {
                AssetDatabase.CreateAsset(texturePacker._copyedToSaveMaterials, folderDestination + "/Materials/" + texturePacker._copyedMaterials.name + ".mat");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                EditorUtility.CopySerialized(texturePacker._copyedToSaveMaterials, materialToSave);
            }

            Material material = (Material)(AssetDatabase.LoadAssetAtPath(folderDestination + "/Materials/" + texturePacker._copyedMaterials.name + ".mat", typeof(Material)));
            foreach (KeyValuePair<string, Texture2D> keyValue in texturePacker._packedTextures)
            {
                if (keyValue.Value != null)
                {
                    string textureName = keyValue.Key;
                    texturePacker.TexturePropertyNames.TryGetValue(keyValue.Key, out textureName);
                    material.SetTexture(keyValue.Key, (Texture2D)(AssetDatabase.LoadAssetAtPath(texturePacker.GetTextureFilePathName(folderDestination, sessionName, textureName, combinedIndex), typeof(Texture2D))));
                }
            }

            Logger.Instance.AddLog("SuperCombiner", "Materials saved in '" + folderDestination + "/Materials/'");
        }

        /// <summary>
        /// Saves the prefabs.
        /// </summary>
        public static void SavePrefabs(List<GameObject> toSavePrefabList, List<Mesh> toSaveMeshList, string folderDestination, string sessionName)
        {
            // UI Progress bar display in Editor
            EditorUtility.DisplayProgressBar("Super Combiner", "Saving Prefabs ...", 0.7f);

            if (!Directory.Exists(folderDestination + "/Prefabs"))
            {
                Directory.CreateDirectory(folderDestination + "/Prefabs");
            }

            // We have to save meshes first
            SaveMeshes(toSaveMeshList, folderDestination, sessionName);

            for (int i = 0; i < toSavePrefabList.Count; i++)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(folderDestination + "/Prefabs/" + sessionName + "_" + toSavePrefabList[i].name + ".prefab"))
                {
                    // Asset already exist! We just have to update it
                    GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(folderDestination + "/Prefabs/" + sessionName + "_" + toSavePrefabList[i].name + ".prefab");

                    asset.transform.position = toSavePrefabList[i].transform.position;
                    asset.transform.localScale = toSavePrefabList[i].transform.localScale;
                    asset.transform.rotation = toSavePrefabList[i].transform.rotation;

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                else
                {
                    // This is a new asset, create it
                    PrefabUtility.SaveAsPrefabAsset(toSavePrefabList[i], folderDestination + "/Prefabs/" + sessionName + "_" + toSavePrefabList[i].name + ".prefab");
                }
            }

            Logger.Instance.AddLog("SuperCombiner", "Prefabs saved in '" + folderDestination + "/Prefabs/'");
        }

        /// <summary>
        /// Saves the meshes.
        /// </summary>
        public static void SaveMeshes(List<Mesh> toSaveMeshList, string folderDestination, string sessionName)
        {
            // UI Progress bar display in Editor
            EditorUtility.DisplayProgressBar("Super Combiner", "Saving Meshes ...", 0.75f);

            if (!Directory.Exists(folderDestination + "/Meshes"))
            {
                Directory.CreateDirectory(folderDestination + "/Meshes");
            }

            // Check if all meshes have different name. This is important not to override a previously saved mesh
            HashSet<string> meshNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < toSaveMeshList.Count; i++)
            {
                if (!meshNames.Contains(toSaveMeshList[i].name))
                {
                    meshNames.Add(toSaveMeshList[i].name);
                }
                else
                {
                    // A mesh with the same name has been found, rename it
                    for (int n = 0; n < 9999; n++)
                    {
                        if (!meshNames.Contains(toSaveMeshList[i].name + "(" + n + ")"))
                        {
                            meshNames.Add(toSaveMeshList[i].name + "(" + n + ")");
                            toSaveMeshList[i].name = toSaveMeshList[i].name + "(" + n + ")";
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < toSaveMeshList.Count; i++)
            {
                Mesh dummy = AssetDatabase.LoadAssetAtPath<Mesh>(folderDestination + "/Meshes/" + sessionName + "_" + toSaveMeshList[i].name + ".asset");
                if (dummy == null)
                {
                    // This is a new mesh to create
                    AssetDatabase.CreateAsset(toSaveMeshList[i], folderDestination + "/Meshes/" + sessionName + "_" + toSaveMeshList[i].name + ".asset");
                }
                else
                {
                    // The mesh already exists, just uptade it
                    dummy.Clear();
                    EditorUtility.CopySerialized(toSaveMeshList[i], dummy);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Logger.Instance.AddLog("SuperCombiner", "Meshes saved in '" + folderDestination + "/Meshes/'");
        }

        /// <summary>
        /// Saves the meshes object.
        /// </summary>
        public static void SaveMeshesObj(List<GameObject> combinedGameObjectFromMeshList, List<GameObject> combinedGameObjectFromSkinnedMeshList, string folderDestination)
        {
            // UI Progress bar display in Editor
            EditorUtility.DisplayProgressBar("Super Combiner", "Saving obj ...", 0.8f);

            if (!Directory.Exists(folderDestination + "/Objs"))
            {
                Directory.CreateDirectory(folderDestination + "/Objs");
            }

            for (int i = 0; i < combinedGameObjectFromMeshList.Count; i++)
            {
                LunarCatsStudio.SuperCombiner.ObjSaver.SaveObjFile(combinedGameObjectFromMeshList[i], false, folderDestination + "/Objs");
            }
            for (int i = 0; i < combinedGameObjectFromSkinnedMeshList.Count; i++)
            {
                LunarCatsStudio.SuperCombiner.ObjSaver.SaveObjFile(combinedGameObjectFromSkinnedMeshList[i], false, folderDestination + "/Objs");
            }
        }

        /// <summary>
        /// Saves the meshes fbx.
        /// </summary>
        public static void SaveMeshesFbx(string folderDestination)
        {
            // UI Progress bar display in Editor
            EditorUtility.DisplayProgressBar("Super Combiner", "Saving fbx ...", 0.9f);

            if (!Directory.Exists(folderDestination + "/Fbx"))
            {
                Directory.CreateDirectory(folderDestination + "/Fbx");
            }

            // TODO : save mesh to fbx !
        }

        /// <summary>
        /// Saves the combined results.
        /// </summary>
        public static void SaveCombinedResults(CombinedResult combinedResult, string folderDestination, string sessionName)
        {
            CombinedResult savedCombinedResult = AssetDatabase.LoadAssetAtPath<CombinedResult>(folderDestination + "/CombinedResults_" + sessionName + ".asset");
            if (savedCombinedResult != null)
            {
                AssetDatabase.DeleteAsset(folderDestination + "/CombinedResults_" + sessionName + ".asset");
            }

            CombinedResult toSaveCombinedResult = GameObject.Instantiate(combinedResult) as CombinedResult;

            for (int i = 0; i < combinedResult._combinedMaterials.Count; i++)
            {
                if (combinedResult._combinedMaterials[i].material != null)
                {
                    toSaveCombinedResult._combinedMaterials[i].material = (Material)(AssetDatabase.LoadAssetAtPath(folderDestination + "/Materials/" + combinedResult._combinedMaterials[i].material.name + ".mat", typeof(Material)));
                }
            }
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(folderDestination + "/CombinedResults_" + sessionName + ".asset");

            AssetDatabase.CreateAsset(toSaveCombinedResult, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Saves the combined settings.
        /// </summary>
        public static void SaveCombinedSettings(SuperCombinerSettings combinedSettings, string folderDestination, string sessionName)
        {
            SuperCombinerSettings savedCombinedSettings = AssetDatabase.LoadAssetAtPath<SuperCombinerSettings>(folderDestination + "/CombinedSettings_" + sessionName + ".asset");
            if (savedCombinedSettings != null)
            {
                AssetDatabase.DeleteAsset(folderDestination + "/CombinedSettings_" + sessionName + ".asset");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(folderDestination + "/CombinedSettings_" + sessionName + ".asset");

            SuperCombinerSettings toSaveCombinedSettings = GameObject.Instantiate(combinedSettings) as SuperCombinerSettings;
            toSaveCombinedSettings.generalSettings = combinedSettings.generalSettings;
            toSaveCombinedSettings.textureSettings = combinedSettings.textureSettings;
            toSaveCombinedSettings.meshSettings = combinedSettings.meshSettings;
            toSaveCombinedSettings.materialSettings = combinedSettings.materialSettings;


            AssetDatabase.CreateAsset(toSaveCombinedSettings, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
    }
}
