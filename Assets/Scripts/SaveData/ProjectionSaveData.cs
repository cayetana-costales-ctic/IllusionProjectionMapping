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

    public Vector2 textureTiling;
    public Vector2 textureOffset;

    public Vector2 quadTextureTiling;
    public Vector2 quadTextureOffset;

    public Vector3[] editedVertices;

    public ProjectionSaveData() : base()
    {
    }

    public ProjectionSaveData(GameObject obj) : base(obj.transform)
    {
        objectName = obj.name;

        var filter = obj.GetComponent<MeshFilter>();
        var renderer = obj.GetComponent<MeshRenderer>();
        var video = obj.GetComponentInChildren<VideoPlayer>();
        var quad = obj.GetComponent<QuadBilinear>();

        var sp = obj.GetComponent<SaveableProjection>();

        meshName = filter?.sharedMesh?.name ?? "";
        materialName = renderer?.sharedMaterial?.name ?? "";

        texturePath = sp?.LastTexturePath ?? "";

        videoPath = sp?.LastVideoPath;
        if (string.IsNullOrEmpty(videoPath))
            videoPath = video?.url ?? "";

        if (renderer && renderer.sharedMaterial)
        {
            textureTiling = renderer.sharedMaterial.mainTextureScale;
            textureOffset = renderer.sharedMaterial.mainTextureOffset;
        }
        else
        {
            textureTiling = Vector2.one;
            textureOffset = Vector2.zero;
        }

        if (quad != null)
        {
            quadTextureTiling = quad.textureTiling;
            quadTextureOffset = quad.textureOffset;
        }
        else
        {
            quadTextureTiling = textureTiling;
            quadTextureOffset = textureOffset;
        }

        var editor = obj.GetComponent<PlaneVertexEditor>();
        if (editor)
            editedVertices = editor.GetCurrentVertices();
    }
}