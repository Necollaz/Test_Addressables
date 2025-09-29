using UnityEngine;

public class RendererBoundsCalculator
{
    public Bounds Calculate(GameObject root)
    {
        if (root == null)
            return new Bounds(Vector3.zero, Vector3.one);

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        
        if (renderers == null || renderers.Length == 0)
            return new Bounds(root.transform.position, Vector3.one);

        Bounds renderBounds = new Bounds(renderers[0].bounds.center, renderers[0].bounds.size);
        
        for (int i = 1; i < renderers.Length; i++)
            renderBounds.Encapsulate(renderers[i].bounds);
        
        return renderBounds;
    }
}