using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class AddressablesAssetLoader
{
    private readonly AddressablesDiagnostics addressablesDiagnostics;
    private readonly Dictionary<string, AsyncOperationHandle> assetHandlesByKey =
        new Dictionary<string, AsyncOperationHandle>(256);
    private readonly HashSet<string> loadedAssetKeys = new HashSet<string>();
    private readonly Dictionary<object, Task> inflightDownloads = new Dictionary<object, Task>();

    [Inject]
    public AddressablesAssetLoader(AddressablesDiagnostics addressablesDiagnostics)
    {
        this.addressablesDiagnostics = addressablesDiagnostics;
    }

    public IReadOnlyCollection<string> LoadedAssetKeys => loadedAssetKeys;

    public async Task<TAsset> LoadAssetAsync<TAsset>(string assetKey) where TAsset : class
    {
        if (!string.IsNullOrWhiteSpace(assetKey) && assetHandlesByKey.TryGetValue(assetKey, out var existingHandle))
        {
            if (existingHandle.IsValid())
            {
                if (existingHandle.Status == AsyncOperationStatus.Succeeded)
                    return existingHandle.Result as TAsset;
                
                Addressables.Release(existingHandle);
            }

            assetHandlesByKey.Remove(assetKey);
        }

        long expectedBytes = await GetExpectedDownloadBytesAsync(assetKey);

        if (expectedBytes > 0)
            await EnsureDependenciesDownloaded(assetKey);

        addressablesDiagnostics.StartTimer(assetKey);

        var loadHandle = Addressables.LoadAssetAsync<TAsset>(assetKey);

        try
        {
            await loadHandle.Task;
        }
        catch
        {
            if (loadHandle.IsValid())
                Addressables.Release(loadHandle);
            
            return null;
        }

        if (!loadHandle.IsValid() || loadHandle.Status != AsyncOperationStatus.Succeeded)
        {
            if (loadHandle.IsValid())
                Addressables.Release(loadHandle);
            
            return null;
        }

        assetHandlesByKey[assetKey] = loadHandle;
        loadedAssetKeys.Add(assetKey);
        addressablesDiagnostics.NotifyAssetLoaded(assetKey);

        return loadHandle.Result;
    }

    public async Task UnloadAllAssetsAsync()
    {
        foreach (var pair in assetHandlesByKey)
        {
            var handle = pair.Value;

            if (handle.IsValid())
                Addressables.Release(handle);
        }

        assetHandlesByKey.Clear();

        foreach (var key in loadedAssetKeys)
            addressablesDiagnostics.NotifyAssetUnloaded(key);

        loadedAssetKeys.Clear();

        AsyncOperation unloadOperation = Resources.UnloadUnusedAssets();
        
        await AwaitAsyncOperation(unloadOperation);

        GC.Collect();
    }

    public async Task<long> GetExpectedDownloadBytesAsync(object keyOrLabel)
    {
        var sizeHandle = Addressables.GetDownloadSizeAsync(keyOrLabel);
        long bytes = 0;

        try
        {
            bytes = await sizeHandle.Task;
        }
        finally
        {
            if (sizeHandle.IsValid())
                Addressables.Release(sizeHandle);
        }

        return bytes;
    }

    public async Task<bool> EnsureDependenciesDownloaded(object keyOrLabel)
    {
        if (inflightDownloads.TryGetValue(keyOrLabel, out var existingTask))
        {
            await existingTask;
            
            return true;
        }

        var completionSource = new TaskCompletionSource<bool>();
        inflightDownloads[keyOrLabel] = completionSource.Task;

        try
        {
            var sizeHandle = Addressables.GetDownloadSizeAsync(keyOrLabel);
            long expectedBytes = 0;

            try
            {
                expectedBytes = await sizeHandle.Task;
            }
            finally
            {
                if (sizeHandle.IsValid())
                    Addressables.Release(sizeHandle);
            }

            if (expectedBytes <= 0)
            {
                completionSource.SetResult(true);
                
                return true;
            }

            var downloadHandle = Addressables.DownloadDependenciesAsync(keyOrLabel, true);

            try
            {
                while (downloadHandle.IsValid() && !downloadHandle.IsDone)
                    await Task.Yield();

                completionSource.SetResult(true);
                
                return true;
            }
            finally
            {
                if (downloadHandle.IsValid())
                    Addressables.Release(downloadHandle);
            }
        }
        catch (Exception exception)
        {
            completionSource.SetException(exception);
            
            throw;
        }
        finally
        {
            inflightDownloads.Remove(keyOrLabel);
        }
    }

    private Task AwaitAsyncOperation(AsyncOperation operation)
    {
        var completionSource = new TaskCompletionSource<bool>();
        
        operation.completed += _ => completionSource.SetResult(true);
        
        return completionSource.Task;
    }
}