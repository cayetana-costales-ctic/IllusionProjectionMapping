using System;
using UnityEngine;
using UnityEngine.Video;

[Serializable]
public class ProjectionSaveData : TransformSaveData
{
    public string objectName;
    public string meshName;
    public string materialName;
    public string textureName;
    public string videoClipName;

    public ProjectionSaveData() : base() { }

    public ProjectionSaveData(GameObject obj) : base(obj.transform)
    {
        objectName = obj.name;

        var filter = obj.GetComponent<MeshFilter>();
        var renderer = obj.GetComponent<MeshRenderer>();
        var video = obj.GetComponentInChildren<VideoPlayer>();

        meshName = filter?.sharedMesh?.name ?? "";
        materialName = renderer?.sharedMaterial?.name ?? "";
        textureName = renderer?.sharedMaterial?.mainTexture?.name ?? "";
        videoClipName = video?.clip?.name ?? "";
    }
}