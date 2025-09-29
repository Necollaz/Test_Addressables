using System;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class AddressablesInitializer
{
    private readonly TimeSpan minInterval = TimeSpan.FromSeconds(5);
    
    private DateTime _lastCheckUtc = DateTime.MinValue;
    
    public async Task InitializeAndUpdateCatalogs() => await RefreshCatalogIfNeeded(false);

    public async Task<bool> RefreshCatalogIfNeeded(bool clearMutableCache)
    {
        if (DateTime.UtcNow - _lastCheckUtc < minInterval)
            return false;

        await Addressables.InitializeAsync().Task;

        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        var catalogs = await checkHandle.Task;
        Addressables.Release(checkHandle);

        bool updated = catalogs != null && catalogs.Count > 0;

        if (updated)
        {
            var updateHandle = Addressables.UpdateCatalogs(catalogs);
            await updateHandle.Task;
            Addressables.Release(updateHandle);

            if (clearMutableCache)
            {
                var clear = Addressables.ClearDependencyCacheAsync("mutable_content", true);
                await clear.Task;
                Addressables.Release(clear);
            }
        }

        _lastCheckUtc = DateTime.UtcNow;
        
        return updated;
    }
}