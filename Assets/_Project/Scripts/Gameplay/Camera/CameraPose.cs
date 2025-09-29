using UnityEngine;

public struct CameraPose
{
    public Vector3 Position;
    public Vector3 EulerAngles;

    public CameraPose(Vector3 position, Vector3 eulerAngles)
    {
        Position = position;
        EulerAngles = eulerAngles;
    }
}