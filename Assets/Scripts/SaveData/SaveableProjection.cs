using System;
using UnityEngine;
using UnityEngine.Video;

public class SaveableProjection : MonoBehaviour, ISaveable
{
    public string uniqueID;

    void Awake()
    {
        if (string.IsNullOrEmpty(uniqueID))
            uniqueID = Guid.NewGuid().ToString();
    }

    public string GetUniqueID() => uniqueID;

    public object CaptureState()
    {
        return new ProjectionSaveData(gameObject);
    }

    public void RestoreState(object state)
    {
        var data = state as ProjectionSaveData;
        if (data == null) return;

        data.ApplyTo(transform);

        var filter = GetComponent<MeshFilter>();
        if (filter != null)
            filter.sharedMesh = FindMeshByName(data.meshName);

        var renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = FindMaterialByName(data.materialName);
            if (renderer.sharedMaterial != null && !string.IsNullOrEmpty(data.textureName))
            {
                renderer.sharedMaterial.mainTexture = FindTextureByName(data.textureName);
            }
        }

        var video = GetComponentInChildren<VideoPlayer>();
        if (video != null)
            video.clip = FindVideoClipByName(data.videoClipName);
    }

    Mesh FindMeshByName(string name)
    {
        foreach (var m in Resources.FindObjectsOfTypeAll<Mesh>())
            if (m.name == name) return m;
        return null;
    }

    Material FindMaterialByName(string name)
    {
        foreach (var m in Resources.FindObjectsOfTypeAll<Material>())
            if (m.name == name) return m;
        return null;
    }

    Texture FindTextureByName(string name)
    {
        foreach (var t in Resources.FindObjectsOfTypeAll<Texture>())
            if (t.name == name) return t;
        return null;
    }

    VideoClip FindVideoClipByName(string name)
    {
        foreach (var v in Resources.FindObjectsOfTypeAll<VideoClip>())
            if (v.name == name) return v;
        return null;
    }
}