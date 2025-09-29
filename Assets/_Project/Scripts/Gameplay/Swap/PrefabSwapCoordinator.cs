using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class PrefabSwapCoordinator : BaseSwapCoordinator<SwappablePrefabAnchor, GameObject>
{
    [Inject]
    public PrefabSwapCoordinator(AddressablesAssetLoader assetLoader, AssetTypeProbe probe, AddressableKeyNormalizer keyNormalizer)
        : base(assetLoader, probe, keyNormalizer) { }

    protected override async Task<bool> CanLoadAsset(string key)
    {
        var (_, hasPrefab) = await TypeProbe.ProbeExactAsync(KeyNormalizer.Normalize(key));
        
        return hasPrefab;
    }
}