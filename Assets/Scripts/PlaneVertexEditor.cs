using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class PlaneVertexEditor : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private List<GameObject> handles = new List<GameObject>();
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        CreateHandles();
    }

    private void CreateHandles()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(vertices[i]);
            GameObject h = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            h.transform.position = worldPos;
            h.transform.localScale = Vector3.one * 0.075f;
            h.GetComponent<Collider>().isTrigger = true;
            h.name = "Handle_" + i;
            // Add a script to handle dragging
            HandleDrag hd = h.AddComponent<HandleDrag>();
            hd.Init(this, i, mainCam);
            handles.Add(h);
        }
        GetComponent<QuadBilinear>().P0 = handles[0].transform;
        GetComponent<QuadBilinear>().P1 = handles[1].transform;
        GetComponent<QuadBilinear>().P2 = handles[2].transform;
        GetComponent<QuadBilinear>().P3 = handles[3].transform;
    }

    public void OnHandleMoved(int index, Vector3 newWorldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(newWorldPos);
        vertices[index] = localPos;
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        // If you use UVs or other data, maybe update them too
    }

    private void Update()
    {
        // Optionally update handle positions if transform moves etc
        for (int i = 0; i < handles.Count; i++)
        {
            handles[i].transform.position = transform.TransformPoint(vertices[i]);
        }
    }
}