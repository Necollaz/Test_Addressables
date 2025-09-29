public class CameraOverrideSelector
{
    private readonly CameraGroupOverride[] cameraGroupOverrides;

    public CameraOverrideSelector(CameraGroupOverride[] cameraGroupOverrides)
    {
        this.cameraGroupOverrides = cameraGroupOverrides;
    }

    public bool TrySelectPose(string assetKey, out CameraPose pose)
    {
        pose = default;

        if (cameraGroupOverrides == null || cameraGroupOverrides.Length == 0 || string.IsNullOrWhiteSpace(assetKey))
            return false;

        for (int i = 0; i < cameraGroupOverrides.Length; i++)
        {
            if (cameraGroupOverrides[i].IsMatch(assetKey))
            {
                pose = new CameraPose(cameraGroupOverrides[i].CameraPosition, cameraGroupOverrides[i].CameraEulerAngles);
                
                return true;
            }
        }

        return false;
    }
}