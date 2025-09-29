using System.Threading.Tasks;
using UnityEngine.UI;

public class AssetButtonsAvailabilityUpdater
{
    private readonly AssetTypeProbe typeProbe;
    private readonly AddressableKeyNormalizer keyNormalizer;
    private readonly Button loadSpriteButton;
    private readonly Button loadPrefabButton;

    public AssetButtonsAvailabilityUpdater(AssetTypeProbe typeProbe, Button loadSpriteButton, Button loadPrefabButton,
        AddressableKeyNormalizer keyNormalizer)
    {
        this.typeProbe = typeProbe;
        this.loadSpriteButton = loadSpriteButton;
        this.loadPrefabButton = loadPrefabButton;
        this.keyNormalizer = keyNormalizer;
    }

    public async Task UpdateForKeyAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            SetButtonsInteractable(false, false);
            
            return;
        }

        var probeKey = keyNormalizer.Normalize(key);
        var (hasSprite, hasPrefab) = await typeProbe.ProbeExactAsync(probeKey);
        SetButtonsInteractable(hasSprite, hasPrefab);
    }

    public void SetButtonsInteractable(bool spriteEnabled, bool prefabEnabled)
    {
        if (loadSpriteButton != null)
            loadSpriteButton.interactable = spriteEnabled;
        
        if (loadPrefabButton != null)
            loadPrefabButton.interactable = prefabEnabled;
    }
}