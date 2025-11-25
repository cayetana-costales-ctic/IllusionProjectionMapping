using UnityEngine;
using UnityEngine.Events;

public class KeyPressEvent : MonoBehaviour
{
    public KeyCode key = KeyCode.Space;

    public UnityEvent onKeyPressed;

    void Update()
    {
        if (Input.GetKeyDown(key))
            onKeyPressed?.Invoke();
    }
}
