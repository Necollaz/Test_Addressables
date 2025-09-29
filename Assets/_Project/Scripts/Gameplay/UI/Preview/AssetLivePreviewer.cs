using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class AssetLivePreviewer
{
    private const float UI_SPRITE_MAX_SIZE = 250f;

    private readonly AssetTypeProbe typeProbe;
    private readonly PrefabPreviewRenderer prefabPreviewRenderer;
    private readonly PreviewPanelSwitcher panelSwitcher;
    private readonly AddressableKeyNormalizer keyNormalizer;
    private readonly Image spritePreviewImage;
    
    private Texture2D _previewTextureCopy;
    private Sprite _previewSpriteCopy;
    private AsyncOperationHandle? _currentPreviewHandle;
    private string _currentPreviewKey;

    public AssetLivePreviewer(AssetTypeProbe typeProbe, PrefabPreviewRenderer prefabPreviewRenderer,
        PreviewPanelSwitcher panelSwitcher, Image spritePreviewImage, AddressableKeyNormalizer keyNormalizer)
    {
        this.typeProbe = typeProbe;
        this.prefabPreviewRenderer = prefabPreviewRenderer;
        this.panelSwitcher = panelSwitcher;
        this.spritePreviewImage = spritePreviewImage;
        this.keyNormalizer = keyNormalizer;
    }

    public async Task PreviewSelectedAssetAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            panelSwitcher.TryHideAll();
            ReleasePreviewIfAny();
            
            return;
        }

        ReleasePreviewIfAny();

        try
        {
            await Addressables.InitializeAsync().Task;
        }
        catch (Exception _)
        {
            panelSwitcher.TryHideAll();
            
            return;
        }

        var probeKey = keyNormalizer.Normalize(key);
        var (hasSprite, hasPrefab) = await typeProbe.ProbeExactAsync(probeKey);

        if (hasSprite)
        {
            var handle = Addressables.LoadAssetAsync<Sprite>(key);
            await handle.Task;
            
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                _previewSpriteCopy = MakeSpriteCopy(handle.Result, out _previewTextureCopy);
                
                if (spritePreviewImage != null)
                {
                    ApplySpritePreviewSizing(key);
                    spritePreviewImage.sprite = _previewSpriteCopy;

                    if (!key.StartsWith(GroupNameKeys.KEY_GROUP_UI, StringComparison.OrdinalIgnoreCase))
                        spritePreviewImage.SetNativeSize();
                }

                panelSwitcher.TryShowSpriteOnly();
            }

            Addressables.Release(handle);
            
            return;
        }

        if (hasPrefab)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(key);
            await handle.Task;
            
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                prefabPreviewRenderer?.ApplyCameraOverrideForKey(key);
                prefabPreviewRenderer?.TryShowPrefab(handle.Result);
                panelSwitcher.TryShowPrefabOnly();
            }

            Addressables.Release(handle);
            
            return;
        }
        
        panelSwitcher.TryHideAll();
    }

    public void ReleasePreviewIfAny()
    {
        prefabPreviewRenderer?.Clear();
        
        if (spritePreviewImage != null)
            spritePreviewImage.sprite = null;

        if (_previewSpriteCopy != null)
        {
            UnityEngine.Object.Destroy(_previewSpriteCopy);
            _previewSpriteCopy = null;
        }

        if (_previewTextureCopy != null)
        {
            UnityEngine.Object.Destroy(_previewTextureCopy);
            _previewTextureCopy = null;
        }
    }
    
    private Sprite MakeSpriteCopy(Sprite sprite, out Texture2D textureCopy)
    {
        Texture2D texture = sprite.texture;
        Rect rect = sprite.rect;
        
        if (texture.isReadable)
        {
            textureCopy = new Texture2D((int)rect.width, (int)rect.height, texture.format, false);
            Color[] pixels = texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
            
            textureCopy.SetPixels(pixels);
            textureCopy.Apply(false, false);
        }
        else
        {
            textureCopy = CopySubTexture(texture, rect);
        }

        Vector2 pivot = new Vector2(sprite.pivot.x / rect.width, sprite.pivot.y / rect.height);
        Sprite copy = Sprite.Create(textureCopy, new Rect(0, 0, rect.width, rect.height), pivot, sprite.pixelsPerUnit, 0,
            SpriteMeshType.Tight, sprite.border);
        copy.name = $"{sprite.name}_PreviewCopy";
        
        return copy;
    }
    
    private Texture2D CopySubTexture(Texture sourceTexture, Rect sourceRectanglePixels)
    {
        int targetWidth = Mathf.RoundToInt(sourceRectanglePixels.width);
        int targetHeight = Mathf.RoundToInt(sourceRectanglePixels.height);
        RenderTexture temporaryRenderTexture = RenderTexture.GetTemporary(targetWidth, targetHeight, 0,
            RenderTextureFormat.ARGB32);
        RenderTexture previousActiveRenderTexture = RenderTexture.active;

        try
        {
            float uvX = sourceRectanglePixels.x / sourceTexture.width;
            float uvY = sourceRectanglePixels.y / sourceTexture.height;
            float uvWidth = sourceRectanglePixels.width / sourceTexture.width;
            float uvHeight = sourceRectanglePixels.height / sourceTexture.height;

            RenderTexture.active = temporaryRenderTexture;

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, targetWidth, targetHeight, 0);
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            
            Rect destinationRectangle = new Rect(0, 0, targetWidth, targetHeight);
            Rect uvRectangle = new Rect(uvX, uvY, uvWidth, uvHeight);

            Graphics.DrawTexture(destinationRectangle, sourceTexture, uvRectangle, 0, 0, 0, 0);
            GL.PopMatrix();
            
            Texture2D outputTexture = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false, false);
            outputTexture.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0, false);
            outputTexture.Apply(false, false);

            return outputTexture;
        }
        finally
        {
            RenderTexture.active = previousActiveRenderTexture;
            RenderTexture.ReleaseTemporary(temporaryRenderTexture);
        }
    }

    private void ApplySpritePreviewSizing(string key)
    {
        if (spritePreviewImage == null)
            return;

        if (key.StartsWith(GroupNameKeys.KEY_GROUP_UI, StringComparison.OrdinalIgnoreCase))
        {
            spritePreviewImage.preserveAspect = true;
            RectTransform spriteTransform = spritePreviewImage.rectTransform;
            spriteTransform.sizeDelta = new Vector2(UI_SPRITE_MAX_SIZE, UI_SPRITE_MAX_SIZE);
        }
    }
}