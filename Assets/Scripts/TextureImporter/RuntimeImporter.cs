using SFB;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public static class RuntimeImporter
{
    private static string imageFolder = Application.persistentDataPath + "/ImportedImages/";
    private static string videoFolder = Application.persistentDataPath + "/ImportedVideos/";

    public static string ImportImage()
    {
        var extensions = new[] {
        new ExtensionFilter("Image Files", "png", "jpg", "jpeg")
    };

        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Image", "", extensions, false);

        if (paths.Length == 0)
            return null;

        string sourcePath = paths[0];

        if (!Directory.Exists(imageFolder))
            Directory.CreateDirectory(imageFolder);

        string fileName = Path.GetFileNameWithoutExtension(sourcePath);
        string extension = Path.GetExtension(sourcePath);
        string uniqueName = fileName + "_" + System.Guid.NewGuid().ToString("N") + extension;

        string targetPath = Path.Combine(imageFolder, uniqueName);

        File.Copy(sourcePath, targetPath, true);

        return targetPath;
    }

    public static Texture2D LoadImage(string path)
    {
        if (!File.Exists(path))
            return null;

        byte[] bytes = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);

        return tex;
    }

    public static string ImportVideo()
    {
        var extensions = new[] {
        new ExtensionFilter("Video Files", "mp4", "mov", "avi", "mkv")
    };

        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Video", "", extensions, false);

        if (paths.Length == 0)
            return null;

        string sourcePath = paths[0];

        if (!Directory.Exists(videoFolder))
            Directory.CreateDirectory(videoFolder);

        string fileName = Path.GetFileNameWithoutExtension(sourcePath);
        string extension = Path.GetExtension(sourcePath);
        string uniqueName = fileName + "_" + System.Guid.NewGuid().ToString("N") + extension;

        string targetPath = Path.Combine(videoFolder, uniqueName);

        File.Copy(sourcePath, targetPath, true);

        return targetPath;
    }
}