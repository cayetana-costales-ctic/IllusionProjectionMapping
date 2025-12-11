using RuntimeGizmos;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MaterialMediaController : MonoBehaviour
{
    private TransformGizmo gizmo;
    private SaveManager saveManager;

    private Renderer currentRenderer;
    private VideoPlayer currentVideo;

    private void Awake()
    {
        gizmo = GetComponent<TransformGizmo>();
        saveManager = FindFirstObjectByType<SaveManager>();
    }

    public void RefreshTarget()
    {
        var target = gizmo.currentTarget;

        if (!target)
        {
            currentRenderer = null;
            currentVideo = null;
            return;
        }

        currentRenderer = target.GetComponent<Renderer>();
        currentVideo = target.GetComponentInChildren<VideoPlayer>(true);
    }

    public void OnImportTextureButton()
    {
        string baseFolder = saveManager ? saveManager.SaveFolderPath : Application.persistentDataPath;

        string path = RuntimeImporter.ImportImage(baseFolder);

        if (string.IsNullOrEmpty(path)) return;

        Texture2D tex = RuntimeImporter.LoadImage(path);

        if (!tex) return;

        RefreshTarget();
        ApplyTexture(tex, path);

        if (currentVideo)
        {
            currentVideo.Stop();
            currentVideo.gameObject.SetActive(false);
        }
    }

    public void ApplyTexture(Texture2D tex, string path)
    {
        if (!currentRenderer) return;

        currentRenderer.material.mainTexture = tex;

        var sp = currentRenderer.GetComponent<SaveableProjection>();
        if (sp != null)
            sp.LastTexturePath = path;
    }

    public void ImportAndApplyVideo()
    {
        string baseFolder = saveManager ? saveManager.SaveFolderPath : Application.persistentDataPath;

        string path = RuntimeImporter.ImportVideo(baseFolder);

        if (path == null) return;

        RefreshTarget();

        if (currentVideo)
        {
            currentVideo.gameObject.SetActive(true);
            currentVideo.source = VideoSource.Url;
            currentVideo.url = path;
            currentVideo.Play();
        }

        if (currentRenderer)
        {
            var sp = currentRenderer.GetComponent<SaveableProjection>();
            if (sp != null) sp.LastVideoPath = path;
        }
    }
}