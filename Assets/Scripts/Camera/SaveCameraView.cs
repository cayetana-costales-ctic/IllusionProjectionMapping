using UnityEngine;

public class SaveCameraView : MonoBehaviour
{
    public Camera targetCamera;
    public GameObject savedViewGizmoPrefab;

    private GameObject savedViewGizmo;
    private Vector3 savedPosition;
    private Quaternion savedRotation;

    void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void Start()
    {
        savedPosition = targetCamera.transform.position;
        savedRotation = targetCamera.transform.rotation;

        if (savedViewGizmoPrefab)
        {
            savedViewGizmo = Instantiate(savedViewGizmoPrefab, savedPosition, Quaternion.identity);
            savedViewGizmo.name = "SavedView";
        }
    }

    public void SaveCurrentView()
    {
        savedPosition = targetCamera.transform.position;
        savedRotation = targetCamera.transform.rotation;

        if (savedViewGizmo)
            savedViewGizmo.transform.position = savedPosition;
    }

    public void RestoreView()
    {
        targetCamera.transform.position = savedPosition;
        targetCamera.transform.rotation = savedRotation;

        FreeCameraController fcc = targetCamera.GetComponent<FreeCameraController>();
        if (fcc)
        {
            Vector3 euler = targetCamera.transform.rotation.eulerAngles;
            fcc.SetRotation(euler.x, euler.y);
        }
    }
}
