using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// SaveManager mejorado:
/// - Auto-registra objetos "saveable" (SaveableProjection) para MeshRenderer / VideoPlayer.
/// - Guarda sólo objetos con tag "SaveableRuntime".
/// - LoadAll reinicia la escena a partir del JSON (destruye objetos previos con esa tag).
/// - Fuerza activación de componentes al instanciar.
/// - Protecciones contra referencias nulas/Instantiate con prefab nulo.
/// </summary>
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

    [Tooltip("Prefab contenedor que se instancia al hacer Load. Debe tener SaveableProjection en su root.")]
    public GameObject projectionPrefab;

    // Tag usado para marcar objetos que deben guardarse / recrearse
    public string saveableTag = "SaveableRuntime";

    private Dictionary<string, SaveEntry> cachedEntries = new Dictionary<string, SaveEntry>();

    // ---------------- SAVE ----------------
    [ContextMenu("Save All")]
    public void SaveAll()
    {
        // Asegurar que objetos nuevos se marquen como Saveable (opción B)
        EnsureSaveables();

        cachedEntries.Clear();

        var allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        var file = new SaveFile();

        foreach (var behaviour in allBehaviours)
        {
            if (behaviour is ISaveable saveable)
            {
                // Sólo guardamos los que tengan la tag correcta (evita guardar assets del proyecto o prefabs en escena)
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

    // ---------------- LOAD ----------------
    [ContextMenu("Load All (Reset Scene)")]
    public void LoadAll()
    {
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning("LoadAll: No save file found at: " + path);
            return;
        }

        // Leer JSON
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

        // 1) Destruir todos los objetos actuales marcados como saveable
        // Usamos ToArray para evitar problemas al modificar la colección durante la iteración
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

        // 2) Si no hay prefab, abortar con mensaje (protección contra Instantiate con null)
        if (projectionPrefab == null)
        {
            Debug.LogError("LoadAll: projectionPrefab is not assigned. Cannot instantiate saved objects.");
            return;
        }

        // 3) Recrear todos los objetos desde el JSON
        foreach (var entry in file.entries)
        {
            // Instanciar prefab contenedor
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

            // Asegurar activo y tag
            obj.SetActive(true);
            obj.tag = saveableTag;
            obj.name = entry.id;

            // Garantizar que exista SaveableProjection
            var saveable = obj.GetComponent<ISaveable>();
            if (saveable == null)
            {
                // Añadir componente SaveableProjection si falta
                var sp = obj.AddComponent<SaveableProjection>();
                saveable = sp;
            }

            // Forzar uniqueID si es SaveableProjection
            if (saveable is SaveableProjection sp2)
            {
                sp2.uniqueID = entry.id;
            }

            // Asegurar que todos los componentes estén activados antes de restaurar
            ForceEnableAllComponents(obj);

            // Deserializar estado y restaurar (con try/catch para no romper la carga completa)
            System.Type type = System.Type.GetType(entry.type);
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

            // Después de restaurar, forzamos activación otra vez (al restaurar podrían haberse desactivado)
            ForceEnableAllComponents(obj);
        }

        Debug.Log("Load Complete");
    }

    // ---------------- HELPERS ----------------

    /// <summary>
    /// Añade SaveableProjection automáticamente a todos los GameObjects que tengan MeshRenderer o VideoPlayer
    /// y que no tengan ya SaveableProjection. Marca los añadidos con la tag saveableTag.
    /// </summary>
    private void EnsureSaveables()
    {
        // Asegurarse de que la tag existe (solo informativo; la tag debe existir en el editor)
        // Añadimos only objects in scene (active or inactive)
        var allGOs = FindObjectsOfType<GameObject>(true);

        foreach (var go in allGOs)
        {
            // Si ya tiene SaveableProjection, aseguramos que tenga la tag correcta
            if (go.GetComponent<SaveableProjection>() != null)
            {
                if (!go.CompareTag(saveableTag))
                    go.tag = saveableTag;
                continue;
            }

            // Si tiene MeshRenderer o VideoPlayer, lo transformamos en "saveable"
            bool hasMesh = go.GetComponent<MeshRenderer>() != null;
            bool hasVideo = go.GetComponentInChildren<UnityEngine.Video.VideoPlayer>() != null;

            if (hasMesh || hasVideo)
            {
                // Evitar convertir objetos que son assets/prefabs que no estén en escena
                if (go.scene.IsValid())
                {
                    var sp = go.AddComponent<SaveableProjection>();
                    sp.uniqueID = Guid.NewGuid().ToString();
                    go.tag = saveableTag;
                }
            }
        }
    }

    /// <summary>
    /// Fuerza que todos los componentes relevantes en el objeto estén activados.
    /// </summary>
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