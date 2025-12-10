using UnityEngine;
using RuntimeGizmos;

public class VertexEditController : MonoBehaviour
{
    [Header("Referencias")]
    public TransformGizmo gizmo;

    public ToggleEvent toggleEvent;

    private bool vertexEditMode = false;
    private PlaneVertexEditor activeEditor;

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
        Transform targetToEdit = gizmo.currentTarget;
        gizmo.SetVisible(false);

        if (targetToEdit)
            EnableEditorFor(targetToEdit);
    }

    public void DeactivateVertexEditMode()
    {
        vertexEditMode = false;
        gizmo.SetVisible(true);

        DisableCurrentEditor();
    }

    private void OnTargetSelected()
    {
        if (!vertexEditMode) return;

        if (gizmo.currentTarget)
            EnableEditorFor(gizmo.currentTarget);
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
        editor.InitOrCheck();

        activeEditor = editor;
        activeEditor.ShowHandles();

        activeEditor.UpdateHandlePositions();
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