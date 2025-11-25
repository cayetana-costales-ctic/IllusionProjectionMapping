using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [Serializable]
    private class SaveFile
    {
        public List<SaveEntry> entries = new List<SaveEntry>();
    }

    [Serializable]
    private class SaveEntry
    {
        public string id;
        public string type;
        public string json;
    }

    public string saveFileName = "scene_save.json";
    public GameObject projectionPrefab;

    private Dictionary<string, SaveEntry> cachedEntries = new Dictionary<string, SaveEntry>();

    [ContextMenu("Save All")]
    public void SaveAll()
    {
        cachedEntries.Clear();
        var allSaveables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        var file = new SaveFile();

        foreach (var behaviour in allSaveables)
        {
            if (behaviour is ISaveable saveable)
            {
                object state = saveable.CaptureState();
                if (state == null) continue;

                string json = JsonUtility.ToJson(state);
                var entry = new SaveEntry
                {
                    id = saveable.GetUniqueID(),
                    type = state.GetType().AssemblyQualifiedName,
                    json = json
                };
                file.entries.Add(entry);
                cachedEntries[entry.id] = entry;
            }
        }

        string saveJson = JsonUtility.ToJson(file, true);
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        File.WriteAllText(path, saveJson);
        Debug.Log($"Save Complete: {file.entries.Count} objs: {path}");
    }

    [ContextMenu("Load All (Reset Scene)")]
    public void LoadAll()
    {
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning("No save file");
            return;
        }

        string json = File.ReadAllText(path);
        var file = JsonUtility.FromJson<SaveFile>(json);

        var existingSaveables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var b in existingSaveables)
            if (b is ISaveable s)
                Destroy(((MonoBehaviour)s).gameObject);

        foreach (var entry in file.entries)
        {
            GameObject obj = Instantiate(projectionPrefab);
            obj.name = entry.id;
            ISaveable saveable = obj.GetComponent<ISaveable>();
            if (saveable is SaveableProjection sp) sp.uniqueID = entry.id;

            System.Type type = System.Type.GetType(entry.type);
            object state = JsonUtility.FromJson(entry.json, type);
            saveable.RestoreState(state);
        }

        Debug.Log("Load Complete");
    }
}