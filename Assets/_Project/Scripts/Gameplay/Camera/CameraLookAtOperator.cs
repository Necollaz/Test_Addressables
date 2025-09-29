using UnityEngine;

public class CameraLookAtOperator
{
    private readonly float cameraLookOffsetY;

    public CameraLookAtOperator(float cameraLookOffsetY)
    {
        this.cameraLookOffsetY = cameraLookOffsetY;
    }

    public void LookAtBoundsCenter(Camera camera, Bounds bounds)
    {
        if (camera == null)
            return;

        Vector3 target = bounds.center + Vector3.up * cameraLookOffsetY;
        camera.transform.LookAt(target);
    }
}