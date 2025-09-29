using UnityEngine;

public class PreviewLightPlacer
{
    private readonly Vector3 lightWorldOffset;

    public PreviewLightPlacer(Vector3 lightWorldOffset)
    {
        if (Mathf.Approximately(lightWorldOffset.x, 0f) && Mathf.Approximately(lightWorldOffset.y, 0f) &&
            Mathf.Approximately(lightWorldOffset.z, 0f))
        {
            this.lightWorldOffset = new Vector3(1f, 2f, 1.5f);
        }
        else
        {
            this.lightWorldOffset = lightWorldOffset;
        }
    }

    public void MoveLightToBoundsCenterWithOffset(Light light, Bounds bounds)
    {
        if (light == null)
            return;

        light.transform.position = bounds.center + lightWorldOffset;
    }
}