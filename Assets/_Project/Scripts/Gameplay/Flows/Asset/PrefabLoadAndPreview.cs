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
        Debug.Log($"[PrefabFlow] TryLoad '{assetKey}' (probeKey='{probeKey}')");

        long expectedBytes = await loader.GetExpectedDownloadBytesAsync(probeKey);
        Debug.Log($"[PrefabFlow] Expected bytes: {expectedBytes}");

        if (expectedBytes > 0)
        {
            Debug.Log("[PrefabFlow] Downloading dependencies…");
            await loader.EnsureDependenciesDownloaded(probeKey);
        }

        GameObject prefab = await loader.LoadAssetAsync<GameObject>(assetKey);
        Debug.Log($"[PrefabFlow] LoadAsset status: {(prefab ? "OK" : "NULL")}");

        if (prefab == null)
            return false;

        if (_spawnedInstance != null)
        {
            Object.Destroy(_spawnedInstance);
            _spawnedInstance = null;
        }

        if (alsoInstantiateIntoScene && sceneSpawnRoot != null)
        {
            _spawnedInstance = Object.Instantiate(prefab, sceneSpawnRoot);
            Debug.Log("[PrefabFlow] Spawned instance in scene");
        }

        previewRenderer?.TryShowPrefab(prefab);
        Debug.Log("[PrefabFlow] Sent to preview renderer");

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