using System;
using UnityEngine;
using UnityEngine.Video;

[Serializable]
public class ProjectionSaveData : TransformSaveData
{
    public string objectName;
    public string meshName;
    public string materialName;

    public string texturePath;
    public string videoPath;

    public ProjectionSaveData() : base()
    {
    }

    public ProjectionSaveData(GameObject obj) : base(obj.transform)
    {
        objectName = obj.name;

        var filter = obj.GetComponent<MeshFilter>();
        var renderer = obj.GetComponent<MeshRenderer>();
        var video = obj.GetComponentInChildren<VideoPlayer>();

        meshName = filter?.sharedMesh?.name ?? "";
        materialName = renderer?.sharedMaterial?.name ?? "";

        texturePath = renderer?.sharedMaterial?.mainTexture != null
            ? renderer.sharedMaterial.mainTexture.name
            : "";

        if (renderer && renderer.sharedMaterial && renderer.sharedMaterial.mainTexture)
        {
            if (renderer.sharedMaterial.mainTexture is Texture2D tex)
                texturePath = tex.name;
        }

        videoPath = video?.url ?? "";
    }
}