using UnityEngine;
using UnityEngine.Events;

public class ToggleEvent : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.Space;

    public UnityEvent onActivate;
    public UnityEvent onDeactivate;

    private bool isActive = false;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isActive = !isActive;

            if (isActive)
                onActivate.Invoke();
            else
                onDeactivate.Invoke();
        }
    }
    public void Toggle()
    {
        isActive = !isActive;

        if (isActive)
            onActivate.Invoke();
        else
            onDeactivate.Invoke();
    }
}
