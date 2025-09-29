using System.Threading.Tasks;
using UnityEngine;

public class PrefabLoadAndPreview
{
    private readonly AddressablesAssetLoader loader;
    private readonly PrefabPreviewRenderer previewRenderer;
    private readonly Transform sceneSpawnRoot;
    private readonly bool alsoInstantiateIntoScene;

    private GameObject _spawnedInstance;

    public PrefabLoadAndPreview(AddressablesAssetLoader loader, PrefabPreviewRenderer previewRenderer, Transform sceneSpawnRoot,
        bool alsoInstantiateIntoScene)
    {
        this.loader = loader;
        this.previewRenderer = previewRenderer;
        this.sceneSpawnRoot = sceneSpawnRoot;
        this.alsoInstantiateIntoScene = alsoInstantiateIntoScene;
    }

    public async Task<bool> TryLoadAndShowAsync(string assetKey, object probeKey)
    {
        long expectedBytes = await loader.GetExpectedDownloadBytesAsync(probeKey);

        if (expectedBytes > 0)
            await loader.EnsureDependenciesDownloaded(probeKey);

        GameObject prefab = await loader.LoadAssetAsync<GameObject>(assetKey);

        if (prefab == null)
            return false;

        if (_spawnedInstance != null)
        {
            Object.Destroy(_spawnedInstance);
            _spawnedInstance = null;
        }

        if (alsoInstantiateIntoScene && sceneSpawnRoot != null)
            _spawnedInstance = Object.Instantiate(prefab, sceneSpawnRoot);

        previewRenderer?.TryShowPrefab(prefab);

        return true;
    }

    public async Task TryUnloadAllAsync()
    {
        if (_spawnedInstance != null)
        {
            Object.Destroy(_spawnedInstance);
            
            _spawnedInstance = null;
        }
        
        await loader.UnloadAllAssetsAsync();
        
        previewRenderer?.Clear();
    }
}