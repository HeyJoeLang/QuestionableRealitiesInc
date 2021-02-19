using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace LunarCatsStudio.SuperCombiner 
{
	/// <summary>
	/// Combined result editor.
	/// </summary>
	[CustomEditor(typeof(CombinedResult))]
	public class CombinedResultEditor : Editor {

        Vector2 pos_log;


        // Reference to the SuperCombiner script
        private CombinedResult _combinedResult;

		/// <summary>
		/// Raises the enable event.
		/// </summary>
		private void OnEnable()
		{
			_combinedResult = (CombinedResult)target;
		}

		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI()
		{
            GUI.enabled = false;
            DisplayStats();
            DisplayCombinedMaterial();
            DisplayCombinedMeshResults();
            GUI.enabled = true;

            DisplayLogs();
        }

        /// <summary>
        /// Display the combined mesh result section
        /// </summary>
        private void DisplayCombinedMeshResults()
        {
            GUILayout.Label("Combined meshes", EditorStyles.whiteBoldLabel);
            if(_combinedResult._meshResults.Count > 0)
            {
                _combinedResult._showCombinedMeshes = EditorGUILayout.Foldout(_combinedResult._showCombinedMeshes, "Combined meshes (" + _combinedResult._meshResults.Count + ")");
                if (_combinedResult._showCombinedMeshes)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    for (int i = 0; i < _combinedResult._meshResults.Count; i++)
                    {
                        _combinedResult._meshResults[i].showMeshCombined = EditorGUILayout.Foldout(_combinedResult._meshResults[i].showMeshCombined, "Combined mesh " + i + " (" + _combinedResult._meshResults[i].names.Count + ")");
                        if(_combinedResult._meshResults[i].showMeshCombined)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            for (int j = 0; j < _combinedResult._meshResults[i].names.Count; j++)
                            {
                                _combinedResult._meshResults[i].indexes[j].showCombinedInstanceIndex = EditorGUILayout.Foldout(_combinedResult._meshResults[i].indexes[j].showCombinedInstanceIndex, "source object " + j + ": " + _combinedResult._meshResults[i].names[j] + "");
                                if(_combinedResult._meshResults[i].indexes[j].showCombinedInstanceIndex)
                                {
                                    EditorGUILayout.TextField("name", _combinedResult._meshResults[i].names[j]);
                                    EditorGUILayout.IntField("instance Id", _combinedResult._meshResults[i].instanceIds[j]);
                                    EditorGUILayout.IntField("first vertex _index", _combinedResult._meshResults[i].indexes[j].firstVertexIndex);
                                    EditorGUILayout.IntField("vertex count", _combinedResult._meshResults[i].indexes[j].vertexCount);
                                    EditorGUILayout.IntField("first triangle _index", _combinedResult._meshResults[i].indexes[j].firstTriangleIndex);
                                    EditorGUILayout.IntField("triangle count", _combinedResult._meshResults[i].indexes[j].triangleCount);
                                }
                            }
                            EditorGUILayout.EndVertical();
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
            } else
            {
                GUILayout.Label("No mesh were combined", EditorStyles.wordWrappedLabel);
            }
        }

        /// <summary>
        /// Display the general stat section
        /// </summary>
        private void DisplayStats()
        {
            // Display settings sections
            GUILayout.Label("General information", EditorStyles.whiteBoldLabel);
            EditorGUILayout.LabelField(_combinedResult._materialCombinedCount + " materials were combined");
            EditorGUILayout.LabelField(_combinedResult._meshesCombinedCount + " meshes were combined");
            EditorGUILayout.LabelField(_combinedResult._skinnedMeshesCombinedCount + " skinnedMeshes were combined");
            EditorGUILayout.LabelField(_combinedResult._subMeshCount + " subMeshes were found");
            EditorGUILayout.LabelField(_combinedResult._totalVertexCount + " vertices where combined");
            //EditorGUILayout.LabelField("All combined in " + _combinedResult._duration);

            EditorGUILayout.Space();
        }

        /// <summary>
        /// Display combine process logs
        /// </summary>
        private void DisplayLogs()
        {
            // logs sections
            GUI.enabled = false;

            _combinedResult._showLogs = EditorGUILayout.Foldout(_combinedResult._showLogs, "Combine Process Logs");
            GUI.enabled = true;

            if (_combinedResult._showLogs)
            {
                pos_log = EditorGUILayout.BeginScrollView(pos_log, true, true);
                GUI.enabled = false;
                
                // Carefull here if logs are too long
                EditorGUILayout.TextArea(_combinedResult._logs, GUILayout.ExpandHeight(true));
                GUI.enabled = true;

                EditorGUILayout.EndScrollView();
            }
            
            EditorGUILayout.Space();
        }

        /// <summary>
        /// Display the combined _material section
        /// </summary>
        private void DisplayCombinedMaterial()
        {
            GUILayout.Label("Combined _material(s)", EditorStyles.whiteBoldLabel);
            _combinedResult._showCombinedMaterials = EditorGUILayout.Foldout(_combinedResult._showCombinedMaterials, "Combined materials (" + _combinedResult._combinedMaterialCount + ")");
            if (_combinedResult._showCombinedMaterials)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                for (int i = 0; i < _combinedResult._combinedMaterials.Count; i++)
                {
                    if (_combinedResult._combinedMaterials[i].material != null)
                    {
                        _combinedResult._combinedMaterials[i].showCombinedMaterial = EditorGUILayout.Foldout(_combinedResult._combinedMaterials[i].showCombinedMaterial, "Combined _material " + _combinedResult._combinedMaterials[i].displayedIndex);
                        if (_combinedResult._combinedMaterials[i].showCombinedMaterial)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            // Show combined _material
                            EditorGUILayout.ObjectField("Material " + _combinedResult._combinedMaterials[i].displayedIndex, _combinedResult._combinedMaterials[i].material, typeof(Material), true);
                            // Show UVs
                            _combinedResult._combinedMaterials[i].showUVs = EditorGUILayout.Foldout(_combinedResult._combinedMaterials[i].showUVs, new GUIContent("UVs (" + _combinedResult._combinedMaterials[i].uvs.Length + ")", "Each UV rectangle below correspond to a specific location in the altas texture"));
                            if (_combinedResult._combinedMaterials[i].showUVs)
                            {
                                for (int j = 0; j < _combinedResult._combinedMaterials[i].uvs.Length; j++)
                                {
                                    EditorGUILayout.RectField("uv[" + j + "]", _combinedResult._combinedMaterials[i].uvs[j]);
                                }
                            }
                            // Show mesh uv bounds
                            _combinedResult._combinedMaterials[i].showMeshUVBounds = EditorGUILayout.Foldout(_combinedResult._combinedMaterials[i].showMeshUVBounds, new GUIContent("Mesh uv bounds (" + _combinedResult._combinedMaterials[i].meshUVBounds.Count + ")", "Each rectangle below correspond to the UV bound of the original mesh. This helps to see if meshes has UVs out of [0, 1] bounds."));
                            if (_combinedResult._combinedMaterials[i].showMeshUVBounds)
                            {
                                for (int j = 0; j < _combinedResult._combinedMaterials[i].meshUVBounds.Count; j++)
                                {
                                    EditorGUILayout.RectField("bound[" + j + "]", _combinedResult._combinedMaterials[i].meshUVBounds[j]);
                                }
                            }
                            EditorGUILayout.EndVertical();
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}
