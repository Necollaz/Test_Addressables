using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class AddressablesSceneLoader
{
    private AsyncOperationHandle<SceneInstance>? _currentSceneHandle;
    private float _currentProgress;

    public float CurrentSceneLoadingProgress => _currentProgress;

    public async Task TryLoadScene(string sceneKey, LoadSceneMode mode)
    {
        if (sceneKey == null)
            throw new ArgumentNullException(nameof(sceneKey));

        var stopwatch = Stopwatch.StartNew();
        var previous = _currentSceneHandle;
        var load = Addressables.LoadSceneAsync(sceneKey, mode, activateOnLoad: true);

        while (!load.IsDone)
        {
            _currentProgress = load.PercentComplete;
            await Task.Yield();
        }

        if (load.Status != AsyncOperationStatus.Succeeded)
        {
            if (load.IsValid())
                Addressables.Release(load);
            
            _currentSceneHandle = null;
            _currentProgress = 0f;
            
            throw load.OperationException ?? new InvalidKeyException($"Key '{sceneKey}' is not a Scene.");
        }

        _currentSceneHandle = load;
        _currentProgress = 1f;

        if (previous.HasValue && previous.Value.IsValid())
        {
            try
            {
                var unloadPreview = Addressables.UnloadSceneAsync(previous.Value, true);
                
                while (!unloadPreview.IsDone)
                    await Task.Yield();
                
                Addressables.Release(previous.Value);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SceneLoader] Unload previous scene failed: {exception.Message}");
            }
        }

        stopwatch.Stop();
    }

    public async Task LoadSceneWithExplicitActivation(string sceneKey, LoadSceneMode mode)
    {
        var load = Addressables.LoadSceneAsync(sceneKey, mode, activateOnLoad: false);

        while (load.PercentComplete < 0.9f)
        {
            await Task.Yield();
        }
        
        var activate = load.Result.ActivateAsync();

        while (!activate.isDone)
            await Task.Yield();

        if (load.Status != AsyncOperationStatus.Succeeded)
        {
            if (load.IsValid())
                Addressables.Release(load);
            
            throw load.OperationException ?? new Exception("Activation failed");
        }
    }
}