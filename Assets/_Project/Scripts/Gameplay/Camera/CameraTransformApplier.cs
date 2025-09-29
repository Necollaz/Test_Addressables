using UnityEngine;

public class CameraTransformApplier
{
    public void Apply(Camera camera, CameraPose pose)
    {
        if (camera == null)
            return;

        camera.transform.position = pose.Position;
        camera.transform.rotation = Quaternion.Euler(pose.EulerAngles);
    }
}