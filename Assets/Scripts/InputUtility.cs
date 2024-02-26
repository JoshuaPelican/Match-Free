using UnityEngine;

public static class InputUtility
{
    public static Vector2 MousePosition => Camera.main.ScreenToWorldPoint(Input.mousePosition);
}