using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;
using Debug = UnityEngine.Debug;

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
        if (assetHandlesByKey.ContainsKey(assetKey) && assetHandlesByKey[assetKey].IsValid())
            return assetHandlesByKey[assetKey].Result as TAsset;

        long expectedBytes = await GetExpectedDownloadBytesAsync(assetKey);

        if (expectedBytes > 0)
            await EnsureDependenciesDownloaded(assetKey);

        addressablesDiagnostics.StartTimer(assetKey);

        var loadHandle = Addressables.LoadAssetAsync<TAsset>(assetKey);
        
        await loadHandle.Task;

        assetHandlesByKey[assetKey] = loadHandle;
        loadedAssetKeys.Add(assetKey);
        addressablesDiagnostics.NotifyAssetLoaded(assetKey);

        return loadHandle.Result;
    }

    public async Task UnloadAllAssetsAsync()
    {
        foreach (var pair in assetHandlesByKey)
        {
            if (pair.Value.IsValid())
                Addressables.Release(pair.Value);
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
        long bytes = await sizeHandle.Task;
        
        Addressables.Release(sizeHandle);
        
        return bytes;
    }

    public async Task<bool> EnsureDependenciesDownloaded(object keyOrLabel)
    {
        if (inflightDownloads.TryGetValue(keyOrLabel, out var existing))
        {
            await existing;
            
            return true;
        }

        var source = new TaskCompletionSource<bool>();
        inflightDownloads[keyOrLabel] = source.Task;

        try
        {
            var sizeH = Addressables.GetDownloadSizeAsync(keyOrLabel);
            long expectedBytes = await sizeH.Task;
            Addressables.Release(sizeH);

            if (expectedBytes <= 0)
            {
                source.SetResult(true);
                
                return true;
            }
            
            var downloadDependencies = Addressables.DownloadDependenciesAsync(keyOrLabel, true);

            long last = -1;
            
            while (!downloadDependencies.IsDone)
            {
                var status = downloadDependencies.GetDownloadStatus();
                
                if (status.TotalBytes > 0 && status.DownloadedBytes != last)
                    last = status.DownloadedBytes;
                
                await Task.Yield();
            }
            
            source.SetResult(true);
            
            return true;
        }
        catch (Exception e)
        {
            source.SetException(e);
            
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