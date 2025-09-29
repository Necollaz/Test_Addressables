using UnityEngine;
using UnityEngine.UI;

public class PreviewPanelSwitcher
{
    private readonly PrefabPreviewRenderer prefabPreviewRenderer;
    private readonly Image spritePreviewImage;
    private readonly GameObject gameObjectPreviewContainer;

    public PreviewPanelSwitcher(Image spritePreviewImage, GameObject gameObjectPreviewContainer, PrefabPreviewRenderer prefabPreviewRenderer)
    {
        this.spritePreviewImage = spritePreviewImage;
        this.gameObjectPreviewContainer = gameObjectPreviewContainer;
        this.prefabPreviewRenderer = prefabPreviewRenderer;
    }

    public void TryShowSpriteOnly()
    {
        SetActive(spritePreviewImage?.gameObject, true);
        SetActive(gameObjectPreviewContainer, false);
        prefabPreviewRenderer?.Clear();
    }

    public void TryShowPrefabOnly()
    {
        SetActive(spritePreviewImage?.gameObject, false);
        SetActive(gameObjectPreviewContainer, true);
    }

    public void TryHideAll()
    {
        SetActive(spritePreviewImage?.gameObject, false);
        SetActive(gameObjectPreviewContainer, false);
        prefabPreviewRenderer?.Clear();
    }

    private void SetActive(GameObject instance, bool active)
    {
        if (instance != null)
            instance.SetActive(active);
    }
}