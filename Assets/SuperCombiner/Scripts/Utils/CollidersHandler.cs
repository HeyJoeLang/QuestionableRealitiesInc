using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LunarCatsStudio.SuperCombiner
{
    /// <summary>
    /// This class  handles collider management
    /// </summary>
    public class CollidersHandler : MonoBehaviour
    {

        /// <summary>
        /// Create a new GameObject with collider element based on the original Collider element in paramter
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="originalCollider"></param>
        /// <returns></returns>
        public static GameObject CreateNewCollider(Transform parent, Collider originalCollider)
        {
            // Instantiate the new GameObject containing the new collider
            GameObject newCollider = new GameObject(originalCollider.name);
            newCollider.transform.parent = parent.transform;
            newCollider.transform.position = originalCollider.transform.position;
            newCollider.transform.rotation = originalCollider.transform.rotation;
            newCollider.transform.localScale = originalCollider.transform.localScale;

            // Add the correct Collider Component type
            System.Type colliderType = originalCollider.GetType();
            if (colliderType == typeof(BoxCollider))
            {
                newCollider.AddComponent<BoxCollider>();
            }
            else if (colliderType == typeof(SphereCollider))
            {
                newCollider.AddComponent<SphereCollider>();
            }
            else if (colliderType == typeof(CapsuleCollider))
            {
                newCollider.AddComponent<CapsuleCollider>();
            }
            else if (colliderType == typeof(MeshCollider))
            {
                newCollider.AddComponent<MeshCollider>();
            }
            else if (colliderType == typeof(TerrainCollider))
            {
                newCollider.AddComponent<TerrainCollider>();
            }
            Collider colliderCopy = newCollider.GetComponent<Collider>();

            System.Reflection.FieldInfo[] fields = colliderType.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(colliderCopy, field.GetValue(originalCollider));
            }

            foreach (var prop in colliderType.GetProperties())
            {
                if (prop.CanWrite && !prop.Name.Equals("name"))
                {
                    prop.SetValue(colliderCopy, prop.GetValue(originalCollider, null), null);
                }
            }

            return newCollider;
        }
    }
}