using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class QuadQuadrilateralUpdater : MonoBehaviour
{
    public Camera targetCamera; // si null, usa Camera.main
    public int indexP0 = 0;
    public int indexP1 = 1;
    public int indexP2 = 2;
    public int indexP3 = 3;

    private Renderer rend;
    private MeshFilter mf;

    private void OnEnable()
    {
        rend = GetComponent<Renderer>();
        mf = GetComponent<MeshFilter>();
        if (targetCamera == null) targetCamera = Camera.main;
    }

    private void Update()
    {
        if (mf == null || mf.sharedMesh == null || rend == null) return;
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) return;

        // Read vertex positions - assumes mesh has at least 4 verts and correct order
        Vector3[] verts = mf.sharedMesh.vertices;
        Transform t = mf.transform;

        // safe index clamps
        int p0 = Mathf.Clamp(indexP0, 0, verts.Length - 1);
        int p1 = Mathf.Clamp(indexP1, 0, verts.Length - 1);
        int p2 = Mathf.Clamp(indexP2, 0, verts.Length - 1);
        int p3 = Mathf.Clamp(indexP3, 0, verts.Length - 1);

        Vector3 worldP0 = t.TransformPoint(verts[p0]);
        Vector3 worldP1 = t.TransformPoint(verts[p1]);
        Vector3 worldP2 = t.TransformPoint(verts[p2]);
        Vector3 worldP3 = t.TransformPoint(verts[p3]);

        // Project to viewport (0..1)
        Vector3 vp0 = targetCamera.WorldToViewportPoint(worldP0);
        Vector3 vp1 = targetCamera.WorldToViewportPoint(worldP1);
        Vector3 vp2 = targetCamera.WorldToViewportPoint(worldP2);
        Vector3 vp3 = targetCamera.WorldToViewportPoint(worldP3);

        // Convert viewport to NDC (-1..1)
        Vector2 ndc0 = new Vector2(vp0.x * 2f - 1f, vp0.y * 2f - 1f);
        Vector2 ndc1 = new Vector2(vp1.x * 2f - 1f, vp1.y * 2f - 1f);
        Vector2 ndc2 = new Vector2(vp2.x * 2f - 1f, vp2.y * 2f - 1f);
        Vector2 ndc3 = new Vector2(vp3.x * 2f - 1f, vp3.y * 2f - 1f);

        // set to material (works for single material)
        Material mat = rend.sharedMaterial;
        if (mat != null)
        {
            mat.SetVector("_P0", new Vector4(ndc0.x, ndc0.y, 0f, 0f));
            mat.SetVector("_P1", new Vector4(ndc1.x, ndc1.y, 0f, 0f));
            mat.SetVector("_P2", new Vector4(ndc2.x, ndc2.y, 0f, 0f));
            mat.SetVector("_P3", new Vector4(ndc3.x, ndc3.y, 0f, 0f));
        }
    }
}