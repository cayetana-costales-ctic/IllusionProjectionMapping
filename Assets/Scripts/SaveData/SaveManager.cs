using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // ... (Tus clases SaveFile y SaveEntry siguen igual) ...
    [Serializable] private class SaveFile { public List<SaveEntry> entries = new List<SaveEntry>(); }

    [Serializable] private class SaveEntry { public string id; public string type; public string json; }

    [Header("Configuración")]
    public string folderName = "UserContent"; // Nombre de tu carpeta visible

    public GameObject projectionPrefab;
    public string saveableTag = "SaveableRuntime";

    // Variables internas
    public string CurrentSceneName { get; private set; } = "";

    private Dictionary<string, SaveEntry> cachedEntries = new Dictionary<string, SaveEntry>();

    // --- NUEVO: Propiedad inteligente para la ruta ---
    public string SaveFolderPath
    {
        get
        {
            string basePath;

            // Si estamos en el Editor, guardamos en la raíz del proyecto (fuera de Assets)
#if UNITY_EDITOR
            basePath = Directory.GetParent(Application.dataPath).FullName;
#else
            // Si es una Build, guardamos junto al ejecutable (.exe)
                basePath = AppDomain.CurrentDomain.BaseDirectory;
#endif

            // Combinamos con el nombre de tu carpeta personalizada
            string fullPath = Path.Combine(basePath, folderName);

            // IMPORTANTE: Crear la carpeta si no existe (la primera vez)
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                Debug.Log($"📁 Carpeta creada: {fullPath}");
            }

            return fullPath;
        }
    }

    // --- MÉTODOS ACTUALIZADOS ---

    public List<string> GetSavedScenes()
    {
        // CAMBIO: Usamos SaveFolderPath en lugar de Application.persistentDataPath
        string dirPath = SaveFolderPath;

        if (!Directory.Exists(dirPath)) return new List<string>();
        return Directory.GetFiles(dirPath, "*.json")
                        .Select(Path.GetFileNameWithoutExtension)
                        .ToList();
    }

    public void SaveScene(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return;

        EnsureSaveables();
        cachedEntries.Clear();

        // ... (Tu lógica de captura de objetos sigue IGUAL aquí) ...
        var allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        var file = new SaveFile();
        foreach (var behaviour in allBehaviours)
        {
            if (!behaviour.gameObject.activeInHierarchy) continue;
            if (behaviour is ISaveable saveable)
            {
                var mb = (MonoBehaviour)saveable;
                if (!mb.gameObject.CompareTag(saveableTag)) continue;
                object state = null;
                try { state = saveable.CaptureState(); } catch { continue; }
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
        // ... (Fin de captura) ...

        // CAMBIO: Usamos SaveFolderPath
        string fullPath = Path.Combine(SaveFolderPath, fileName + ".json");

        try
        {
            File.WriteAllText(fullPath, JsonUtility.ToJson(file, true));
            Debug.Log($"✅ Guardado en: {fullPath}");
            CurrentSceneName = fileName;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error guardando: {ex}");
        }
    }

    public void LoadScene(string fileName)
    {
        // CAMBIO: Usamos SaveFolderPath
        string fullPath = Path.Combine(SaveFolderPath, fileName + ".json");

        if (!File.Exists(fullPath))
        {
            Debug.LogWarning("Archivo no encontrado: " + fullPath);
            return;
        }

        string json = File.ReadAllText(fullPath);
        SaveFile file = JsonUtility.FromJson<SaveFile>(json);

        // ... (Tu lógica de limpieza y carga sigue IGUAL) ...
        var allGameObjects = FindObjectsOfType<GameObject>(true);
        var toDestroy = new List<GameObject>();
        foreach (var go in allGameObjects)
            if (go.CompareTag(saveableTag)) toDestroy.Add(go);

        foreach (var go in toDestroy)
        {
            if (Application.isPlaying)
            {
                Destroy(go);
            }
#if UNITY_EDITOR
            else
            {
                // Solo usamos DestroyImmediate en el Editor cuando NO estamos en Play Mode
                DestroyImmediate(go);
            }
#endif
        }

        foreach (var entry in file.entries)
        {
            GameObject obj = Instantiate(projectionPrefab);
            obj.SetActive(true);
            obj.tag = saveableTag;
            obj.name = entry.id;

            var saveable = obj.GetComponent<ISaveable>();
            if (saveable == null) saveable = obj.AddComponent<SaveableProjection>();
            if (saveable is SaveableProjection sp) sp.uniqueID = entry.id;

            ForceEnableAllComponents(obj);
            Type type = Type.GetType(entry.type);
            saveable.RestoreState(JsonUtility.FromJson(entry.json, type));
            ForceEnableAllComponents(obj);
        }
        // ... (Fin de carga) ...

        CurrentSceneName = fileName;
        Debug.Log($"📂 Escena cargada desde: {fullPath}");
    }

    // ... (El resto de métodos EnsureSaveables y ForceEnableAllComponents siguen igual) ...
    private void EnsureSaveables()
    {
        var allGOs = FindObjectsOfType<GameObject>(true);
        foreach (var go in allGOs)
        {
            if (go.GetComponent<SaveableProjection>() != null)
            {
                if (!go.CompareTag(saveableTag)) go.tag = saveableTag;
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
        if (!root) return;
        root.SetActive(true);
        foreach (var c in root.GetComponentsInChildren<Behaviour>(true))
            try { c.enabled = true; } catch { }
    }
}