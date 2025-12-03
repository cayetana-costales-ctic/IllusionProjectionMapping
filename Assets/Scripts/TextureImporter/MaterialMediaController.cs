using RuntimeGizmos;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MaterialMediaController : MonoBehaviour
{
    private TransformGizmo gizmo;

    private Renderer currentRenderer;
    private VideoPlayer currentVideo;

    public RawImage texturePreview;

    private void Awake()
    {
        gizmo = GetComponent<TransformGizmo>();
    }

    public void RefreshTarget()
    {
        var target = gizmo.currentTarget;

        if (!target)
        {
            currentRenderer = null;
            currentVideo = null;

            if (texturePreview)
                texturePreview.texture = null;

            return;
        }

        currentRenderer = target.GetComponent<Renderer>();
        currentVideo = target.GetComponentInChildren<VideoPlayer>();

        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (!texturePreview) return;

        if (currentRenderer && currentRenderer.sharedMaterial && currentRenderer.sharedMaterial.mainTexture)
            texturePreview.texture = currentRenderer.sharedMaterial.mainTexture;
        else
            texturePreview.texture = null;
    }

    public void OnImportTextureButton()
    {
        string path = RuntimeImporter.ImportImage();

        if (string.IsNullOrEmpty(path))
            return;

        Texture2D tex = RuntimeImporter.LoadImage(path);

        if (!tex)
            return;

        RefreshTarget();
        ApplyTexture(tex);
    }

    public void ApplyTexture(Texture2D tex)
    {
        if (!currentRenderer) return;

        currentRenderer.material.mainTexture = tex;
    }

    public void ImportAndApplyVideo()
    {
        string path = RuntimeImporter.ImportVideo();
        if (path == null) return;

        RefreshTarget();

        if (currentVideo)
        {
            currentVideo.source = VideoSource.Url;
            currentVideo.url = path;
            currentVideo.Play();
        }
    }
}