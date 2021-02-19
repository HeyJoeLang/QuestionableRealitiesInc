using UnityEngine;
using System.Collections.Generic;
using LunarCatsStudio.SuperCombiner;

public class RemoveMeshFromCombined : MonoBehaviour {

    public List<int> instanceID = new List<int>();

    public CombinedMeshModification meshModifier;

	// Use this for initialization
	void Start () {
        foreach(int i in instanceID)
        {
            meshModifier.RemoveFromCombined(i);
        }
    }
}
