using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PlaneFromVertices : MonoBehaviour
{
    [Header("Asignaciones")]
    public Camera mainCamera;
    public Material planeMaterial;
    public GameObject markerPrefab;
    public float markerScale = 0.01f;

    [Header("Ajustes")]
    public int requiredPoints = 4;

    private List<Vector3> selectedPoints = new List<Vector3>();
    private List<GameObject> markers = new List<GameObject>();
    private LineRenderer lineRenderer;

    void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.loop = false;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;

        if (lineRenderer.sharedMaterial == null)
        {
            lineRenderer.sharedMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
            lineRenderer.sharedMaterial.color = Color.yellow;
        }
    }

    void Update()
    {
        // Click izquierdo para seleccionar punto
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                Vector3 point = hit.point;
                AddPoint(point);
            }
        }

        // R para reiniciar
        if (Input.GetKeyDown(KeyCode.R))
        {
            ClearSelection();
        }
    }

    void AddPoint(Vector3 point)
    {
        selectedPoints.Add(point);

        if (markerPrefab != null)
        {
            GameObject m = Instantiate(markerPrefab, point, Quaternion.identity);
            m.transform.localScale = Vector3.one * markerScale;
            markers.Add(m);
        }

        UpdateLinePreview();

        if (selectedPoints.Count == requiredPoints)
        {
            if (!TryCreatePlane(selectedPoints))
            {
                Debug.LogWarning("No se pudo crear el plano — puntos inválidos (colineales / duplicados).");
            }

            ClearSelection();
        }
    }

    void UpdateLinePreview()
    {
        if (selectedPoints.Count == 0)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.positionCount = selectedPoints.Count;
        for (int i = 0; i < selectedPoints.Count; i++)
            lineRenderer.SetPosition(i, selectedPoints[i]);
    }

    void ClearSelection()
    {
        selectedPoints.Clear();
        foreach (var m in markers)
            if (m != null) Destroy(m);
        markers.Clear();
        lineRenderer.positionCount = 0;
    }

    bool TryCreatePlane(List<Vector3> points)
    {
        if (points == null || points.Count != requiredPoints) return false;

        for (int i = 0; i < points.Count; i++)
            for (int j = i + 1; j < points.Count; j++)
                if (Vector3.Distance(points[i], points[j]) < 0.0001f)
                    return false;

        Vector3 normal = Vector3.Cross(points[1] - points[0], points[2] - points[0]);
        if (normal.sqrMagnitude < 1e-8f)
            return false;
        normal.Normalize();

        Vector3 center = (points[0] + points[1] + points[2] + points[3]) / 4f;

        Vector3 axisX = (points[0] - center);
        if (axisX.sqrMagnitude < 1e-8f)
            axisX = (points[1] - center);

        axisX = (axisX - Vector3.Dot(axisX, normal) * normal).normalized;
        if (axisX.sqrMagnitude < 1e-6f)
        {
            axisX = Vector3.Cross(Vector3.up, normal).normalized;
            if (axisX.sqrMagnitude < 1e-6f)
                axisX = Vector3.right;
        }
        Vector3 axisY = Vector3.Cross(normal, axisX).normalized;

        var angleList = new List<(Vector3 p, float angle)>();
        foreach (var p in points)
        {
            Vector3 d = p - center;
            float x = Vector3.Dot(d, axisX);
            float y = Vector3.Dot(d, axisY);
            float angle = Mathf.Atan2(y, x);
            angleList.Add((p, angle));
        }

        var ordered = angleList.OrderBy(a => a.angle).Select(a => a.p).ToArray();
        CreateMeshFromOrderedPoints(ordered, normal);
        return true;
    }

    void CreateMeshFromOrderedPoints(Vector3[] orderedPoints, Vector3 normal)
    {
        GameObject planeObj = new GameObject("GeneratedPlane");
        planeObj.transform.position = Vector3.zero;
        var mf = planeObj.AddComponent<MeshFilter>();
        var mr = planeObj.AddComponent<MeshRenderer>();

        // ✅ Material compatible con Built-in si no se asigna uno
        if (planeMaterial == null)
            planeMaterial = new Material(Shader.Find("Standard"));

        mr.material = planeMaterial;

        Mesh mesh = new Mesh();
        mesh.name = "QuadFromPoints";

        mesh.vertices = orderedPoints;
        mesh.triangles = new int[]
        {
            0,1,2,
            0,2,3,
            2,1,0,
            3,2,0
        };

        Vector3 centroid = (orderedPoints[0] + orderedPoints[1] + orderedPoints[2] + orderedPoints[3]) / 4f;
        Vector3 axisX = (orderedPoints[0] - centroid);
        axisX = (axisX - Vector3.Dot(axisX, normal) * normal).normalized;
        if (axisX.sqrMagnitude < 1e-6f) axisX = Vector3.right;
        Vector3 axisY = Vector3.Cross(normal, axisX).normalized;

        Vector2[] uvs = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            Vector3 d = orderedPoints[i] - centroid;
            uvs[i] = new Vector2(Vector3.Dot(d, axisX), Vector3.Dot(d, axisY));
        }

        float minX = uvs.Min(u => u.x), maxX = uvs.Max(u => u.x);
        float minY = uvs.Min(u => u.y), maxY = uvs.Max(u => u.y);
        float rangeX = Mathf.Max(0.0001f, maxX - minX);
        float rangeY = Mathf.Max(0.0001f, maxY - minY);
        for (int i = 0; i < 4; i++)
            uvs[i] = new Vector2((uvs[i].x - minX) / rangeX, (uvs[i].y - minY) / rangeY);

        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mf.mesh = mesh;

        var meshCol = planeObj.AddComponent<MeshCollider>();
        meshCol.sharedMesh = mesh;
    }
}
