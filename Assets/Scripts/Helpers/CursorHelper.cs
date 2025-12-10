using UnityEngine;

[CreateAssetMenu(fileName = "CursorManager", menuName = "Tools/Cursor Manager")]
public class CursorManager : ScriptableObject
{
    public void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}