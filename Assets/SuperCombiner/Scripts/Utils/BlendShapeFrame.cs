using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LunarCatsStudio.SuperCombiner
{
    /// <summary>
    /// Class describing data for a unique blendshape
    /// </summary>
	public class BlendShapeFrame
    {
		public string _shapeName;
		public float _frameWeight;
		public Vector3[] _deltaVertices;
		public Vector3[] _deltaNormals;
		public Vector3[] _deltaTangents;
		public int _vertexOffset;

		public BlendShapeFrame(string shapeName_p, float frameWeight_p, Vector3[] deltaVertices_p, Vector3[] deltaNormals_p, Vector3[] deltaTangents_p, int offset) {
			_shapeName = shapeName_p;
			_frameWeight = frameWeight_p;
			_deltaVertices = deltaVertices_p;
			_deltaNormals = deltaNormals_p;
			_deltaTangents = deltaTangents_p;
			_vertexOffset = offset;
		}
	}
}
