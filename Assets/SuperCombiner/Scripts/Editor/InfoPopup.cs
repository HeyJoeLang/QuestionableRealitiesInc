using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEditor;


namespace LunarCatsStudio.SuperCombiner
{
    /// <summary>
    /// 
    /// </summary>
    public class InfoPopup : EditorWindow
    {

        public List<string> text = new List<string>();

        private Vector2 _scrollPosition = Vector2.zero;

        static void init()
        {

        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Warnings:", EditorStyles.boldLabel);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MinHeight(180));
            foreach (string message in text)
            {
                //EditorGUILayout.ObjectField()
                EditorGUILayout.HelpBox("coucou", MessageType.None);
                GUILayout.Box(message, GUILayout.ExpandWidth(true));
                EditorGUILayout.HelpBox("coucou", MessageType.Info);
                EditorGUILayout.HelpBox("coucou", MessageType.Error);
                EditorGUILayout.HelpBox("coucou", MessageType.Warning);
            }

            EditorGUILayout.EndScrollView();
            //EditorGUILayout.LabelField(text);
            GUILayout.Space(10);
            if(GUILayout.Button("Close", GUILayout.MinHeight(25)))
            {
                this.Close();
            }
            EditorGUILayout.EndVertical();


            // Plusieurs materials avec des propiétés différentes
        }
    }
}