using System;
using UnityEngine;

[Serializable]
public class TransformSaveData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public TransformSaveData() { }

    public TransformSaveData(Transform t)
    {
        position = t.position;
        rotation = t.rotation;
        scale = t.localScale;
    }

    public void ApplyTo(Transform t)
    {
        t.position = position;
        t.rotation = rotation;
        t.localScale = scale;
    }
}