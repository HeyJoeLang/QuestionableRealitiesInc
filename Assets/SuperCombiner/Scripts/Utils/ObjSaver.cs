﻿using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

namespace LunarCatsStudio.SuperCombiner
{
    /// <summary>
    /// This class is responsible for saving meshes in .obj file
    /// </summary>
	public class ObjSaver {

		// Used for exporting to .obj
		public static int _StartIndex = 0;

		public static void SaveObjFile(GameObject obj, bool makeSubmeshes, string floderPath)
		{	
			string meshName = obj.name;
			string fileName = floderPath + "/" + meshName + ".obj";

			_StartIndex = 0;

			StringBuilder meshString = new StringBuilder();		
			meshString.Append("#" + meshName + ".obj"
				+ "\n#" + System.DateTime.Now.ToLongDateString() 
				+ "\n#" + System.DateTime.Now.ToLongTimeString()
				+ "\n#-------" 
				+ "\n\n");

			Transform t = obj.transform;
			Vector3 originalPosition = t.position;
			t.position = Vector3.zero;

			if (!makeSubmeshes) {
				meshString.Append("g ").Append(t.name).Append("\n");
			}
			meshString.Append(processTransform(t, makeSubmeshes));
			WriteToFile(meshString.ToString(),fileName);
			t.position = originalPosition;

			_StartIndex = 0;
		}

		static void WriteToFile(string s, string filename)
		{
			using (StreamWriter sw = new StreamWriter(filename)) 
			{
				sw.Write(s);
			}
		}

		static string processTransform(Transform t, bool makeSubmeshes)
		{
			StringBuilder meshString = new StringBuilder();

			meshString.Append("#" + t.name
				+ "\n#-------" 
				+ "\n");

			if (makeSubmeshes)
			{
				meshString.Append("g ").Append(t.name).Append("\n");
			}

			MeshFilter mf = t.GetComponent<MeshFilter>();
			SkinnedMeshRenderer skinned = t.GetComponent<SkinnedMeshRenderer>();
			if (mf)
			{
				meshString.Append(MeshToString(mf.sharedMesh, mf.GetComponent<Renderer>().sharedMaterials, t));
			}
			if (skinned) 
			{
				meshString.Append(MeshToString(skinned.sharedMesh, skinned.sharedMaterials , t));
			}

			for(int i = 0; i < t.childCount; i++)
			{
				meshString.Append(processTransform(t.GetChild(i), makeSubmeshes));
			}

			return meshString.ToString();
		}

		public static string MeshToString(Mesh m, Material[] mats, Transform t) 
		{	
			/*Vector3 s 		= t.localScale;
			Vector3 p 		= t.localPosition;*/
			Quaternion r 	= t.localRotation;	

			int numVertices = 0;
			if (!m)
			{
				return "####Error####";
			}
			StringBuilder sb = new StringBuilder();

			foreach(Vector3 vv in m.vertices)
			{
				Vector3 v = t.TransformPoint(vv);
				numVertices++;
                // inverting x-component since we're in a different coordinate system than "everyone" is "used to".
                sb.Append(string.Format("v {0} {1} {2}\n",v.x,v.y,-v.z));
				//sb.Append(string.Format("v {0} {1} {2}\n",v.x,v.y,v.z));
			}
			sb.Append("\n");
			foreach(Vector3 nn in m.normals) 
			{
				Vector3 v = r * nn;
				sb.Append(string.Format("vn {0} {1} {2}\n",-v.x,-v.y,v.z));
				//sb.Append(string.Format("vn {0} {1} {2}\n",v.x,v.y,v.z));
			}
			sb.Append("\n");
			foreach(Vector3 v in m.uv) 
			{
				sb.Append(string.Format("vt {0} {1}\n",v.x,v.y));
			}
			for (int material=0; material < m.subMeshCount; material ++) 
			{
				sb.Append("\n");
				sb.Append("usemtl ").Append(mats[material].name).Append("\n");
				sb.Append("usemap ").Append(mats[material].name).Append("\n");

				int[] triangles = m.GetTriangles(material);
				for (int i=0;i<triangles.Length;i+=3) {
                    //sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", 
                    //Because we inverted the x-component, we also needed to alter the triangle winding.
                    sb.Append(string.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n",
                        triangles[i]+1+_StartIndex, triangles[i+1]+1+_StartIndex, triangles[i+2]+1+_StartIndex));
				}
			}

			_StartIndex += numVertices;
			return sb.ToString();
		}
	}
}
