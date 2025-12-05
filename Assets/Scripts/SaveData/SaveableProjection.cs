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

        if (renderer && !string.IsNullOrEmpty(data.texturePath) && File.Exists(data.texturePath))
        {
            Texture2D tex = RuntimeImporter.LoadImage(data.texturePath);
            if (tex != null) renderer.material.mainTexture = tex;
        }

        if (video && !string.IsNullOrEmpty(data.videoPath) && File.Exists(data.videoPath))
        {
            video.source = VideoSource.Url;
            video.url = data.videoPath;
        }
    }
}