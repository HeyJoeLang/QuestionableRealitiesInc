using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    public float speed = 1;
    public Vector3 axis = Vector3.up;

    void FixedUpdate()
    {
        transform.Rotate(axis, speed * Time.fixedDeltaTime,Space.Self);
    }
}
