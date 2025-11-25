using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FreeCameraController : MonoBehaviour
{
    [Header("Configuración")]
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;
    public bool active = false;

    private float pitch;
    private float yaw;
    private bool isRightClickHeld = false;

    void Start()
    {
        Vector3 rot = transform.rotation.eulerAngles;
        pitch = rot.x;
        yaw = rot.y;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        if (!active) return;

        HandleMouseInput();
        if (isRightClickHeld)
            HandleMovement();
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isRightClickHeld = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isRightClickHeld = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (!isRightClickHeld) return;

        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleMovement()
    {
        //WASD/QE
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) move += transform.forward;
        if (Input.GetKey(KeyCode.S)) move -= transform.forward;
        if (Input.GetKey(KeyCode.A)) move -= transform.right;
        if (Input.GetKey(KeyCode.D)) move += transform.right;
        if (Input.GetKey(KeyCode.E)) move += transform.up;
        if (Input.GetKey(KeyCode.Q)) move -= transform.up;

        transform.position += move * moveSpeed * Time.deltaTime;
    }

    public void SetRotation(float newPitch, float newYaw)
    {
        pitch = newPitch;
        yaw = newYaw;
    }

}
