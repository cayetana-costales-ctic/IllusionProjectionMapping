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

    public string saveableTag = "SaveableRuntime";

    private Dictionary<string, SaveEntry> cachedEntries = new Dictionary<string, SaveEntry>();

    [ContextMenu("Save All")]
    public void SaveAll()
    {
        EnsureSaveables();

        cachedEntries.Clear();

        var allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        var file = new SaveFile();

        foreach (var behaviour in allBehaviours)
        {
            if (behaviour is ISaveable saveable)
            {
                var mb = (MonoBehaviour)saveable;
                if (!mb.gameObject.CompareTag(saveableTag))
                    continue;

                object state = null;
                try
                {
                    state = saveable.CaptureState();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"SaveAll: error capturing state for {mb.gameObject.name}: {ex}");
                    continue;
                }

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

        try
        {
            File.WriteAllText(path, saveJson);
            Debug.Log($"Save Complete: {file.entries.Count} objects saved to: {path}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"SaveAll: error writing save file: {ex}");
        }
    }

    [ContextMenu("Load All (Reset Scene)")]
    public void LoadAll()
    {
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning("LoadAll: No save file found at: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        SaveFile file = null;
        try
        {
            file = JsonUtility.FromJson<SaveFile>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError("LoadAll: failed to parse save file: " + ex);
            return;
        }

        var allGameObjects = FindObjectsOfType<GameObject>(true);
        var toDestroy = new List<GameObject>();
        foreach (var go in allGameObjects)
        {
            if (go.CompareTag(saveableTag))
                toDestroy.Add(go);
        }

        foreach (var go in toDestroy)
        {
            if (Application.isPlaying)
                Destroy(go);
#if UNITY_EDITOR
            else
                DestroyImmediate(go);
#endif
        }

        if (projectionPrefab == null)
        {
            Debug.LogError("LoadAll: projectionPrefab is not assigned. Cannot instantiate saved objects.");
            return;
        }

        foreach (var entry in file.entries)
        {
            GameObject obj = null;
            try
            {
                obj = Instantiate(projectionPrefab);
            }
            catch (Exception ex)
            {
                Debug.LogError("LoadAll: Instantiate failed: " + ex);
                continue;
            }

            if (obj == null)
            {
                Debug.LogError("LoadAll: Instantiate returned null for projectionPrefab. Skipping entry " + entry.id);
                continue;
            }

            obj.SetActive(true);
            obj.tag = saveableTag;
            obj.name = entry.id;

            var saveable = obj.GetComponent<ISaveable>();
            if (saveable == null)
            {
                var sp = obj.AddComponent<SaveableProjection>();
                saveable = sp;
            }

            if (saveable is SaveableProjection sp2)
            {
                sp2.uniqueID = entry.id;
            }

            ForceEnableAllComponents(obj);

            Type type = Type.GetType(entry.type);
            object state = null;
            try
            {
                state = JsonUtility.FromJson(entry.json, type);
                saveable.RestoreState(state);
            }
            catch (Exception ex)
            {
                Debug.LogError($"LoadAll: error restoring entry {entry.id}: {ex}");
            }

            ForceEnableAllComponents(obj);
        }

        Debug.Log("Load Complete");
    }

    private void EnsureSaveables()
    {
        var allGOs = FindObjectsOfType<GameObject>(true);

        foreach (var go in allGOs)
        {
            if (go.GetComponent<SaveableProjection>() != null)
            {
                if (!go.CompareTag(saveableTag))
                    go.tag = saveableTag;
                continue;
            }

            bool hasMesh = go.GetComponent<MeshRenderer>() != null;
            bool hasVideo = go.GetComponentInChildren<UnityEngine.Video.VideoPlayer>() != null;

            if (hasMesh || hasVideo)
            {
                if (go.scene.IsValid())
                {
                    var sp = go.AddComponent<SaveableProjection>();
                    sp.uniqueID = Guid.NewGuid().ToString();
                    go.tag = saveableTag;
                }
            }
        }
    }

    private void ForceEnableAllComponents(GameObject root)
    {
        if (root == null) return;

        root.SetActive(true);

        var components = root.GetComponentsInChildren<Component>(true);
        foreach (var c in components)
        {
            if (c == null) continue;

            if (c is Behaviour b)
            {
                try { b.enabled = true; } catch { }
            }
        }
    }
}