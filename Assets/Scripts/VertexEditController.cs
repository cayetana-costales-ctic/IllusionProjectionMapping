using UnityEngine;
using RuntimeGizmos;

public class VertexEditController : MonoBehaviour
{
    [Header("Referencias")]
    public TransformGizmo gizmo; // El Gizmo que gestionará las selecciones

    public ToggleEvent toggleEvent; // El evento para cambiar de modo

    private bool vertexEditMode = false; // Indica si estamos en modo de edición de vértices
    private PlaneVertexEditor activeEditor; // Referencia al editor de vértices activo

    private void Start()
    {
        if (!gizmo)
            gizmo = FindFirstObjectByType<TransformGizmo>();

        if (toggleEvent)
        {
            toggleEvent.onActivate.AddListener(ActivateVertexEditMode);
            toggleEvent.onDeactivate.AddListener(DeactivateVertexEditMode);
        }

        gizmo.OnTargetSelected.AddListener(OnTargetSelected);
        gizmo.OnTargetDeselected.AddListener(OnTargetDeselected);

        DeactivateVertexEditMode();
    }

    private void OnDestroy()
    {
        if (toggleEvent)
        {
            toggleEvent.onActivate.RemoveListener(ActivateVertexEditMode);
            toggleEvent.onDeactivate.RemoveListener(DeactivateVertexEditMode);
        }

        gizmo.OnTargetSelected.RemoveListener(OnTargetSelected);
        gizmo.OnTargetDeselected.RemoveListener(OnTargetDeselected);
    }

    public void ActivateVertexEditMode()
    {
        vertexEditMode = true;
        gizmo.SetVisible(false);
        Debug.Log("🟢 Modo edición de vértices activado");

        if (gizmo.currentTarget)
            EnableEditorFor(gizmo.currentTarget);
    }

    public void DeactivateVertexEditMode()
    {
        vertexEditMode = false;
        gizmo.SetVisible(true);
        Debug.Log("⚪ Modo normal activado");

        DisableCurrentEditor();
    }

    private void OnTargetSelected()
    {
        if (!vertexEditMode) return;

        if (gizmo.currentTarget)
        {
            EnableEditorFor(gizmo.currentTarget);
        }
    }

    private void OnTargetDeselected()
    {
        DisableCurrentEditor();
    }

    private void EnableEditorFor(Transform target)
    {
        DisableCurrentEditor();

        var meshFilter = target.GetComponent<MeshFilter>();
        if (!meshFilter || !meshFilter.sharedMesh)
            return;

        var editor = target.GetComponent<PlaneVertexEditor>();
        if (!editor)
            editor = target.gameObject.AddComponent<PlaneVertexEditor>();

        editor.ShowHandles();
        activeEditor = editor;
    }

    private void DisableCurrentEditor()
    {
        if (activeEditor)
        {
            activeEditor.HideHandles();
            activeEditor = null;
        }
    }
}