using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class SpriteSwapCoordinator : BaseSwapCoordinator<SwappableSpriteAnchor, Sprite>
{
    [Inject]
    public SpriteSwapCoordinator(AddressablesAssetLoader assetLoader, AssetTypeProbe probe, AddressableKeyNormalizer keyNormalizer)
        : base(assetLoader, probe, keyNormalizer) { }

    protected override async Task<bool> CanLoadAsset(string key)
    {
        var (hasSprite, _) = await TypeProbe.ProbeExactAsync(KeyNormalizer.Normalize(key));
        
        return hasSprite;
    }
}