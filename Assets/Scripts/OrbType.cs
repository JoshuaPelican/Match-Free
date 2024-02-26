using UnityEngine;

public enum OrbType
{
    None,
    Red,
    Blue,
    Green,
    Yellow,
    Purple,
    Orange
}

public static class OrbTypeUtility
{
    public static readonly Color[] TypeColors = new Color[]
    {
        new Color32(120, 110, 120, 255), // gray
        new Color32(234, 85, 70, 255), // red
        new Color32(64, 164, 216, 255), // blue
        new Color32(107, 192, 120, 255), // green
        new Color32(254, 204, 47, 255), //yellow
        new Color32(163, 99, 216, 255), //purple
        new Color32(249, 162, 40, 255) // orange
    };
}