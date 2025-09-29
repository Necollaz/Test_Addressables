using UnityEngine;

public readonly struct DropdownItemImageLayout
{
    public readonly Vector2 AnchorMin;
    public readonly Vector2 AnchorMax;
    public readonly Vector2 Pivot;
    public readonly float RightPadding;

    public DropdownItemImageLayout(Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, float rightPadding)
    {
        AnchorMin = anchorMin;
        AnchorMax = anchorMax;
        Pivot = pivot;
        RightPadding = rightPadding;
    }
}