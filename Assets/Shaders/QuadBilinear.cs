using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class QuadBilinear : MonoBehaviour
{
    private Camera cam;
    private Material targetMaterial;

    public Vector2 textureTiling = Vector2.one;
    public Vector2 textureOffset = Vector2.zero;

    public Transform P0 { set; get; }
    public Transform P1 { set; get; }
    public Transform P2 { set; get; }
    public Transform P3 { set; get; }

    private void Start()
    {
        cam = Camera.main;
        targetMaterial = GetComponent<Renderer>().material;

        targetMaterial.mainTextureScale = textureTiling;
        targetMaterial.mainTextureOffset = textureOffset;
    }

    private void Update()
    {
        if (cam == null || targetMaterial == null || P0 == null || P1 == null || P2 == null || P3 == null) { return; }

        Vector3 v0 = cam.WorldToViewportPoint(P0.position);
        v0.y = 1.0f - v0.y;
        Vector3 v1 = cam.WorldToViewportPoint(P1.position);
        v1.y = 1.0f - v1.y;
        Vector3 v2 = cam.WorldToViewportPoint(P2.position);
        v2.y = 1.0f - v2.y;
        Vector3 v3 = cam.WorldToViewportPoint(P3.position);
        v3.y = 1.0f - v3.y;

        targetMaterial.SetVector("_P0", new Vector4(v0.x, v0.y, v0.z, 0));
        targetMaterial.SetVector("_P1", new Vector4(v1.x, v1.y, v1.z, 0));
        targetMaterial.SetVector("_P2", new Vector4(v2.x, v2.y, v2.z, 0));
        targetMaterial.SetVector("_P3", new Vector4(v3.x, v3.y, v3.z, 0));

        targetMaterial.mainTextureScale = textureTiling;
        targetMaterial.mainTextureOffset = textureOffset;
    }
}