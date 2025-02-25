using System;
using UnityEngine;

[Serializable]
public class TransformOffset
{
    public Vector3 position;
    public Vector3 rotation;


    public Quaternion Rotation => Quaternion.Euler(rotation);
    
}