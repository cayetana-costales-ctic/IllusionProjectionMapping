using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class PlaneVertexEditor : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private readonly List<GameObject> handles = new List<GameObject>();
    private Camera mainCam;
    private bool handlesVisible = true;
    private QuadBilinear quadBilinear;

    private void Start()
    {
        mainCam = Camera.main;
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        quadBilinear = GetComponent<QuadBilinear>();

        CreateHandles();
        AssignHandlesToQuad();
    }

    public Vector3[] GetCurrentVertices()
    {
        return vertices != null ? (Vector3[])vertices.Clone() : null;
    }

    public void RestoreVertices(Vector3[] newVerts)
    {
        if (mesh == null)
            mesh = GetComponent<MeshFilter>().mesh;

        if (newVerts == null || newVerts.Length != mesh.vertexCount)
        {
            Debug.LogWarning("RestoreVertices: vertex array size mismatch!");
            return;
        }

        vertices = (Vector3[])newVerts.Clone();
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        UpdateHandlePositions();

        AssignHandlesToQuad();
    }

    private void CreateHandles()
    {
        const float handleScale = 0.1f;
        const float colliderRadius = 1.5f;

        handles.Capacity = vertices.Length;

        for (int i = 0; i < vertices.Length; i++)
        {
            var h = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var ht = h.transform;

            ht.SetParent(transform);

            ht.localPosition = vertices[i];
            ht.localScale = Vector3.one * handleScale;

            var collider = h.GetComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = colliderRadius;

            h.name = $"Handle_{i}";

            var hd = h.AddComponent<HandleDrag>();
            hd.Init(this, i, mainCam);

            handles.Add(h);
            h.SetActive(false);
        }
    }

    private void AssignHandlesToQuad()
    {
        if (!quadBilinear || handles.Count < 4) return;

        quadBilinear.P0 = handles[0].transform;
        quadBilinear.P1 = handles[1].transform;
        quadBilinear.P2 = handles[2].transform;
        quadBilinear.P3 = handles[3].transform;
    }

    public void ShowHandles()
    {
        if (handlesVisible) return;

        SetHandlesActive(true);
        handlesVisible = true;
        UpdateHandlePositions();
    }

    public void HideHandles()
    {
        if (!handlesVisible) return;

        SetHandlesActive(false);
        handlesVisible = false;
    }

    private void SetHandlesActive(bool state)
    {
        foreach (var h in handles)
        {
            if (h) h.SetActive(state);
        }
    }

    public void OnHandleMoved(int index, Vector3 newWorldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(newWorldPos);
        localPos.z = 0f; // Bloquear el eje Z

        vertices[index] = localPos;
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        UpdateHandlePositions();
    }

    private void UpdateHandlePositions()
    {
        if (!handlesVisible) return;

        for (int i = 0; i < handles.Count; i++)
        {
            var h = handles[i];
            if (!h) continue;

            // Actualizamos las posiciones locales de los handles
            h.transform.localPosition = vertices[i];
        }
    }

    // Limpiar handles cuando se desactive el editor
    public void ClearHandles()
    {
        foreach (var h in handles)
        {
            if (h != null)
            {
                Destroy(h);
            }
        }
        handles.Clear();
    }
}