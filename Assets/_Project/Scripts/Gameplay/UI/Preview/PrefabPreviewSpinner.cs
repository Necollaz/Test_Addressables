using UnityEngine;

public class PrefabPreviewSpinner
{
    private readonly Vector3 axisWorld;
    private readonly float degreesPerSecond;
    
    private Transform _target;

    public PrefabPreviewSpinner(Vector3 axisWorld, float degreesPerSecond)
    {
        this.axisWorld = axisWorld;
        this.degreesPerSecond = degreesPerSecond;
    }

    public void Tick(float deltaTimeUnscaled)
    {
        if (_target == null)
            return;

        _target.Rotate(axisWorld, degreesPerSecond * deltaTimeUnscaled, Space.World);
    }
    
    public void SetTarget(Transform newTarget) => _target = newTarget;

    public void ClearTarget() => _target = null;
}