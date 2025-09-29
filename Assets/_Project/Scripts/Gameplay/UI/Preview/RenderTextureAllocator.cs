using UnityEngine;

public class RenderTextureAllocator
{
    private readonly RenderTextureFormat format;
    private readonly string textureName;
    private readonly int textureSize;
    private readonly int depthBufferBits;

    public RenderTextureAllocator(RenderTextureFormat format, string textureName, int textureSize, int depthBufferBits)
    {
        this.format = format;
        this.textureName = textureName;
        this.textureSize = textureSize;
        this.depthBufferBits = depthBufferBits;
    }

    public RenderTexture Ensure(RenderTexture current)
    {
        if (current != null)
            return current;

        var texture = new RenderTexture(textureSize, textureSize, depthBufferBits, format)
        {
            name = textureName,
            useMipMap = false,
            autoGenerateMips = false
        };
        
        return texture;
    }
}