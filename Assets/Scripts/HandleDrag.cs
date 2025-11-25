using UnityEngine;

public class HandleDrag : MonoBehaviour
{
    private PlaneVertexEditor editor;
    private int vertexIndex;
    private bool dragging = false;
    private Camera cam;
    private Vector3 localZLocked;

    public void Init(PlaneVertexEditor ed, int idx, Camera c)
    {
        editor = ed;
        vertexIndex = idx;
        cam = c;
        localZLocked = transform.localPosition;
    }

    private void OnMouseDown()
    {
        dragging = true;
    }

    private void OnMouseUp()
    {
        dragging = false;
    }

    private void Update()
    {
        if (!dragging || editor == null || cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        Plane p = new Plane(editor.transform.forward, transform.position);
        float enter;

        if (p.Raycast(ray, out enter))
        {
            Vector3 hit = ray.GetPoint(enter);
            Vector3 localHit = editor.transform.InverseTransformPoint(hit);

            hit.z = localZLocked.z;
            transform.position = hit;

            Vector3 worldFixed = editor.transform.TransformPoint(localHit);
            transform.position = worldFixed;

            editor.OnHandleMoved(vertexIndex, worldFixed);
        }
    }
}