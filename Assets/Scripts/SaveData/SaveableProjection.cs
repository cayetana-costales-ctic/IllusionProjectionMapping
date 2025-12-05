using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class SaveableProjection : MonoBehaviour, ISaveable
{
    public string uniqueID;

    private void Awake()
    {
        if (string.IsNullOrEmpty(uniqueID))
            uniqueID = System.Guid.NewGuid().ToString();
    }

    public string GetUniqueID() => uniqueID;

    public object CaptureState()
    {
        return new ProjectionSaveData(gameObject);
    }

    public void RestoreState(object state)
    {
        var data = (ProjectionSaveData)state;
        if (data == null) return;

        data.ApplyTo(transform);

        var renderer = GetComponent<MeshRenderer>();
        var video = GetComponentInChildren<VideoPlayer>();
        var quad = GetComponent<QuadBilinear>();

        if (renderer && !string.IsNullOrEmpty(data.texturePath) && File.Exists(data.texturePath))
        {
            Texture2D tex = RuntimeImporter.LoadImage(data.texturePath);
            if (tex != null) renderer.material.mainTexture = tex;
        }

        if (renderer)
        {
            renderer.material.mainTextureScale = data.textureTiling;
            renderer.material.mainTextureOffset = data.textureOffset;
        }

        if (quad != null)
        {
            quad.textureTiling = data.quadTextureTiling;
            quad.textureOffset = data.quadTextureOffset;

            if (renderer)
            {
                renderer.material.mainTextureScale = quad.textureTiling;
                renderer.material.mainTextureOffset = quad.textureOffset;
            }
        }

        if (video && !string.IsNullOrEmpty(data.videoPath) && File.Exists(data.videoPath))
        {
            video.source = VideoSource.Url;
            video.url = data.videoPath;
        }

        var editor = GetComponent<PlaneVertexEditor>();
        if (editor != null && data.editedVertices != null && data.editedVertices.Length > 0)
        {
            editor.RestoreVertices(data.editedVertices);
        }
    }
}