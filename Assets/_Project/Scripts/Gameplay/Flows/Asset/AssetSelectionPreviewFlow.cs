using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class AssetSelectionPreviewFlow
{
    private const float UI_SPRITE_MAX_SIZE = 250f;

    private readonly AllowedKeyFilter filter;
    private readonly AssetTypeProbe typeProbe;
    private readonly AddressableKeyNormalizer keyNormalizer;
    private readonly PrefabLoadAndPreview prefabFlow;
    private readonly PrefabPreviewRenderer prefabPreviewRenderer;
    private readonly PreviewPanelSwitcher panelSwitcher;
    private readonly Image spritePreviewImage;

    public AssetSelectionPreviewFlow(AllowedKeyFilter filter, AssetTypeProbe typeProbe, AddressableKeyNormalizer keyNormalizer,
        PrefabLoadAndPreview prefabFlow, PrefabPreviewRenderer prefabPreviewRenderer, PreviewPanelSwitcher panelSwitcher,
        Image spritePreviewImage)
    {
        this.filter = filter;
        this.typeProbe = typeProbe;
        this.keyNormalizer = keyNormalizer;
        this.prefabFlow = prefabFlow;
        this.prefabPreviewRenderer = prefabPreviewRenderer;
        this.panelSwitcher = panelSwitcher;
        this.spritePreviewImage = spritePreviewImage;
    }

    public async Task OnAssetKeySelectedAsync(string key, AddressablesAssetLoader assetLoader, Sprite loadedPrefabIconSprite, Image loadedPrefabIconImage)
    {
        if (string.IsNullOrWhiteSpace(key) || !filter.IsAllowed(key))
        {
            panelSwitcher.TryHideAll();
            
            return;
        }

        var probeKey = keyNormalizer.Normalize(key);
        var (hasSprite, hasPrefab) = await typeProbe.ProbeExactAsync(probeKey);

        if (hasSprite)
        {
            Sprite sprite = await assetLoader.LoadAssetAsync<Sprite>(key);
            
            if (spritePreviewImage != null)
            {
                if (key.StartsWith(GroupNameKeys.KEY_GROUP_UI, System.StringComparison.OrdinalIgnoreCase))
                {
                    spritePreviewImage.preserveAspect = true;
                    spritePreviewImage.rectTransform.sizeDelta = new Vector2(UI_SPRITE_MAX_SIZE, UI_SPRITE_MAX_SIZE);
                }
                
                spritePreviewImage.sprite = sprite;
                
                if (!key.StartsWith(GroupNameKeys.KEY_GROUP_UI, System.StringComparison.OrdinalIgnoreCase))
                    spritePreviewImage.SetNativeSize();
            }
            
            panelSwitcher.TryShowSpriteOnly();
            
            if (loadedPrefabIconImage != null)
                loadedPrefabIconImage.enabled = false;
            
            return;
        }

        if (hasPrefab)
        {
            prefabPreviewRenderer?.ApplyCameraOverrideForKey(key);
            await prefabFlow.TryLoadAndShowAsync(key, probeKey);
            panelSwitcher.TryShowPrefabOnly();

            if (loadedPrefabIconImage != null)
            {
                loadedPrefabIconImage.sprite = loadedPrefabIconSprite;
                loadedPrefabIconImage.enabled = loadedPrefabIconSprite != null;
            }
            return;
        }

        panelSwitcher.TryHideAll();
    }
}